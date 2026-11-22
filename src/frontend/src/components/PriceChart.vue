<script setup lang="ts">
import { ref, watch, onMounted, onUnmounted, computed } from 'vue'
import { createChart, type IChartApi, type ISeriesApi, LineSeries, CandlestickSeries, HistogramSeries } from 'lightweight-charts'
import type { DailyCandle } from '@/api/candles'
import type { SmaPoint } from '@/stats/movingAverages'
import { formatDatePL, formatPrice, formatVolumeMln, formatPercent } from '@/stats/formatting'

const props = defineProps<{
  candles: DailyCandle[]
  sma20: SmaPoint[]
  sma50: SmaPoint[]
  sma200: SmaPoint[]
}>()

type ChartMode = 'line' | 'candle'

const chartMode = ref<ChartMode>('line')
const containerRef = ref<HTMLElement | null>(null)
const legendRef = ref<HTMLElement | null>(null)

let chart: IChartApi | null = null
let mainSeries: ISeriesApi<any> | null = null
let volumeSeries: ISeriesApi<any> | null = null
let sma20Series: ISeriesApi<any> | null = null
let sma50Series: ISeriesApi<any> | null = null
let sma200Series: ISeriesApi<any> | null = null

const SMA_COLORS = { sma20: '#2196F3', sma50: '#FF9800', sma200: '#9C27B0' }

function buildChart() {
  if (!containerRef.value) return
  if (chart) {
    chart.remove()
    chart = null
  }

  chart = createChart(containerRef.value, {
    width: containerRef.value.clientWidth,
    height: 420,
    layout: {
      background: { color: 'transparent' },
      textColor: getComputedStyle(document.documentElement).getPropertyValue('--text').trim() || '#333',
    },
    grid: {
      vertLines: { color: 'rgba(128,128,128,0.1)' },
      horzLines: { color: 'rgba(128,128,128,0.1)' },
    },
    timeScale: { timeVisible: false },
    rightPriceScale: { borderColor: 'rgba(128,128,128,0.2)' },
    crosshair: { mode: 0 },
  })

  if (chartMode.value === 'candle') {
    mainSeries = chart.addSeries(CandlestickSeries, {
      upColor: '#26a69a',
      downColor: '#ef5350',
      borderUpColor: '#26a69a',
      borderDownColor: '#ef5350',
      wickUpColor: '#26a69a',
      wickDownColor: '#ef5350',
    })
    mainSeries.setData(
      props.candles.map((c) => ({
        time: c.date,
        open: c.open,
        high: c.high,
        low: c.low,
        close: c.close,
      })),
    )
  } else {
    mainSeries = chart.addSeries(LineSeries, {
      color: '#2196F3',
      lineWidth: 2,
    })
    mainSeries.setData(props.candles.map((c) => ({ time: c.date, value: c.close })))
  }

  volumeSeries = chart.addSeries(HistogramSeries, {
    priceFormat: { type: 'volume' },
    priceScaleId: 'volume',
  })
  chart.priceScale('volume').applyOptions({
    scaleMargins: { top: 0.85, bottom: 0 },
  })
  volumeSeries.setData(
    props.candles.map((c) => ({
      time: c.date,
      value: c.quoteVolume,
      color: c.close >= c.open ? 'rgba(38,166,154,0.3)' : 'rgba(239,83,80,0.3)',
    })),
  )

  sma20Series = chart.addSeries(LineSeries, { color: SMA_COLORS.sma20, lineWidth: 1 })
  sma20Series.setData(props.sma20.map((p) => ({ time: p.date, value: p.value })))

  sma50Series = chart.addSeries(LineSeries, { color: SMA_COLORS.sma50, lineWidth: 1 })
  sma50Series.setData(props.sma50.map((p) => ({ time: p.date, value: p.value })))

  sma200Series = chart.addSeries(LineSeries, { color: SMA_COLORS.sma200, lineWidth: 1 })
  sma200Series.setData(props.sma200.map((p) => ({ time: p.date, value: p.value })))

  chart.subscribeCrosshairMove((param) => {
    if (!legendRef.value) return
    if (!param.time || !param.seriesData.size) {
      legendRef.value.textContent = ''
      return
    }
    const candleData = param.seriesData.get(mainSeries!) as any
    if (!candleData) return

    const candle = props.candles.find((c) => c.date === param.time)
    if (!candle) return

    const prevCandle = props.candles[props.candles.indexOf(candle) - 1]
    const change = prevCandle ? candle.close / prevCandle.close - 1 : null

    legendRef.value.innerHTML = [
      formatDatePL(candle.date),
      `O: ${formatPrice(candle.open)}`,
      `H: ${formatPrice(candle.high)}`,
      `L: ${formatPrice(candle.low)}`,
      `C: ${formatPrice(candle.close)}`,
      change !== null ? `Zmiana: ${formatPercent(change)}` : '',
      `Vol: ${formatVolumeMln(candle.quoteVolume)}`,
    ]
      .filter(Boolean)
      .join(' &middot; ')
  })

  chart.timeScale().fitContent()
}

