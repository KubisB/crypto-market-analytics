import type { DailyReturn } from './returns'

export interface CorrelationResult {
  correlation: number | null
  beta: number | null
  rSquared: number | null
  warning?: string
  empty?: string
}

export interface RollingCorrelationPoint {
  date: string
  value: number
}

export function computeCorrelation(
  bnbReturns: DailyReturn[],
  btcReturns: DailyReturn[],
): CorrelationResult {
  const { bnb, btc } = alignReturns(bnbReturns, btcReturns)
  const n = bnb.length

  if (n < 2) {
    return { correlation: null, beta: null, rSquared: null, empty: `Za mało danych: wymagane 2 wspólne zwroty, jest ${n}.` }
  }

  const meanBnb = mean(bnb)
  const meanBtc = mean(btc)

  let sumXY = 0
  let sumXX = 0
  let sumYY = 0
  for (let i = 0; i < n; i++) {
    const dx = bnb[i] - meanBnb
    const dy = btc[i] - meanBtc
    sumXY += dx * dy
    sumXX += dx * dx
    sumYY += dy * dy
  }

  if (sumYY === 0) {
    return { correlation: null, beta: null, rSquared: null, empty: 'Wartość nieokreślona, ponieważ cena BTC w wybranym okresie się nie zmieniała.' }
  }
  if (sumXX === 0) {
    return { correlation: null, beta: null, rSquared: null, empty: 'Wartość nieokreślona, ponieważ cena BNB w wybranym okresie się nie zmieniała.' }
  }

  const corr = sumXY / Math.sqrt(sumXX * sumYY)
  const beta = sumXY / sumYY
  const rSquared = corr * corr

  const result: CorrelationResult = { correlation: corr, beta, rSquared }
  if (n < 30) {
    result.warning = `Wynik mało wiarygodny: zalecane co najmniej 30 wspólnych zwrotów, jest ${n}.`
  }
  return result
}

export function computeRollingCorrelation(
  bnbReturns: DailyReturn[],
  btcReturns: DailyReturn[],
  window: number = 90,
): RollingCorrelationPoint[] {
  const points: RollingCorrelationPoint[] = []
  const btcByDate = new Map(btcReturns.map((r) => [r.date, r.log]))

  const aligned: { date: string; bnb: number; btc: number }[] = []
  for (const r of bnbReturns) {
    const btcLog = btcByDate.get(r.date)
    if (btcLog !== undefined) {
      aligned.push({ date: r.date, bnb: r.log, btc: btcLog })
    }
  }

  for (let i = window - 1; i < aligned.length; i++) {
    const slice = aligned.slice(i - window + 1, i + 1)
    const bnb = slice.map((s) => s.bnb)
    const btc = slice.map((s) => s.btc)
    const corr = pearson(bnb, btc)
    if (corr !== null) {
      points.push({ date: aligned[i].date, value: corr })
    }
  }

  return points
}

function alignReturns(bnb: DailyReturn[], btc: DailyReturn[]) {
  const btcByDate = new Map(btc.map((r) => [r.date, r.log]))
  const alignedBnb: number[] = []
  const alignedBtc: number[] = []
  for (const r of bnb) {
    const btcLog = btcByDate.get(r.date)
    if (btcLog !== undefined) {
      alignedBnb.push(r.log)
      alignedBtc.push(btcLog)
    }
  }
  return { bnb: alignedBnb, btc: alignedBtc }
}

function pearson(x: number[], y: number[]): number | null {
  const n = x.length
  if (n < 2) return null
  const mx = mean(x)
  const my = mean(y)
  let sxy = 0, sxx = 0, syy = 0
  for (let i = 0; i < n; i++) {
    const dx = x[i] - mx
    const dy = y[i] - my
    sxy += dx * dy
    sxx += dx * dx
    syy += dy * dy
  }
  if (sxx === 0 || syy === 0) return null
  return sxy / Math.sqrt(sxx * syy)
}

function mean(values: number[]): number {
  return values.reduce((a, b) => a + b, 0) / values.length
}
