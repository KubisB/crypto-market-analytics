"""Statistical methods from docs/analysis/metody-statystyczne.md.

Every function uses the variant chosen in that document: n-1 divisor,
annualisation with sqrt(365), simple returns for the descriptive statistics,
log returns for volatility, distribution shape, correlation and beta,
linear-interpolation quantiles and bias-corrected skewness and kurtosis.

All series are pandas Series indexed by UTC day. Missing, non-finite and
non-positive prices are ignored, and a return is formed only from two valid
prices on consecutive calendar days. When a measure cannot be computed it is
NaN (or None for dates) and the result carries the reason, as required in
chapter 11.3 of docs/requirements/wymagania-i-zakres.md. Reasons and warnings
are user-facing texts, so they are in Polish.
"""

from __future__ import annotations

import math

import numpy as np
import pandas as pd
from scipy import stats

DAYS_PER_YEAR = 365

# Minimum and reliable sample sizes, docs/requirements/wymagania-i-zakres.md, chapter 11.3.
MIN_RETURNS = 2
MIN_SKEWNESS = 3
MIN_KURTOSIS = 4
MIN_PRICES_DRAWDOWN = 2
RELIABLE_VOLATILITY = 30
RELIABLE_SHAPE = 365
RELIABLE_HISTOGRAM = 30
RELIABLE_MARKET = 30
RELIABLE_EVENT_ESTIMATION = 30
RELIABLE_VOLUME = 90

NAN = math.nan


# ---------- messages ----------

def _plural(n: int, one: str, few: str, many: str) -> str:
    """Polish plural form for a count."""
    if n == 1:
        return one
    if 2 <= n % 10 <= 4 and not 12 <= n % 100 <= 14:
        return few
    return many


def _returns_text(n: int) -> str:
    return f"{n} {_plural(n, 'zwrot', 'zwroty', 'zwrotów')}"


def _prices_text(n: int) -> str:
    return f"{n} {_plural(n, 'cena', 'ceny', 'cen')}"


def _not_enough(required: str, available: str) -> str:
    return f"za mało danych: wymagane {required}, w okresie jest {available}"


def _unreliable(reliable: int, available: int) -> str | None:
    if available >= reliable:
        return None
    return f"wynik mało wiarygodny: zalecane co najmniej {_returns_text(reliable)}, w okresie jest {available}"


ZERO_VARIANCE = "wartość nieokreślona: zwroty w okresie się nie zmieniały"


# ---------- data preparation ----------

def valid_prices(prices: pd.Series) -> pd.Series:
    """Finite, positive prices sorted by date."""
    values = pd.to_numeric(prices, errors="coerce").astype(float).sort_index()
    return values[np.isfinite(values) & (values > 0)]


def _calendar(prices: pd.Series) -> pd.Series:
    """Valid prices on a full daily calendar; a missing day becomes NaN."""
    clean = valid_prices(prices)
    if clean.empty:
        return clean
    return clean.reindex(pd.date_range(clean.index.min(), clean.index.max(), freq="D"))


# ---------- returns (chapter 3) ----------

def returns(prices: pd.Series, kind: str = "log") -> pd.Series:
    """Daily returns, dated by the later day.

    A return between non-consecutive days spans a gap, would overstate volatility,
    and is therefore dropped from the daily statistics (chapter 2). Only finite
    returns are kept, so the length of the result is the number of valid returns.
    """
    clean = valid_prices(prices)
    ratio = clean / clean.shift(1)
    consecutive = clean.index.to_series().diff() == pd.Timedelta(days=1)
    values = np.log(ratio) if kind == "log" else ratio - 1
    keep = consecutive & np.isfinite(values)
    return values[keep].rename(f"{kind}_return")


# ---------- descriptive statistics (chapter 4) ----------

