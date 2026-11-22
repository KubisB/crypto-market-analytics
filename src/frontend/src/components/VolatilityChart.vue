<script setup lang="ts">
import { ref, watch, onMounted, onUnmounted } from 'vue'
import { createChart, type IChartApi, LineSeries } from 'lightweight-charts'
import type { VolatilityPoint } from '@/stats/volatility'

const props = defineProps<{
  data: VolatilityPoint[]
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

  const series = chart.addSeries(LineSeries, { color: '#FF6F00', lineWidth: 1.5 })
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
  <section class="volatility-section">
    <h2>Zmienność 30-dniowa w skali rocznej</h2>
    <p class="desc">Zmienność z ostatnich 30 dni, liczona osobno dla każdego dnia. Pokazuje okresy spokoju i paniki na rynku.</p>
    <div ref="containerRef" class="chart-container"></div>
  </section>
</template>

<style scoped>
.volatility-section { margin-bottom: 32px; }
h2 { font-size: 1.2rem; margin: 0 0 4px; }
.desc { font-size: 0.8rem; color: var(--text-muted); margin: 0 0 8px; }
.chart-container { width: 100%; }
</style>
