export interface DailyCandle {
  date: string
  open: number
  high: number
  low: number
  close: number
  volume: number
  quoteVolume: number
}

interface DailyCandleDto {
  date: string
  open: number
  high: number
  low: number
  close: number
  volume: number
  quoteVolume: number
}

interface CandleRangeDto {
  symbol: string
  from: string
  to: string
  candles: DailyCandleDto[]
  missingDays: string[]
}

export async function fetchCandles(
  symbol: string,
  from: string,
  to: string,
  signal?: AbortSignal,
): Promise<DailyCandle[]> {
  const params = new URLSearchParams({ symbol, from, to })
  const response = await fetch(`/api/candles?${params}`, { signal })

  if (!response.ok) {
    const problem = await response.json().catch(() => null)
    const detail = problem?.detail ?? `HTTP ${response.status}`
    if (response.status === 400) throw new ValidationError(detail)
    if (response.status === 502) throw new BinanceUnavailableError(detail)
    throw new Error(detail)
  }

  const body: CandleRangeDto = await response.json()
  return body.candles.map((d) => ({
    date: d.date,
    open: d.open,
    high: d.high,
    low: d.low,
    close: d.close,
    volume: d.volume,
    quoteVolume: d.quoteVolume,
  }))
}

export class ValidationError extends Error {
  constructor(message: string) {
    super(message)
    this.name = 'ValidationError'
  }
}

export class BinanceUnavailableError extends Error {
  constructor(message: string) {
    super(message)
    this.name = 'BinanceUnavailableError'
  }
}
