import type { DailyCandle } from '@/api/candles'

export interface DailyReturn {
  date: string
  simple: number
  log: number
}

function isConsecutiveDay(a: string, b: string): boolean {
  const da = new Date(a)
  const db = new Date(b)
  const diff = db.getTime() - da.getTime()
  return diff === 86_400_000
}

export function computeReturns(candles: DailyCandle[]): DailyReturn[] {
  const returns: DailyReturn[] = []
  for (let i = 1; i < candles.length; i++) {
    if (!isConsecutiveDay(candles[i - 1].date, candles[i].date)) continue
    const prev = candles[i - 1].close
    const curr = candles[i].close
    if (prev <= 0 || curr <= 0) continue
    returns.push({
      date: candles[i].date,
      simple: curr / prev - 1,
      log: Math.log(curr / prev),
    })
  }
  return returns
}

export function countGaps(candles: DailyCandle[]): number {
  let gaps = 0
  for (let i = 1; i < candles.length; i++) {
    if (!isConsecutiveDay(candles[i - 1].date, candles[i].date)) gaps++
  }
  return gaps
}
