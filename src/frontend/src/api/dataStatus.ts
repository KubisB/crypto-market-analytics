export interface SymbolStatus {
  symbol: string
  firstDate: string | null
  lastDate: string | null
  lastSyncedAt: string | null
  lastAttemptAt: string | null
  lastError: string | null
  exchangeStatus: string | null
  isUpToDate: boolean
}

export interface DataStatus {
  lastClosedDate: string
  analysisStartDate: string
  earliestDate: string
  symbols: SymbolStatus[]
}

export async function fetchDataStatus(signal?: AbortSignal): Promise<DataStatus> {
  const response = await fetch('/api/data-status', { signal })

  if (!response.ok) {
    throw new Error(`Nie udało się pobrać statusu danych: HTTP ${response.status}`)
  }

  return response.json()
}
