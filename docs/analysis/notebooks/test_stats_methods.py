"""Tests of stats_methods.py and binance_data.py on synthetic data, without network access.

Covers the test data from chapter 11 of docs/analysis/metody-statystyczne.md and
the edge cases: a missing day, a zero return, NaN and infinite values, a constant
price, an empty sample and invalid candles.

Run from this directory:  python -m unittest -v test_stats_methods
"""

from __future__ import annotations

import math
import unittest
import warnings
from datetime import date, datetime, timezone
from unittest import mock

import numpy as np
import pandas as pd

import binance_data as bd
import stats_methods as sm

TOLERANCE = 5.5e-11
DAY_MS = bd.DAY_MS


def series(values, start="2026-01-01"):
    return pd.Series(values, index=pd.date_range(start, periods=len(values), freq="D"), dtype=float)


def dated(pairs):
    return pd.Series([v for _, v in pairs], index=pd.to_datetime([d for d, _ in pairs]), dtype=float)


EMPTY = pd.Series([], index=pd.DatetimeIndex([]), dtype=float)


class ChapterElevenTest(unittest.TestCase):
    """Values from chapter 11.1 and 11.2 of the methods document."""

    def setUp(self):
        self.prices = series([100, 110, 99, 108.9, 102.366])

    def test_returns_and_descriptive(self):
        log = sm.returns(self.prices, "log")
        self.assertEqual(len(log), 4)
        self.assertAlmostEqual(log.sum(), 0.0233844402, delta=TOLERANCE)
        d = sm.descriptive(self.prices)
        self.assertAlmostEqual(d["mean"], 0.01, delta=TOLERANCE)
        self.assertAlmostEqual(d["median"], 0.02, delta=TOLERANCE)
        self.assertAlmostEqual(d["total_return"], 0.02366, delta=TOLERANCE)
        self.assertAlmostEqual(d["up_share"], 0.5)
        self.assertIsNone(d["reason"])

    def test_volatility_shape_drawdown_sma(self):
        v = sm.volatility(self.prices)
        self.assertAlmostEqual(v["daily"], 0.1048185044, delta=TOLERANCE)
        self.assertAlmostEqual(v["annual"], 2.0025547144, delta=TOLERANCE)
        s = sm.shape(self.prices)
        self.assertAlmostEqual(s["skewness"], -0.1468980258, delta=TOLERANCE)
        self.assertAlmostEqual(s["kurtosis"], -5.1579652449, delta=TOLERANCE)
        _, dd = sm.drawdown(self.prices)
        self.assertAlmostEqual(dd["max_drawdown"], -0.10, delta=TOLERANCE)
        self.assertIsNone(dd["recovery_date"])
        self.assertAlmostEqual(sm.sma(self.prices, 3).iloc[-1], 103.422, delta=TOLERANCE)

    def test_correlation_beta(self):
        days = pd.date_range("2026-01-02", periods=4, freq="D")
        m = sm.correlation_beta(pd.Series([0.10, -0.10, 0.10, -0.06], index=days),
                                pd.Series([0.02, -0.01, 0.03, -0.02], index=days))
        self.assertAlmostEqual(m["pearson"], 0.9317610191, delta=TOLERANCE)
        self.assertAlmostEqual(m["beta"], 4.1176470588, delta=TOLERANCE)


