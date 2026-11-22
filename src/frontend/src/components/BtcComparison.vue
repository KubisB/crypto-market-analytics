<script setup lang="ts">
import { ref, watch, onMounted, onUnmounted } from 'vue'
import { createChart, type IChartApi, LineSeries } from 'lightweight-charts'
import type { DailyCandle } from '@/api/candles'
import type { CorrelationResult } from '@/stats/correlation'
import { formatIndicator } from '@/stats/formatting'

const props = defineProps<{
  bnbCandles: DailyCandle[]
  btcCandles: DailyCandle[]
  correlation: CorrelationResult
}>()

const containerRef = ref<HTMLElement | null>(null)
let chart: IChartApi | null = null

function cumulativeReturn(candles: DailyCandle[]): { time: string; value: number }[] {
  if (candles.length === 0) return []
  const p0 = candles[0].close
  return candles.map((c) => ({ time: c.date, value: ((c.close / p0 - 1) * 100) }))
}

function build() {
  if (!containerRef.value) return
  chart?.remove()

  chart = createChart(containerRef.value, {
    width: containerRef.value.clientWidth,
    height: 300,
    layout: { background: { color: 'transparent' }, textColor: getComputedStyle(document.documentElement).getPropertyValue('--text').trim() || '#333' },
    grid: { vertLines: { color: 'rgba(128,128,128,0.1)' }, horzLines: { color: 'rgba(128,128,128,0.1)' } },
    rightPriceScale: { borderColor: 'rgba(128,128,128,0.2)' },
    timeScale: { timeVisible: false },
  })

  const bnbSeries = chart.addSeries(LineSeries, { color: '#F0B90B', lineWidth: 2 })
  bnbSeries.setData(cumulativeReturn(props.bnbCandles))

  const btcSeries = chart.addSeries(LineSeries, { color: '#FF9800', lineWidth: 2 })
  btcSeries.setData(cumulativeReturn(props.btcCandles))

  chart.timeScale().fitContent()
}

function resize() {
  if (chart && containerRef.value) chart.applyOptions({ width: containerRef.value.clientWidth })
}

onMounted(() => { build(); window.addEventListener('resize', resize) })
onUnmounted(() => { window.removeEventListener('resize', resize); chart?.remove() })
watch(() => [props.bnbCandles, props.btcCandles], build)
</script>

<template>
  <section class="btc-comparison-section">
    <h2>BNB na tle bitcoina</h2>
    <h3>Zwrot skumulowany od początku okresu</h3>
    <p class="desc">
      Oba szeregi startują od 0%, dzięki czemu można porównać zmiany cen o zupełnie różnej skali.
    </p>
    <div class="legend">
      <span><span class="dot bnb"></span>BNB/USDT</span>
      <span><span class="dot btc"></span>BTC/USDT</span>
    </div>
    <div ref="containerRef" class="chart-container"></div>
    <div class="metrics">
      <div class="metric">
        <span class="metric-label" title="Na ile ruchy BNB i bitcoina idą w tę samą stronę. 1 oznacza pełną zgodność ruchów, 0 brak związku liniowego.">Korelacja <span class="help">?</span></span>
        <span class="metric-value">
          <template v-if="correlation.correlation !== null">{{ formatIndicator(correlation.correlation) }}</template>
          <template v-else><span class="empty">{{ correlation.empty }}</span></template>
        </span>
      </div>
      <div class="metric">
        <span class="metric-label" title="O ile procent średnio zmieniał się BNB, gdy bitcoin zmieniał się o 1%. Przy słabej korelacji to tylko przeciętna, a nie reguła dla pojedynczych dni.">Beta <span class="help">?</span></span>
        <span class="metric-value">
          <template v-if="correlation.beta !== null">{{ formatIndicator(correlation.beta) }}</template>
          <template v-else><span class="empty">{{ correlation.empty }}</span></template>
        </span>
      </div>
      <div class="metric">
        <span class="metric-label" title="Udział wariancji dziennych zwrotów BNB wyjaśniany przez model z bitcoinem. Reszta to ruch specyficzny dla BNB, ale też czynniki, których model nie obejmuje.">R² <span class="help">?</span></span>
        <span class="metric-value">
          <template v-if="correlation.rSquared !== null">{{ formatIndicator(correlation.rSquared) }}</template>
          <template v-else><span class="empty">{{ correlation.empty }}</span></template>
        </span>
      </div>
      <p v-if="correlation.warning" class="warning">{{ correlation.warning }}</p>
    </div>
  </section>
</template>

<style scoped>
.btc-comparison-section { margin-bottom: 32px; }
h2 { font-size: 1.2rem; margin: 0 0 4px; }
h3 { font-size: 1rem; margin: 0 0 4px; color: var(--text); }
.desc { font-size: 0.8rem; color: var(--text-muted); margin: 0 0 8px; }
.legend { display: flex; gap: 16px; font-size: 0.85rem; margin-bottom: 8px; }
.dot { display: inline-block; width: 10px; height: 10px; border-radius: 50%; margin-right: 4px; vertical-align: middle; }
.dot.bnb { background: #F0B90B; }
.dot.btc { background: #FF9800; }
.chart-container { width: 100%; }
.metrics { display: flex; gap: 24px; flex-wrap: wrap; margin-top: 12px; }
.metric { display: flex; flex-direction: column; }
.metric-label { font-size: 0.85rem; color: var(--text-muted); font-weight: 600; }
.metric-value { font-size: 1.1rem; font-weight: 700; margin-top: 2px; }
.help { display: inline-flex; align-items: center; justify-content: center; width: 14px; height: 14px; border-radius: 50%; border: 1px solid var(--text-muted); font-size: 0.65rem; cursor: help; vertical-align: middle; }
.empty { color: var(--text-muted); font-style: italic; font-size: 0.85rem; }
.warning { font-size: 0.8rem; color: var(--text-warning); margin: 4px 0 0; width: 100%; }
</style>
