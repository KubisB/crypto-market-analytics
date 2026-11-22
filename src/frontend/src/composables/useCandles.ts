import { ref, shallowRef } from 'vue'
import { type DailyCandle, fetchCandles, BinanceUnavailableError } from '@/api/candles'
import { addDays } from '@/stats/formatting'

const WARMUP_DAYS = 200

export function useCandles() {
  const bnbCandles = shallowRef<DailyCandle[]>([])
  const btcCandles = shallowRef<DailyCandle[]>([])
  const loading = ref(false)
  const error = ref<string | null>(null)
  const loadedFrom = ref<string | null>(null)
  const loadedTo = ref<string | null>(null)

  let controller: AbortController | null = null

  async function load(from: string, to: string, earliestDate: string, symbols: string[]) {
    controller?.abort()
    controller = new AbortController()
    const signal = controller.signal

    loading.value = true
    error.value = null

    let warmupFrom = addDays(from, -WARMUP_DAYS)
    if (warmupFrom < earliestDate) warmupFrom = earliestDate

    try {
      const results = await Promise.all(
        symbols.map((s) => fetchCandles(s, warmupFrom, to, signal)),
      )
      bnbCandles.value = results[0] ?? []
      btcCandles.value = results[1] ?? []
      loadedFrom.value = from
      loadedTo.value = to
    } catch (e: unknown) {
      if (e instanceof DOMException && e.name === 'AbortError') return
      if (e instanceof BinanceUnavailableError) {
        error.value = 'Nie udało się pobrać danych z Binance. Spróbuj ponownie za kilka minut.'
      } else if (e instanceof Error) {
        error.value = e.message
      } else {
        error.value = 'Wystąpił nieoczekiwany błąd. Odśwież stronę i spróbuj ponownie.'
      }
    } finally {
      loading.value = false
    }
  }

  return { bnbCandles, btcCandles, loading, error, loadedFrom, loadedTo, load }
}