class EdgeCaseTest(unittest.TestCase):
    """Cases from chapter 11.3 of the methods document."""

    def test_zero_return_is_left_out_of_up_share(self):
        d = sm.descriptive(series([100, 110, 110, 99]))
        self.assertEqual(d["returns"], 3)
        self.assertEqual((d["up_days"], d["down_days"], d["zero_days"]), (1, 1, 1))
        self.assertAlmostEqual(d["up_share"], 0.5)

    def test_gap_drops_returns_and_moving_averages(self):
        prices = dated([("2026-01-01", 100), ("2026-01-02", 110), ("2026-01-04", 121)])
        self.assertEqual(len(sm.returns(prices)), 1)
        d = sm.descriptive(prices)
        self.assertTrue(math.isnan(d["mean"]))
        self.assertIsNotNone(d["reason"])
        self.assertAlmostEqual(d["total_return"], 0.21)
        self.assertEqual(d["calendar_days"], 3)
        self.assertTrue(math.isnan(sm.sma(prices, 3).loc["2026-01-04"]))
        self.assertTrue(math.isnan(sm.ema(prices, 3).loc["2026-01-04"]))

    def test_ema_restarts_after_gap(self):
        prices = dated([("2026-01-01", 10), ("2026-01-02", 12), ("2026-01-03", 14),
                        ("2026-01-05", 20), ("2026-01-06", 22), ("2026-01-07", 25)])
        result = sm.ema(prices, 2)
        self.assertTrue(math.isnan(result.loc["2026-01-01"]))
        self.assertAlmostEqual(result.loc["2026-01-02"], 11)
        self.assertAlmostEqual(result.loc["2026-01-03"], 2 / 3 * 14 + 1 / 3 * 11)
        self.assertTrue(math.isnan(result.loc["2026-01-05"]))
        self.assertAlmostEqual(result.loc["2026-01-06"], 21)
        self.assertAlmostEqual(result.loc["2026-01-07"], 2 / 3 * 25 + 1 / 3 * 21)
        sma = sm.sma(prices, 2)
        self.assertTrue(math.isnan(sma.loc["2026-01-05"]))
        self.assertAlmostEqual(sma.loc["2026-01-06"], 21)

    def test_rolling_volatility_does_not_bridge_gap(self):
        prices = dated([("2026-01-01", 100), ("2026-01-02", 101), ("2026-01-03", 99),
                        ("2026-01-05", 100), ("2026-01-06", 102)])
        result = sm.rolling_volatility(prices, 2)
        self.assertFalse(math.isnan(result.loc["2026-01-03"]))
        self.assertTrue(result.loc["2026-01-04":"2026-01-06"].isna().all())

    def test_nan_and_infinite_prices_are_not_counted(self):
        for bad in (math.nan, math.inf):
            prices = series([100, bad, 110, 121, 133.1])
            simple = sm.returns(prices, "simple")
            self.assertEqual(len(simple), 2)
            self.assertTrue(np.allclose(simple.to_numpy(), [0.1, 0.1]))
            d = sm.descriptive(prices)
            self.assertEqual((d["prices"], d["returns"]), (4, 2))
            self.assertEqual(sm.volatility(prices)["returns"], 2)

    def test_constant_price(self):
        prices = series([100] * 5)
        d = sm.descriptive(prices)
        self.assertEqual(d["mean"], 0)
        self.assertEqual(d["zero_days"], 4)
        self.assertTrue(math.isnan(d["up_share"]))
        self.assertIsNotNone(d["up_share_reason"])
        self.assertEqual(sm.volatility(prices)["daily"], 0)
        s = sm.shape(prices)
        self.assertTrue(math.isnan(s["skewness"]) and math.isnan(s["kurtosis"]))
        self.assertEqual(s["skewness_reason"], sm.ZERO_VARIANCE)
        _, dd = sm.drawdown(prices)
        self.assertEqual(dd["max_drawdown"], 0)
        self.assertIsNone(dd["peak_date"])
        bins = sm.histogram(sm.returns(prices))
        self.assertEqual(int(bins["count"].sum()), 4)
        self.assertEqual(bins.attrs["reason"], sm.ZERO_VARIANCE)

    def test_constant_market(self):
        with warnings.catch_warnings():
            warnings.simplefilter("error")
            m = sm.market_relation(series([100, 110, 99, 108.9, 102.366]), series([100] * 5))
        self.assertTrue(math.isnan(m["beta"]) and math.isnan(m["pearson"]))
        self.assertIsNotNone(m["reason"])

    def test_empty_and_single_price(self):
        for prices in (EMPTY, series([100])):
            with warnings.catch_warnings():
                warnings.simplefilter("error")
                d = sm.descriptive(prices)
                v = sm.volatility(prices)
                line, dd = sm.drawdown(prices)
                s = sm.shape(prices)
                bins = sm.histogram(sm.returns(prices))
                m = sm.market_relation(prices, prices)
                rolling = sm.rolling_volatility(prices)
                averages = sm.sma(prices, 20), sm.ema(prices, 20)
            for value in (d["mean"], d["total_return"], v["daily"], dd["max_drawdown"],
                          s["skewness"], s["kurtosis"], m["beta"]):
                self.assertTrue(math.isnan(value))
            for reason in (d["reason"], v["reason"], dd["reason"], s["skewness_reason"],
                           s["kurtosis_reason"], bins.attrs["reason"], m["reason"]):
                self.assertIsInstance(reason, str)
            self.assertTrue(bins.empty)
            self.assertTrue(rolling.isna().all())
            self.assertTrue(all(a.isna().all() for a in averages))

    def test_cagr_needs_a_full_year(self):
        d = sm.descriptive(series(np.linspace(100, 120, 365)))
        self.assertEqual(d["calendar_days"], 364)
        self.assertTrue(math.isnan(d["cagr"]))
        self.assertIsNotNone(d["cagr_reason"])
        d = sm.descriptive(series(np.linspace(100, 120, 366)))
        self.assertAlmostEqual(d["cagr"], 0.2)

    def test_warning_below_reliable_sample(self):
        self.assertIsNotNone(sm.volatility(series([100, 101, 102, 101]))["warning"])
        self.assertIsNone(sm.volatility(series(100 + np.arange(40) % 3))["warning"])

    def test_drawdown_uses_last_peak_before_trough(self):
        _, dd = sm.drawdown(series([100, 100, 90, 100]))
        self.assertAlmostEqual(dd["max_drawdown"], -0.10)
        self.assertEqual(dd["peak_date"], date(2026, 1, 2))
        self.assertEqual(dd["trough_date"], date(2026, 1, 3))
        self.assertEqual(dd["duration_days"], 1)
        self.assertEqual(dd["recovery_date"], date(2026, 1, 4))