def descriptive(prices: pd.Series) -> dict:
    clean = valid_prices(prices)
    simple = returns(clean, "simple")
    result: dict = {
        "prices": len(clean),
        "returns": len(simple),
        "mean": NAN, "median": NAN,
        "worst": NAN, "worst_date": None,
        "best": NAN, "best_date": None,
        "up_share": NAN, "up_days": 0, "down_days": 0, "zero_days": 0,
        "total_return": NAN, "calendar_days": None, "cagr": NAN,
        "reason": None, "up_share_reason": None, "cagr_reason": None,
    }

    # Total return and CAGR are computed from prices, so gaps do not affect them.
    if len(clean) >= 2:
        calendar_days = (clean.index[-1] - clean.index[0]).days
        growth = clean.iloc[-1] / clean.iloc[0]
        result["calendar_days"] = calendar_days
        result["total_return"] = growth - 1
        if calendar_days >= DAYS_PER_YEAR:
            result["cagr"] = growth ** (DAYS_PER_YEAR / calendar_days) - 1
        else:
            result["cagr_reason"] = "liczony tylko dla okresów co najmniej rocznych"
    else:
        result["cagr_reason"] = _not_enough(_prices_text(2), str(len(clean)))

    if len(simple) < MIN_RETURNS:
        result["reason"] = _not_enough(_returns_text(MIN_RETURNS), str(len(simple)))
        result["up_share_reason"] = result["reason"]
        return result

    up, down = int((simple > 0).sum()), int((simple < 0).sum())
    result.update(
        mean=simple.mean(),
        median=simple.median(),
        worst=simple.min(),
        worst_date=simple.idxmin().date(),
        best=simple.max(),
        best_date=simple.idxmax().date(),
        up_days=up,
        down_days=down,
        zero_days=len(simple) - up - down,
    )
    # Days without a price change are left out of both the numerator and the denominator.
    if up + down > 0:
        result["up_share"] = up / (up + down)
    else:
        result["up_share_reason"] = "brak dni ze zmianą ceny"
    return result


# ---------- volatility (chapter 5) ----------

def volatility(prices: pd.Series) -> dict:
    log = returns(prices, "log")
    result = {"returns": len(log), "daily": NAN, "annual": NAN, "reason": None, "warning": None}
    if len(log) < MIN_RETURNS:
        result["reason"] = _not_enough(_returns_text(MIN_RETURNS), str(len(log)))
        return result
    daily = log.std(ddof=1)
    result.update(daily=daily, annual=daily * math.sqrt(DAYS_PER_YEAR),
                  warning=_unreliable(RELIABLE_VOLATILITY, len(log)))
    return result


def rolling_volatility(prices: pd.Series, window: int = 30) -> pd.Series:
    """Annualised volatility of the last `window` daily log returns, i.e. window + 1 prices."""
    full = _calendar(prices)
    if full.empty:
        return full
    log = returns(full.dropna(), "log").reindex(full.index)  # a gap stays NaN inside the window
    return log.rolling(window, min_periods=window).std(ddof=1) * math.sqrt(DAYS_PER_YEAR)


# ---------- moving averages (chapter 6) ----------

def sma(prices: pd.Series, window: int) -> pd.Series:
    """SMA of the last `window` consecutive calendar days; a gap inside the window gives NaN."""
    full = _calendar(prices)
    if full.empty:
        return pd.Series(NAN, index=prices.index)
    return full.rolling(window, min_periods=window).mean().reindex(prices.index)


def ema(prices: pd.Series, window: int) -> pd.Series:
    """EMA with alpha = 2 / (w + 1); the first value is the SMA of the first w prices.

    After a missing day the EMA starts again: it waits for `window` consecutive
    prices and is seeded with their SMA, as at the beginning of the series.
    """
    full = _calendar(prices)
    if full.empty:
        return pd.Series(NAN, index=prices.index)
    alpha = 2 / (window + 1)
    values = full.to_numpy(dtype=float)
    result = np.full(len(values), NAN)
    run = 0
    for i, value in enumerate(values):
        if math.isnan(value):
            run = 0
            continue
        run += 1
        if run == window:
            result[i] = values[i - window + 1:i + 1].mean()
        elif run > window:
            result[i] = alpha * value + (1 - alpha) * result[i - 1]
    return pd.Series(result, index=full.index).reindex(prices.index)


