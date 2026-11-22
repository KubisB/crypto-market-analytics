export interface HistogramBin {
  from: number
  to: number
  count: number
}

export interface HistogramData {
  bins: HistogramBin[]
  normalCurve: { x: number; y: number }[]
  mean: number
  stdDev: number
}

export function computeHistogram(logReturns: number[]): HistogramData | null {
  const n = logReturns.length
  if (n < 2) return null

  const sorted = [...logReturns].sort((a, b) => a - b)
  const q1 = quantile(sorted, 0.25)
  const q3 = quantile(sorted, 0.75)
  const iqr = q3 - q1

  const m = mean(logReturns)
  const s = std(logReturns)

  let binWidth: number
  if (iqr === 0 || s === 0) {
    binWidth = (sorted[n - 1] - sorted[0]) / 30 || 0.01
  } else {
    binWidth = 2 * iqr * Math.pow(n, -1 / 3)
  }

  const min = sorted[0]
  const max = sorted[n - 1]
  let numBins = Math.ceil((max - min) / binWidth)
  numBins = Math.max(20, Math.min(100, numBins))
  binWidth = (max - min) / numBins

  const bins: HistogramBin[] = []
  for (let i = 0; i < numBins; i++) {
    bins.push({ from: min + i * binWidth, to: min + (i + 1) * binWidth, count: 0 })
  }

  for (const v of logReturns) {
    let idx = Math.floor((v - min) / binWidth)
    if (idx >= numBins) idx = numBins - 1
    if (idx < 0) idx = 0
    bins[idx].count++
  }

  const normalCurve: { x: number; y: number }[] = []
  const steps = 200
  const range = max - min
  const pad = range * 0.1
  for (let i = 0; i <= steps; i++) {
    const x = min - pad + ((range + 2 * pad) * i) / steps
    const density = normalPdf(x, m, s)
    normalCurve.push({ x, y: density * n * binWidth })
  }

  return { bins, normalCurve, mean: m, stdDev: s }
}

function quantile(sorted: number[], p: number): number {
  const h = (sorted.length - 1) * p
  const lo = Math.floor(h)
  const hi = Math.ceil(h)
  if (lo === hi) return sorted[lo]
  return sorted[lo] + (h - lo) * (sorted[hi] - sorted[lo])
}

function normalPdf(x: number, mean: number, stdDev: number): number {
  const z = (x - mean) / stdDev
  return Math.exp(-0.5 * z * z) / (stdDev * Math.sqrt(2 * Math.PI))
}

function mean(values: number[]): number {
  return values.reduce((a, b) => a + b, 0) / values.length
}

function std(values: number[]): number {
  const m = mean(values)
  const sumSq = values.reduce((acc, v) => acc + (v - m) ** 2, 0)
  return Math.sqrt(sumSq / (values.length - 1))
}
