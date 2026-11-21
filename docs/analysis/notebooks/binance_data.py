"""Download and validate daily candles from the public Binance market data API.

Follows docs/requirements/wymagania-i-zakres.md, chapters 7 and 10: only closed
candles are kept, long periods are fetched in pages of 1000 candles, invalid
records are dropped and missing days of the requested period are reported,
never filled in.
Downloaded data is kept in memory only and is not saved to the repository.
"""

from __future__ import annotations

import json
import time
import urllib.error
import urllib.request
from datetime import date, datetime, timedelta, timezone

import numpy as np
import pandas as pd

BASE_URLS = ("https://data-api.binance.vision", "https://api.binance.com")
DAY_MS = 86_400_000
PAGE_LIMIT = 1000
COLUMNS = [
    "open_time", "open", "high", "low", "close", "volume",
    "close_time", "quote_volume", "trades", "taker_base_volume", "taker_quote_volume", "ignore",
]
PRICE_COLUMNS = ["open", "high", "low", "close"]
NUMERIC_COLUMNS = PRICE_COLUMNS + ["volume", "quote_volume", "trades", "open_time", "close_time"]


def _unix_ms(day: date) -> int:
    return int(datetime(day.year, day.month, day.day, tzinfo=timezone.utc).timestamp() * 1000)


def _get_json(path: str):
    """GET with retries; tries the fallback host when the primary one keeps failing."""
    last_error: Exception | None = None
    for base_url in BASE_URLS:
        for attempt in range(3):
            request = urllib.request.Request(base_url + path, headers={"User-Agent": "crypto-market-analytics-notebooks"})
            try:
                with urllib.request.urlopen(request, timeout=30) as response:
                    return json.load(response)
            except urllib.error.HTTPError as error:
                last_error = error
                if error.code in (418, 429):
                    # Rate limit: wait as long as Retry-After says, then try again.
                    time.sleep(min(int(error.headers.get("Retry-After", "5")), 60))
                    continue
                if 400 <= error.code < 500:
                    raise
                time.sleep(2 ** attempt)
            except (urllib.error.URLError, TimeoutError) as error:
                last_error = error
                time.sleep(2 ** attempt)
    raise RuntimeError(f"Binance API is unavailable: {last_error}")


def fetch_daily_klines(symbol: str, start: date, end: date | None = None) -> pd.DataFrame:
    """Closed daily candles for start..end inclusive (UTC days). end defaults to yesterday.

    The requested days are stored in attrs, so that validate() can report days
    missing at the beginning and at the end of the period.
    """
    now = datetime.now(timezone.utc)
    if end is None:
        end = now.date() - timedelta(days=1)
    start_ms = _unix_ms(start)
    end_ms = _unix_ms(end + timedelta(days=1)) - 1

    rows: list[list] = []
    while start_ms <= end_ms:
        page = _get_json(
            f"/api/v3/klines?symbol={symbol}&interval=1d&startTime={start_ms}&endTime={end_ms}&limit={PAGE_LIMIT}"
        )
        if not page:
            break
        rows.extend(page)
        if len(page) < PAGE_LIMIT:
            break
        start_ms = int(page[-1][0]) + DAY_MS

    frame = pd.DataFrame(rows, columns=COLUMNS)
    # A value that is not a number becomes NaN here and the record is rejected in validate().
    for column in NUMERIC_COLUMNS:
        frame[column] = pd.to_numeric(frame[column], errors="coerce").astype(float)
    # The candle of the current day is not closed yet: its close time is in the future.
    frame = frame[~(frame["close_time"] >= now.timestamp() * 1000)].copy()
    # The day is taken from the open time; validate() checks that it is exactly 00:00 UTC.
    frame["date"] = pd.to_datetime(frame["open_time"], unit="ms", errors="coerce").dt.floor("D")
    frame = frame.set_index("date")
    frame.attrs.update(symbol=symbol, requested_start=start, requested_end=end)
    return frame[PRICE_COLUMNS + ["volume", "quote_volume", "trades", "open_time", "close_time"]]


def validate(frame: pd.DataFrame, big_move: float = 0.5,
             start: date | None = None, end: date | None = None) -> tuple[pd.DataFrame, dict]:
    """Drop invalid records and report data quality.

    A record is invalid when any of its numbers is missing or not finite, the
    candle does not open at 00:00 UTC or does not last exactly one day, a price
    is not positive, low <= open/close <= high does not hold, the volume is
    negative or the date is outside the requested period. Of duplicated dates
    the first valid record is kept. Missing days are reported, not filled in,
    and they are counted over the requested period (start..end, by default
    taken from fetch_daily_klines), so a shortened history is also detected.
    Daily close-to-close changes above big_move are flagged for manual review,
    not removed.
    """
    start = start or frame.attrs.get("requested_start")
    end = end or frame.attrs.get("requested_end")
    numbers = frame[[c for c in NUMERIC_COLUMNS if c in frame.columns]].apply(pd.to_numeric, errors="coerce")
    numbers = numbers.astype(float)

    not_finite = ~np.isfinite(numbers).all(axis=1)
    no_date = frame.index.isna()
    open_time, close_time = numbers["open_time"], numbers["close_time"]
    wrong_time = (open_time % DAY_MS != 0) | (close_time != open_time + DAY_MS - 1)
    body_low = numbers[["open", "close"]].min(axis=1)
    body_high = numbers[["open", "close"]].max(axis=1)
    inconsistent = (numbers["low"] > body_low) | (numbers["high"] < body_high)
    non_positive = (numbers[PRICE_COLUMNS] <= 0).any(axis=1)
    negative_volume = (numbers[["volume", "quote_volume"]] < 0).any(axis=1)
    outside = pd.Series(False, index=frame.index)
    if start is not None:
        outside |= frame.index < pd.Timestamp(start)
    if end is not None:
        outside |= frame.index > pd.Timestamp(end)
    invalid = (not_finite | no_date | wrong_time | inconsistent | non_positive | negative_volume | outside).to_numpy()

    candidates = frame[~invalid]
    duplicated_valid = candidates.index.duplicated(keep="first")
    clean = candidates[~duplicated_valid].sort_index()
    clean.attrs = dict(frame.attrs)

    if start is not None and end is not None:
        calendar = pd.date_range(pd.Timestamp(start), pd.Timestamp(end), freq="D")
    elif len(clean):
        calendar = pd.date_range(clean.index.min(), clean.index.max(), freq="D")
    else:
        calendar = pd.DatetimeIndex([])
    close = clean["close"].reindex(calendar) if len(calendar) else clean["close"]
    change = close / close.shift(1) - 1  # consecutive days only, a gap gives NaN
    flagged = change[change.abs() > big_move]

    report = {
        "symbol": frame.attrs.get("symbol", ""),
        "rows": len(frame),
        "expected_days": len(calendar),
        "valid_days": len(clean),
        "sorted_by_date": bool(frame.index.is_monotonic_increasing),
        "duplicated_dates": sorted({d.date() for d in frame.index[frame.index.duplicated(keep=False)] if pd.notna(d)}),
        "invalid_records": [d.date() if pd.notna(d) else None for d in frame.index[invalid]],
        "missing_days": [d.date() for d in calendar.difference(clean.index)],
        "big_moves": {d.date(): round(float(v), 4) for d, v in flagged.items()},
        "first_day": clean.index.min().date() if len(clean) else None,
        "last_day": clean.index.max().date() if len(clean) else None,
    }
    return clean, report
