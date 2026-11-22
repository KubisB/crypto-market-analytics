import type { DailyReturn } from './returns'

export interface StatResult {
  value: number | null
  warning?: string
  empty?: string
}

export interface DayResult {
  value: number | null
  date: string | null
  warning?: string
  empty?: string
}

export interface DescriptiveStats {
  priceCount: number
  returnCount: number
  calendarDays: number
  meanReturn: StatResult
  medianReturn: StatResult
  dailyStdDev: StatResult
  annualVolatility: StatResult
  worstDay: DayResult
  bestDay: DayResult
  percentUpDays: StatResult
  skewness: StatResult
  kurtosis: StatResult
  maxDrawdown: StatResult
  totalReturn: StatResult
  cagr: StatResult
}

export function computeDescriptiveStats(
  returns: DailyReturn[],
  priceCount: number,
  calendarDays: number,
  firstPrice: number | null,
  lastPrice: number | null,
): DescriptiveStats {
  const n = returns.length

  const result: DescriptiveStats = {
    priceCount,
    returnCount: n,
    calendarDays,
    meanReturn: empty('Za mało danych: wymagane 2 zwroty.'),
    medianReturn: empty('Za mało danych: wymagane 2 zwroty.'),
    dailyStdDev: empty('Za mało danych: wymagane 2 zwroty.'),
    annualVolatility: empty('Za mało danych: wymagane 2 zwroty.'),
    worstDay: { value: null, date: null, empty: 'Za mało danych: wymagane 2 zwroty.' },
    bestDay: { value: null, date: null, empty: 'Za mało danych: wymagane 2 zwroty.' },
    percentUpDays: empty('Za mało danych: wymagane 2 zwroty.'),
    skewness: empty('Za mało danych: wymagane 3 zwroty.'),
    kurtosis: empty('Za mało danych: wymagane 4 zwroty.'),
    maxDrawdown: empty('Za mało danych: wymagane 2 ceny.'),
    totalReturn: empty('Za mało danych: wymagane 2 ceny.'),
    cagr: empty('Za mało danych: wymagane 2 ceny.'),
  }

  if (n >= 2) {
    const simpleReturns = returns.map((r) => r.simple)
    const logReturns = returns.map((r) => r.log)

    result.meanReturn = stat(mean(simpleReturns))
    result.medianReturn = stat(median(simpleReturns))

    const std = stdDev(logReturns)
    if (std === 0) {
      result.dailyStdDev = stat(0)
      result.annualVolatility = stat(0)
    } else {
      result.dailyStdDev = stat(std)
      result.annualVolatility = stat(std * Math.sqrt(365))
    }
    if (n < 30) {
      const warn = `Wynik mało wiarygodny: zalecane co najmniej 30 zwrotów, w wybranym okresie jest ${n}.`
      result.dailyStdDev.warning = warn
      result.annualVolatility.warning = warn
    }

    const worst = minWithIndex(simpleReturns)
    result.worstDay = { value: worst.value, date: returns[worst.index].date }
    const best = maxWithIndex(simpleReturns)
    result.bestDay = { value: best.value, date: returns[best.index].date }

    result.percentUpDays = computePercentUpDays(simpleReturns)
  }

  if (n >= 3) {
    const logReturns = returns.map((r) => r.log)
    const std = stdDev(logReturns)
    if (std === 0) {
      result.skewness = { value: null, empty: 'Wartość nieokreślona, ponieważ cena w wybranym okresie się nie zmieniała.' }
    } else {
      result.skewness = stat(skewness(logReturns))
      if (n < 365) {
        result.skewness.warning = `Wynik mało wiarygodny: zalecane co najmniej 365 zwrotów, w wybranym okresie jest ${n}.`
      }
    }
  }

  if (n >= 4) {
    const logReturns = returns.map((r) => r.log)
    const std = stdDev(logReturns)
    if (std === 0) {
      result.kurtosis = { value: null, empty: 'Wartość nieokreślona, ponieważ cena w wybranym okresie się nie zmieniała.' }
    } else {
      result.kurtosis = stat(kurtosis(logReturns))
      if (n < 365) {
        result.kurtosis.warning = `Wynik mało wiarygodny: zalecane co najmniej 365 zwrotów, w wybranym okresie jest ${n}.`
      }
    }
  }

  if (firstPrice !== null && lastPrice !== null && firstPrice > 0) {
    result.totalReturn = stat(lastPrice / firstPrice - 1)

    if (calendarDays >= 365) {
      result.cagr = stat(Math.pow(lastPrice / firstPrice, 365 / calendarDays) - 1)
    } else {
      result.cagr = { value: null, empty: 'Liczony tylko dla okresów co najmniej rocznych.' }
    }
  }

  return result
}

function empty(reason: string): StatResult {
  return { value: null, empty: reason }
}

function stat(value: number): StatResult {
  return { value }
}

function mean(values: number[]): number {
  return values.reduce((a, b) => a + b, 0) / values.length
}

function median(values: number[]): number {
  const sorted = [...values].sort((a, b) => a - b)
  const mid = Math.floor(sorted.length / 2)
  return sorted.length % 2 !== 0 ? sorted[mid] : (sorted[mid - 1] + sorted[mid]) / 2
}

export function stdDev(values: number[]): number {
  if (values.length < 2) return 0
  const m = mean(values)
  const sumSq = values.reduce((acc, v) => acc + (v - m) ** 2, 0)
  return Math.sqrt(sumSq / (values.length - 1))
}

function skewness(values: number[]): number {
  const n = values.length
  const m = mean(values)
  const s = stdDev(values)
  const sum = values.reduce((acc, v) => acc + ((v - m) / s) ** 3, 0)
  return (n / ((n - 1) * (n - 2))) * sum
}

function kurtosis(values: number[]): number {
  const n = values.length
  const m = mean(values)
  const s = stdDev(values)
  const sum = values.reduce((acc, v) => acc + ((v - m) / s) ** 4, 0)
  return ((n * (n + 1)) / ((n - 1) * (n - 2) * (n - 3))) * sum - (3 * (n - 1) ** 2) / ((n - 2) * (n - 3))
}

function minWithIndex(values: number[]) {
  let minVal = values[0]
  let minIdx = 0
  for (let i = 1; i < values.length; i++) {
    if (values[i] < minVal) {
      minVal = values[i]
      minIdx = i
    }
  }
  return { value: minVal, index: minIdx }
}

function maxWithIndex(values: number[]) {
  let maxVal = values[0]
  let maxIdx = 0
  for (let i = 1; i < values.length; i++) {
    if (values[i] > maxVal) {
      maxVal = values[i]
      maxIdx = i
    }
  }
  return { value: maxVal, index: maxIdx }
}

function computePercentUpDays(simpleReturns: number[]): StatResult {
  let up = 0
  let withChange = 0
  for (const r of simpleReturns) {
    if (r !== 0) {
      withChange++
      if (r > 0) up++
    }
  }
  if (withChange === 0) return { value: null, empty: 'Brak dni ze zmianą ceny.' }
  return stat(up / withChange)
}