# ---------- drawdown (chapter 7) ----------

def drawdown(prices: pd.Series) -> tuple[pd.Series, dict]:
    """Drawdown from the highest price reached so far within the given prices, and the maximum drawdown."""
    clean = valid_prices(prices)
    info = {"max_drawdown": NAN, "peak_date": None, "trough_date": None,
            "duration_days": None, "recovery_date": None, "reason": None}
    if len(clean) < MIN_PRICES_DRAWDOWN:
        info["reason"] = _not_enough(_prices_text(MIN_PRICES_DRAWDOWN), str(len(clean)))
        return pd.Series(NAN, index=clean.index, dtype=float), info
    series = clean / clean.cummax() - 1
    mdd = series.min()
    if mdd >= 0:
        info["max_drawdown"] = 0.0
        return series, info
    trough = series.idxmin()
    before_trough = clean.loc[:trough]
    peak = before_trough[before_trough == before_trough.max()].index[-1]
    after = clean.loc[trough:].iloc[1:]
    recovered = after[after >= clean.loc[peak]]
    recovery = recovered.index[0] if len(recovered) else None
    info.update(
        max_drawdown=mdd,
        peak_date=peak.date(),
        trough_date=trough.date(),
        duration_days=(trough - peak).days,
        recovery_date=recovery.date() if recovery is not None else None,
    )
    return series, info


# ---------- distribution (chapter 8) ----------

def shape(prices: pd.Series) -> dict:
    """Bias-corrected skewness G1 and excess kurtosis G2 of log returns, as SKEW and KURT in Excel."""
    log = returns(prices, "log")
    n = len(log)
    result = {
        "returns": n,
        "skewness": NAN, "kurtosis": NAN,
        "skewness_se": math.sqrt(6 / n) if n else NAN,
        "kurtosis_se": math.sqrt(24 / n) if n else NAN,
        "extreme_day": None, "skewness_without_extreme_day": NAN,
        "skewness_reason": None, "kurtosis_reason": None,
        "warning": _unreliable(RELIABLE_SHAPE, n) if n else None,
    }
    constant = n > 1 and log.std(ddof=1) == 0

    if n < MIN_SKEWNESS:
        result["skewness_reason"] = _not_enough(_returns_text(MIN_SKEWNESS), str(n))
    elif constant:
        result["skewness_reason"] = ZERO_VARIANCE
    else:
        result["skewness"] = log.skew()

    if n < MIN_KURTOSIS:
        result["kurtosis_reason"] = _not_enough(_returns_text(MIN_KURTOSIS), str(n))
    elif constant:
        result["kurtosis_reason"] = ZERO_VARIANCE
    else:
        result["kurtosis"] = log.kurt()

    if n:
        extreme_day = log.abs().idxmax()
        result["extreme_day"] = extreme_day.date()
        rest = log.drop(extreme_day)
        if len(rest) >= MIN_SKEWNESS and rest.std(ddof=1) > 0:
            result["skewness_without_extreme_day"] = rest.skew()
    return result


