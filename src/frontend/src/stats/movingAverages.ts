import type { DailyCandle } from '@/api/candles'

export interface SmaPoint {
  date: string
  value: number
}

export function computeSma(candles: DailyCandle[], window: number): SmaPoint[] {
  const points: SmaPoint[] = []
  if (candles.length < window) return points

  let sum = 0
  for (let i = 0; i < window; i++) {
    sum += candles[i].close
  }
  points.push({ date: candles[window - 1].date, value: sum / window })

  for (let i = window; i < candles.length; i++) {
    sum += candles[i].close - candles[i - window].close
    points.push({ date: candles[i].date, value: sum / window })
  }

  return points
}
