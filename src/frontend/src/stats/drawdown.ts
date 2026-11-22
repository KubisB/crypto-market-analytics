import type { DailyCandle } from '@/api/candles'

export interface DrawdownPoint {
  date: string
  value: number
}

export interface MaxDrawdownInfo {
  value: number
  peakDate: string
  troughDate: string
  daysDrop: number
  recoveryDate: string | null
}

export function computeDrawdownSeries(candles: DailyCandle[]): DrawdownPoint[] {
  if (candles.length < 2) return []

  let peak = candles[0].close
  let peakDate = candles[0].date
  const series: DrawdownPoint[] = []

  for (const c of candles) {
    if (c.close >= peak) {
      peak = c.close
      peakDate = c.date
    }
    series.push({
      date: c.date,
      value: c.close / peak - 1,
    })
  }

  return series
}

export function computeMaxDrawdown(candles: DailyCandle[]): MaxDrawdownInfo | null {
  if (candles.length < 2) return null

  let peak = candles[0].close
  let peakDate = candles[0].date
  let mdd = 0
  let mddPeakDate = candles[0].date
  let mddTroughDate = candles[0].date

  for (const c of candles) {
    if (c.close >= peak) {
      peak = c.close
      peakDate = c.date
    }
    const dd = c.close / peak - 1
    if (dd < mdd) {
      mdd = dd
      mddPeakDate = peakDate
      mddTroughDate = c.date
    }
  }

  if (mdd === 0) return { value: 0, peakDate: candles[0].date, troughDate: candles[0].date, daysDrop: 0, recoveryDate: candles[0].date }

  const peakPrice = candles.find((c) => c.date === mddPeakDate)!.close
  let recoveryDate: string | null = null
  let pastTrough = false
  for (const c of candles) {
    if (c.date === mddTroughDate) pastTrough = true
    if (pastTrough && c.close >= peakPrice) {
      recoveryDate = c.date
      break
    }
  }

  const daysDrop = Math.round(
    (new Date(mddTroughDate).getTime() - new Date(mddPeakDate).getTime()) / 86_400_000,
  )

  return { value: mdd, peakDate: mddPeakDate, troughDate: mddTroughDate, daysDrop, recoveryDate }
}