function handleResize() {
  if (chart && containerRef.value) {
    chart.applyOptions({ width: containerRef.value.clientWidth })
  }
}

onMounted(() => {
  buildChart()
  window.addEventListener('resize', handleResize)
})

onUnmounted(() => {
  window.removeEventListener('resize', handleResize)
  chart?.remove()
})

watch(() => [props.candles, props.sma20, props.sma50, props.sma200], buildChart)
watch(chartMode, buildChart)
</script>

<template>
  <section class="price-chart-section">
    <div class="chart-header">
      <h2>Cena BNB/USDT</h2>
      <div class="mode-toggle">
        <button :class="{ active: chartMode === 'line' }" @click="chartMode = 'line'">Liniowy</button>
        <button :class="{ active: chartMode === 'candle' }" @click="chartMode = 'candle'">Świece japońskie</button>
      </div>
    </div>
    <p v-if="chartMode === 'candle'" class="candle-desc">
      Każda świeca to jeden dzień. Prostokąt sięga od ceny otwarcia do ceny zamknięcia, a linie nad
      nim i pod nim do ceny najwyższej i najniższej. Świeca zielona oznacza wzrost ceny w ciągu
      dnia, czerwona spadek.
    </p>
    <div class="sma-legend">
      <span><span class="dot" :style="{ background: SMA_COLORS.sma20 }"></span>SMA 20</span>
      <span><span class="dot" :style="{ background: SMA_COLORS.sma50 }"></span>SMA 50</span>
      <span><span class="dot" :style="{ background: SMA_COLORS.sma200 }"></span>SMA 200</span>
    </div>
    <p class="sma-desc">
      Średnia cena z ostatnich 20, 50 lub 200 dni. Krótsze średnie pokazują bieżący kierunek ceny,
      a SMA 200 długoterminowy trend.
    </p>
    <div ref="containerRef" class="chart-container"></div>
    <div ref="legendRef" class="chart-legend"></div>
  </section>
</template>

<style scoped>
.price-chart-section {
  margin-bottom: 32px;
}
.chart-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  flex-wrap: wrap;
  gap: 12px;
  margin-bottom: 8px;
}
.chart-header h2 {
  margin: 0;
  font-size: 1.2rem;
}
.mode-toggle {
  display: flex;
  gap: 0;
}
.mode-toggle button {
  padding: 4px 14px;
  border: 1px solid var(--border);
  background: var(--bg);
  color: var(--text);
  cursor: pointer;
  font-size: 0.85rem;
}
.mode-toggle button:first-child {
  border-radius: 6px 0 0 6px;
}
.mode-toggle button:last-child {
  border-radius: 0 6px 6px 0;
  border-left: none;
}
.mode-toggle button.active {
  background: var(--accent);
  color: #fff;
  border-color: var(--accent);
}
.candle-desc,
.sma-desc {
  font-size: 0.8rem;
  color: var(--text-muted);
  margin: 4px 0 8px;
}
.sma-legend {
  display: flex;
  gap: 16px;
  font-size: 0.85rem;
  margin-top: 8px;
}
.dot {
  display: inline-block;
  width: 10px;
  height: 10px;
  border-radius: 50%;
  margin-right: 4px;
  vertical-align: middle;
}
.chart-container {
  width: 100%;
}
.chart-legend {
  font-size: 0.8rem;
  color: var(--text-muted);
  min-height: 1.2em;
  margin-top: 4px;
}
</style>
