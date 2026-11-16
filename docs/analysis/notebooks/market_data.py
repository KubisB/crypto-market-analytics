"""Download and validate closed daily Binance candles without writing market data."""

import json
from urllib.parse import urlencode
from urllib.request import urlopen

import numpy as np
import pandas as pd

BASE = "https://data-api.binance.vision/api/v3/klines"
DAY_MS = 86_400_000
COLUMNS = ["open_time", "open", "high", "low", "close", "volume", "close_time",
           "quote_volume", "trades", "taker_volume", "taker_quote_volume", "ignore"]


def daily_price_ratios(prices):
    """Return close-to-close ratios only for consecutive daily candles."""
    consecutive = prices.index.to_series().diff().eq(pd.Timedelta(days=1))
    return (prices / prices.shift()).where(consecutive)


def download(symbol, start="2022-06-15", now=None):
    """Return daily candles up to yesterday UTC and validation diagnostics."""
    now = pd.Timestamp.now(tz="UTC") if now is None else pd.Timestamp(now)
    if now.tzinfo is None:
        raise ValueError("now must be timezone-aware")
    today = now.tz_convert("UTC").normalize()
    start = pd.Timestamp(start, tz="UTC")
    first_open = int(start.timestamp() * 1000)
    cursor = first_open
    end = int(today.timestamp() * 1000) - 1
    rows = []
    while cursor <= end:
        query = urlencode(dict(symbol=symbol, interval="1d", startTime=cursor, endTime=end, limit=1000))
        with urlopen(f"{BASE}?{query}", timeout=30) as response:
            batch = json.load(response)
        if not isinstance(batch, list):
            raise ValueError(f"Unexpected Binance response: {batch}")
        if not batch:
            break
        rows.extend(batch)
        next_cursor = int(batch[-1][0]) + DAY_MS
        if next_cursor <= cursor:
            raise ValueError("Binance pagination did not advance")
        cursor = next_cursor
        if len(batch) < 1000:
            break
    if not rows:
        raise ValueError(f"No candles returned for {symbol}")
    df = pd.DataFrame(rows, columns=COLUMNS)
    for col in ["open_time", "close_time", "open", "high", "low", "close", "volume", "quote_volume"]:
        df[col] = pd.to_numeric(df[col], errors="coerce")
    df["date"] = pd.to_datetime(df.open_time, unit="ms", utc=True, errors="coerce")
    numeric = df[["open", "high", "low", "close", "volume", "quote_volume"]]
    valid = (np.isfinite(numeric).all(axis=1) & (df[["open", "high", "low", "close"]] > 0).all(axis=1)
             & (df[["volume", "quote_volume"]] >= 0).all(axis=1)
             & (df.low <= df[["open", "close"]].min(axis=1))
             & (df.high >= df[["open", "close"]].max(axis=1))
             & (df.low <= df.high) & (df.date == df.date.dt.normalize())
             & df.open_time.between(first_open, end - DAY_MS + 1)
             & (df.close_time == df.open_time + DAY_MS - 1))
    invalid = int((~valid).sum())
    df = df.loc[valid].copy()
    unordered = not df.date.is_monotonic_increasing
    duplicates = int(df.date.duplicated(keep=False).sum())
    # Ambiguous duplicates are excluded altogether, not silently selected.
    df = df.loc[~df.date.duplicated(keep=False)].sort_values("date").set_index("date")
    if df.empty:
        raise ValueError(f"No valid candles for {symbol}: {invalid} invalid and {duplicates} duplicate rows")
    expected = pd.date_range(start, today - pd.Timedelta(days=1), freq="D")
    missing = expected.difference(df.index)
    returns = daily_price_ratios(df.close) - 1
    outliers = returns[returns.abs() > 0.5]
    return df, {"invalid": invalid, "duplicate_rows": duplicates, "unordered": unordered,
                "missing_dates": missing, "outliers": outliers, "as_of": today - pd.Timedelta(days=1)}
