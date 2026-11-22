export function formatDatePL(isoDate: string): string {
  const [y, m, d] = isoDate.split('-')
  return `${d}.${m}.${y}`
}

export function parseISODate(isoDate: string): Date {
  return new Date(isoDate + 'T00:00:00Z')
}

export function formatPercent(value: number | null): string {
  if (value === null) return '—'
  return (value * 100).toFixed(2) + '%'
}

export function formatPrice(value: number | null): string {
  if (value === null) return '—'
  return value.toFixed(2) + ' USDT'
}

export function formatVolumeMln(value: number | null): string {
  if (value === null) return '—'
  return (value / 1_000_000).toFixed(2) + ' mln USDT'
}

export function formatIndicator(value: number | null): string {
  if (value === null) return '—'
  return value.toFixed(4)
}

export function calendarDays(from: string, to: string): number {
  const a = parseISODate(from)
  const b = parseISODate(to)
  return Math.round((b.getTime() - a.getTime()) / 86_400_000)
}

export function addDays(isoDate: string, days: number): string {
  const d = parseISODate(isoDate)
  d.setUTCDate(d.getUTCDate() + days)
  return d.toISOString().slice(0, 10)
}

export function yesterdayUTC(): string {
  const d = new Date()
  d.setUTCDate(d.getUTCDate() - 1)
  return d.toISOString().slice(0, 10)
}

export function todayUTC(): string {
  return new Date().toISOString().slice(0, 10)
}
