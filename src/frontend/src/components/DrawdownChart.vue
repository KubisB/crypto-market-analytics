<script setup lang="ts">
import { ref, watch, onMounted, onUnmounted } from 'vue'
import { createChart, type IChartApi, AreaSeries } from 'lightweight-charts'
import type { DrawdownPoint } from '@/stats/drawdown'
import type { MaxDrawdownInfo } from '@/stats/drawdown'
import { formatPercent, formatDatePL } from '@/stats/formatting'

const props = defineProps<{
  data: DrawdownPoint[]
  maxDrawdown: MaxDrawdownInfo | null
}>()

const containerRef = ref<HTMLElement | null>(null)
let chart: IChartApi | null = null

function build() {
  if (!containerRef.value) return
  chart?.remove()

  chart = createChart(containerRef.value, {
    width: containerRef.value.clientWidth,
    height: 250,
    layout: { background: { color: 'transparent' }, textColor: getComputedStyle(document.documentElement).getPropertyValue('--text').trim() || '#333' },
    grid: { vertLines: { color: 'rgba(128,128,128,0.1)' }, horzLines: { color: 'rgba(128,128,128,0.1)' } },
    rightPriceScale: { borderColor: 'rgba(128,128,128,0.2)' },
    timeScale: { timeVisible: false },
  })

  const series = chart.addSeries(AreaSeries, {
    lineColor: '#ef5350',
    topColor: 'rgba(239,83,80,0.3)',
    bottomColor: 'rgba(239,83,80,0.05)',
    lineWidth: 1.5,
  })
  series.setData(props.data.map((p) => ({ time: p.date, value: p.value * 100 })))
  chart.timeScale().fitContent()
}

function resize() {
  if (chart && containerRef.value) chart.applyOptions({ width: containerRef.value.clientWidth })
}

onMounted(() => { build(); window.addEventListener('resize', resize) })
onUnmounted(() => { window.removeEventListener('resize', resize); chart?.remove() })
watch(() => props.data, build)
</script>

<template>
  <section class="drawdown-section">
    <h2>Spadek ceny od szczytu</h2>
    <p class="desc">
      O ile procent cena jest niższa od najwyższej ceny osiągniętej do danego dnia w wybranym
      okresie. Zero oznacza, że cena jest na szczycie.
    </p>
    <div ref="containerRef" class="chart-container"></div>
    <p v-if="maxDrawdown && maxDrawdown.value < 0" class="summary">
      Maksymalne obsunięcie {{ formatPercent(maxDrawdown.value) }}: szczyt
      {{ formatDatePL(maxDrawdown.peakDate) }}, dołek
      {{ formatDatePL(maxDrawdown.troughDate) }}, {{ maxDrawdown.daysDrop }} dni spadku.
      <template v-if="maxDrawdown.recoveryDate">
        Cena wróciła do poziomu szczytu {{ formatDatePL(maxDrawdown.recoveryDate) }}.
      </template>
      <template v-else>
        Cena nie wróciła do poziomu szczytu do końca okresu.
      </template>
    </p>
  </section>
</template>

<style scoped>
.drawdown-section { margin-bottom: 32px; }
h2 { font-size: 1.2rem; margin: 0 0 4px; }
.desc { font-size: 0.8rem; color: var(--text-muted); margin: 0 0 8px; }
.chart-container { width: 100%; }
.summary { font-size: 0.85rem; margin-top: 8px; color: var(--text); }
</style>