def histogram(log_returns: pd.Series, min_bins: int = 20, max_bins: int = 100) -> pd.DataFrame:
    """Freedman-Diaconis bins limited to 20-100, with the normal density scaled to the number of days.

    The reason for an empty or incomplete result and a warning are in .attrs.
    """
    values = pd.to_numeric(log_returns, errors="coerce").astype(float)
    values = values[np.isfinite(values)]
    n = len(values)
    frame = pd.DataFrame(columns=["from", "to", "count", "normal"], dtype=float)
    frame.attrs = {"reason": None, "warning": None}
    if n < MIN_RETURNS:
        frame.attrs["reason"] = _not_enough(_returns_text(MIN_RETURNS), str(n))
        return frame
    low, high = values.min(), values.max()
    std = values.std(ddof=1)
    if high == low or std == 0:
        frame = pd.DataFrame({"from": [low], "to": [high], "count": [n], "normal": [NAN]})
        frame.attrs = {"reason": ZERO_VARIANCE, "warning": None}
        return frame
    iqr = values.quantile(0.75) - values.quantile(0.25)
    width_fd = 2 * iqr * n ** (-1 / 3)
    bins = math.ceil((high - low) / width_fd) if width_fd > 0 else min_bins
    bins = min(max(bins, min_bins), max_bins)
    counts, edges = np.histogram(values, bins=bins, range=(low, high))
    width = edges[1] - edges[0]
    centers = (edges[:-1] + edges[1:]) / 2
    density = stats.norm.pdf(centers, loc=values.mean(), scale=std)
    frame = pd.DataFrame({"from": edges[:-1], "to": edges[1:], "count": counts, "normal": n * width * density})
    frame.attrs = {"reason": None, "warning": _unreliable(RELIABLE_HISTOGRAM, n)}
    return frame


# ---------- BNB against the market (chapter 9) ----------

def correlation_beta(asset: pd.Series, market: pd.Series) -> dict:
    """Pearson and Spearman correlation, beta, alpha and R^2 on two return series, common dates only."""
    joined = pd.concat([asset.rename("asset"), market.rename("market")], axis=1, join="inner")
    joined = joined.apply(pd.to_numeric, errors="coerce").astype(float)
    joined = joined[np.isfinite(joined).all(axis=1)]
    n = len(joined)
    result = {"returns": n, "pearson": NAN, "spearman": NAN, "beta": NAN, "alpha": NAN,
              "r_squared": NAN, "reason": None, "warning": None}
    if n < MIN_RETURNS:
        result["reason"] = _not_enough(_returns_text(MIN_RETURNS) + " wspólne", str(n))
        return result
    result["warning"] = _unreliable(RELIABLE_MARKET, n)
    a, m = joined["asset"], joined["market"]
    if m.var(ddof=1) == 0:
        result["reason"] = "wartość nieokreślona: zwroty BTC w okresie się nie zmieniały"
        return result
    regression = stats.linregress(m, a)
    result.update(beta=regression.slope, alpha=regression.intercept)
    if a.var(ddof=1) == 0:
        result["reason"] = "korelacja nieokreślona: zwroty BNB w okresie się nie zmieniały"
        return result
    result.update(
        pearson=a.corr(m),
        spearman=stats.spearmanr(a, m).statistic,
        r_squared=regression.rvalue ** 2,
    )
    return result


def market_relation(asset_prices: pd.Series, market_prices: pd.Series) -> dict:
    return correlation_beta(returns(asset_prices, "log"), returns(market_prices, "log"))


def rolling_correlation(asset_prices: pd.Series, market_prices: pd.Series, window: int = 90) -> pd.Series:
    """Correlation of the last `window` calendar days; a gap or a constant window gives NaN."""
    asset_log, market_log = returns(asset_prices, "log"), returns(market_prices, "log")
    if asset_log.empty or market_log.empty:
        return pd.Series(dtype=float)
    calendar = pd.date_range(min(asset_log.index.min(), market_log.index.min()),
                             max(asset_log.index.max(), market_log.index.max()), freq="D")
    asset, market = asset_log.reindex(calendar), market_log.reindex(calendar)
    result = asset.rolling(window, min_periods=window).corr(market)
    return result.where(np.isfinite(result))


# ---------- event study (chapter 9.3) ----------

def holm_adjust(p_values: list[float]) -> list[float]:
    """Holm step-down adjustment across all planned tests (NaN stays NaN, still counted in M)."""
    m = len(p_values)
    adjusted = [NAN] * m
    valid = sorted((p, i) for i, p in enumerate(p_values) if p is not None and math.isfinite(p))
    running_max = 0.0
    for rank, (p, idx) in enumerate(valid):
        scaled = min(1.0, (m - rank) * p)
        running_max = max(running_max, scaled)
        adjusted[idx] = running_max
    return adjusted


