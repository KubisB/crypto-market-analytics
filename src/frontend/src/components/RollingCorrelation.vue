<script setup lang="ts">
import { ref, watch, onMounted, onUnmounted } from 'vue'
import { createChart, type IChartApi, LineSeries } from 'lightweight-charts'
import type { RollingCorrelationPoint } from '@/stats/correlation'

const props = defineProps<{
  data: RollingCorrelationPoint[]
}>()

const containerRef = ref<HTMLElement | null>(null)
let chart: IChartApi | null = null

function build() {
  if (!containerRef.value) return
  chart?.remove()

  chart = createChart(containerRef.value, {
    width: containerRef.value.clientWidth,
    height: 220,
    layout: { background: { color: 'transparent' }, textColor: getComputedStyle(document.documentElement).getPropertyValue('--text').trim() || '#333' },
    grid: { vertLines: { color: 'rgba(128,128,128,0.1)' }, horzLines: { color: 'rgba(128,128,128,0.1)' } },
    rightPriceScale: { borderColor: 'rgba(128,128,128,0.2)' },
    timeScale: { timeVisible: false },
  })

  const series = chart.addSeries(LineSeries, { color: '#7B1FA2', lineWidth: 1.5 })
  series.setData(props.data.map((p) => ({ time: p.date, value: p.value })))
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
  <section class="rolling-corr-section">
    <h3>Korelacja z bitcoinem w oknie 90 dni</h3>
    <p class="desc">Pokazuje, czy związek BNB z rynkiem jest stały, czy słabnie w niektórych okresach.</p>
    <div ref="containerRef" class="chart-container"></div>
  </section>
</template>

<style scoped>
.rolling-corr-section { margin-bottom: 32px; }
h3 { font-size: 1rem; margin: 0 0 4px; }
.desc { font-size: 0.8rem; color: var(--text-muted); margin: 0 0 8px; }
.chart-container { width: 100%; }
</style>