def candles(days, start=date(2026, 1, 1), overrides=None):
    """Valid synthetic candles in the shape returned by fetch_daily_klines."""
    index = pd.date_range(pd.Timestamp(start), periods=days, freq="D")
    open_time = np.array([int(d.tz_localize("UTC").timestamp() * 1000) for d in index], dtype=float)
    frame = pd.DataFrame({
        "open": 100.0, "high": 110.0, "low": 90.0, "close": 105.0 + np.arange(days),
        "volume": 1000.0, "quote_volume": 100_000.0, "trades": 50.0,
        "open_time": open_time, "close_time": open_time + DAY_MS - 1,
    }, index=index)
    frame["high"] = frame[["high", "close"]].max(axis=1)
    for (row, column), value in (overrides or {}).items():
        frame.iloc[row, frame.columns.get_loc(column)] = value
    frame.attrs.update(symbol="TEST", requested_start=start, requested_end=index[-1].date())
    return frame


class ValidateTest(unittest.TestCase):

    def test_clean_data(self):
        clean, report = bd.validate(candles(5))
        self.assertEqual(len(clean), 5)
        self.assertEqual(report["missing_days"], [])
        self.assertEqual(report["invalid_records"], [])

    def test_nan_and_infinite_values_are_rejected(self):
        cases = {("close", 1): math.nan, ("volume", 2): math.nan, ("high", 3): math.inf}
        for (column, row), value in cases.items():
            frame = candles(5, overrides={(row, column): value})
            clean, report = bd.validate(frame)
            self.assertEqual(len(clean), 4, column)
            self.assertEqual(report["missing_days"], [frame.index[row].date()])

    def test_candle_with_wrong_open_hour_is_rejected(self):
        frame = candles(3)
        frame.iloc[1, frame.columns.get_loc("open_time")] += 3_600_000
        frame.iloc[1, frame.columns.get_loc("close_time")] += 3_600_000
        clean, report = bd.validate(frame)
        self.assertEqual(len(clean), 2)
        self.assertEqual(report["missing_days"], [date(2026, 1, 2)])

    def test_days_missing_at_both_ends_of_the_period(self):
        frame = candles(3, start=date(2026, 1, 3))
        clean, report = bd.validate(frame, start=date(2026, 1, 1), end=date(2026, 1, 7))
        self.assertEqual(report["expected_days"], 7)
        self.assertEqual(report["missing_days"],
                         [date(2026, 1, 1), date(2026, 1, 2), date(2026, 1, 6), date(2026, 1, 7)])

    def test_all_records_rejected(self):
        frame = candles(3)
        frame["close"] = math.nan
        clean, report = bd.validate(frame)
        self.assertTrue(clean.empty)
        self.assertIsNone(report["first_day"])
        self.assertEqual(len(report["missing_days"]), 3)

    def test_duplicate_keeps_first_valid_record(self):
        frame = candles(3)
        duplicate = frame.iloc[[1]].copy()
        frame.iloc[1, frame.columns.get_loc("close")] = math.nan
        frame = pd.concat([frame, duplicate])
        frame.attrs.update(candles(3).attrs)
        clean, report = bd.validate(frame)
        self.assertEqual(len(clean), 3)
        self.assertEqual(report["duplicated_dates"], [date(2026, 1, 2)])

    def test_gap_is_not_flagged_as_big_move(self):
        frame = candles(4)
        frame = frame.drop(frame.index[1])
        frame.attrs.update(candles(4).attrs)
        frame.iloc[1, frame.columns.get_loc("close")] = 1000.0
        frame.iloc[1, frame.columns.get_loc("high")] = 1000.0
        _, report = bd.validate(frame)
        self.assertEqual(list(report["big_moves"]), [date(2026, 1, 4)])


class FetchTest(unittest.TestCase):

    def test_non_numeric_value_does_not_stop_download(self):
        day = int(datetime(2026, 1, 1, tzinfo=timezone.utc).timestamp() * 1000)
        page = [
            [day, "100", "110", "90", "105", "1000", day + DAY_MS - 1, "100000", 50, "0", "0", "0"],
            [day + DAY_MS, "105", "110", "90", "abc", "1000", day + 2 * DAY_MS - 1, "100000", 50, "0", "0", "0"],
        ]
        with mock.patch.object(bd, "_get_json", return_value=page):
            frame = bd.fetch_daily_klines("TEST", date(2026, 1, 1), date(2026, 1, 3))
        self.assertEqual(len(frame), 2)
        self.assertTrue(math.isnan(frame["close"].iloc[1]))
        clean, report = bd.validate(frame)
        self.assertEqual(len(clean), 1)
        self.assertEqual(report["missing_days"], [date(2026, 1, 2), date(2026, 1, 3)])


if __name__ == "__main__":
    unittest.main()