def event_study(
    asset_prices: pd.Series,
    market_prices: pd.Series,
    event_day,
    excluded_dates=None,
    win_before: int = 1,
    win_after: int = 1,
    est_before: int = 120,
    est_after: int = 11,
) -> dict:
    """Market-model event study with MacKinlay forecast-error SE(CAR)."""
    asset_clean = valid_prices(asset_prices)
    market_clean = valid_prices(market_prices)
    asset_log = returns(asset_clean, "log")
    market_log = returns(market_clean, "log")
    common_ret = asset_log.index.intersection(market_log.index).sort_values()

    event_ts = pd.Timestamp(event_day)
    if event_ts.tzinfo is not None:
        event_ts = event_ts.tz_convert("UTC").tz_localize(None)
    event_ts = event_ts.normalize()

    excluded = {pd.Timestamp(d).tz_localize(None).normalize() if pd.Timestamp(d).tzinfo is None
                else pd.Timestamp(d).tz_convert("UTC").tz_localize(None).normalize()
                for d in (excluded_dates or ())}

    est_start = event_ts - pd.Timedelta(days=est_before)
    est_end = event_ts - pd.Timedelta(days=est_after)
    est_idx = common_ret[(common_ret >= est_start) & (common_ret <= est_end) & (~common_ret.isin(excluded))]
    n_est = len(est_idx)

    result = {
        "event_date": event_ts.date(),
        "window": f"[{ -win_before }, +{ win_after }]",
        "estimation_days": n_est,
        "alpha": NAN, "beta": NAN, "s_eps": NAN,
        "asset_return": NAN, "market_return": NAN,
        "car": NAN, "car_alpha0": NAN, "se_car": NAN, "t": NAN, "p_value": NAN,
        "reason": None, "warning": None,
    }
    if n_est < 3:
        result["reason"] = _not_enough("3 dni w oknie estymacji", str(n_est))
        return result

    m_est = market_log.loc[est_idx].to_numpy(dtype=float)
    r_est = asset_log.loc[est_idx].to_numpy(dtype=float)
    m_mean = float(m_est.mean())
    s_xx = float(np.sum((m_est - m_mean) ** 2))
    if s_xx == 0:
        result["reason"] = "wartość nieokreślona: zwroty BTC w oknie estymacji się nie zmieniały"
        return result

    beta = float(np.sum((m_est - m_mean) * (r_est - r_est.mean())) / s_xx)
    alpha = float(r_est.mean() - beta * m_mean)
    resid = r_est - (alpha + beta * m_est)
    s_eps = float(math.sqrt(np.sum(resid ** 2) / (n_est - 2)))
    result.update(alpha=alpha, beta=beta, s_eps=s_eps, warning=_unreliable(RELIABLE_EVENT_ESTIMATION, n_est))

    ev_idx = pd.date_range(event_ts - pd.Timedelta(days=win_before), event_ts + pd.Timedelta(days=win_after), freq="D")
    if not ev_idx.isin(common_ret).all():
        missing = int((~ev_idx.isin(common_ret)).sum())
        result["reason"] = f"brak pełnych danych w oknie wydarzenia, brakuje {_returns_text(missing)}"
        return result

    t_len = len(ev_idx)
    r_ev = asset_log.loc[ev_idx].to_numpy(dtype=float)
    m_ev = market_log.loc[ev_idx].to_numpy(dtype=float)
    car = float(np.sum(r_ev - (alpha + beta * m_ev)))
    car_alpha0 = float(np.sum(r_ev - beta * m_ev))

    pre_day = event_ts - pd.Timedelta(days=win_before + 1)
    end_day = event_ts + pd.Timedelta(days=win_after)
    if pre_day in asset_clean.index and end_day in asset_clean.index:
        result["asset_return"] = float(asset_clean.loc[end_day] / asset_clean.loc[pre_day] - 1)
    if pre_day in market_clean.index and end_day in market_clean.index:
        result["market_return"] = float(market_clean.loc[end_day] / market_clean.loc[pre_day] - 1)

    se_car = float(s_eps * math.sqrt(t_len + (t_len ** 2) / n_est + ((float(m_ev.sum()) - t_len * m_mean) ** 2) / s_xx))
    result.update(car=car, car_alpha0=car_alpha0, se_car=se_car)
    if se_car > 0:
        t_stat = car / se_car
        p_val = float(2.0 * stats.t.sf(abs(t_stat), df=n_est - 2))
        result.update(t=t_stat, p_value=p_val)
    else:
        result["reason"] = "zerowy błąd standardowy reszt w oknie estymacji"
    return result


