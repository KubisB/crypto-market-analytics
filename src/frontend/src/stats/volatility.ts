import type { DailyReturn } from './returns'
import { stdDev } from './descriptive'

export interface VolatilityPoint {
  date: string
  value: number
}

export function computeRollingVolatility(
  returns: DailyReturn[],
  window: number = 30,
): VolatilityPoint[] {
  const points: VolatilityPoint[] = []
  for (let i = window - 1; i < returns.length; i++) {
    const windowReturns = returns.slice(i - window + 1, i + 1).map((r) => r.log)
    const vol = stdDev(windowReturns) * Math.sqrt(365)
    points.push({ date: returns[i].date, value: vol })
  }
  return points
}