def events_summary(
    asset_prices: pd.Series,
    market_prices: pd.Series,
    events: list[tuple[str, str]],
    outlier_dates=None,
    windows: tuple[tuple[int, int], ...] = ((1, 1), (5, 5)),
) -> pd.DataFrame:
    """Evaluate all events across planned windows and apply Holm correction."""
    excluded = set(outlier_dates or ())
    for event_day, _ in events:
        ts = pd.Timestamp(event_day).normalize()
        for offset in range(-5, 6):
            excluded.add(ts + pd.Timedelta(days=offset))

    rows = []
    for event_day, label in events:
        for before, after in windows:
            res = event_study(asset_prices, market_prices, event_day, excluded, win_before=before, win_after=after)
            res["label"] = label
            rows.append(res)

    adjusted = holm_adjust([row["p_value"] for row in rows])
    for row, p_adj in zip(rows, adjusted):
        row["p_holm"] = p_adj
        if math.isfinite(p_adj):
            row["significant"] = "istotna" if p_adj < 0.05 else "nieistotna"
        else:
            row["significant"] = "brak wyniku"
    return pd.DataFrame(rows)


# ---------- volume analysis (chapter 10) ----------

def relative_volume(quote_volume: pd.Series, window: int = 30) -> pd.Series:
    """Logarithm of daily quote volume relative to the mean of the preceding `window` calendar days."""
    values = pd.to_numeric(quote_volume, errors="coerce").astype(float).sort_index()
    values = values[np.isfinite(values) & (values > 0)]
    if values.empty:
        return values
    calendar = values.reindex(pd.date_range(values.index.min(), values.index.max(), freq="D"))
    prior_mean = calendar.shift(1).rolling(window, min_periods=window).mean()
    ratio = calendar / prior_mean
    result = np.log(ratio.where(ratio > 0))
    return result.reindex(quote_volume.index)


def volume_relation(prices: pd.Series, quote_volume: pd.Series, window: int = 30) -> dict:
    """Pearson and Spearman correlation between |log_return| and relative log volume."""
    abs_ret = returns(prices, "log").abs().rename("abs_ret")
    rel_vol = relative_volume(quote_volume, window=window).rename("rel_vol")
    joined = pd.concat([abs_ret, rel_vol], axis=1, join="inner")
    joined = joined.apply(pd.to_numeric, errors="coerce").astype(float)
    joined = joined[np.isfinite(joined).all(axis=1)]
    n = len(joined)
    result = {"observations": n, "pearson": NAN, "pearson_p": NAN,
              "spearman": NAN, "spearman_p": NAN, "reason": None, "warning": None}
    if n < MIN_RETURNS:
        result["reason"] = _not_enough(_returns_text(MIN_RETURNS), str(n))
        return result
    result["warning"] = _unreliable(RELIABLE_VOLUME, n)
    if joined["abs_ret"].var(ddof=1) == 0 or joined["rel_vol"].var(ddof=1) == 0:
        result["reason"] = ZERO_VARIANCE
        return result
    p_res = stats.pearsonr(joined["abs_ret"], joined["rel_vol"])
    s_res = stats.spearmanr(joined["abs_ret"], joined["rel_vol"])
    result.update(
        pearson=float(p_res.statistic), pearson_p=float(p_res.pvalue),
        spearman=float(s_res.statistic), spearman_p=float(s_res.pvalue),
    )
    return result
