<script setup lang="ts">
import { computed } from 'vue'
import { Bar } from 'vue-chartjs'
import {
  Chart as ChartJS,
  CategoryScale,
  LinearScale,
  BarElement,
  LineElement,
  PointElement,
  Tooltip,
  Legend,
} from 'chart.js'
import type { HistogramData } from '@/stats/histogram'

ChartJS.register(CategoryScale, LinearScale, BarElement, LineElement, PointElement, Tooltip, Legend)

const props = defineProps<{
  data: HistogramData | null
}>()

const chartData = computed(() => {
  if (!props.data) return null
  const { bins, normalCurve } = props.data
  return {
    labels: bins.map((b) => ((b.from + b.to) / 2 * 100).toFixed(1)),
    datasets: [
      {
        type: 'bar' as const,
        label: 'Liczba dni',
        data: bins.map((b) => b.count),
        backgroundColor: bins.map((b) => (b.from + b.to) / 2 >= 0 ? 'rgba(38,166,154,0.6)' : 'rgba(239,83,80,0.6)'),
        borderWidth: 0,
        order: 2,
      },
      {
        type: 'line' as const,
        label: 'Rozkład normalny',
        data: normalCurve
          .filter((_, i) => i % Math.max(1, Math.floor(normalCurve.length / bins.length)) === 0)
          .slice(0, bins.length)
          .map((p) => p.y),
        borderColor: '#FF6F00',
        borderWidth: 2,
        pointRadius: 0,
        fill: false,
        order: 1,
      },
    ],
  }
})

const chartOptions = {
  responsive: true,
  maintainAspectRatio: false,
  plugins: {
    legend: { position: 'top' as const },
  },
  scales: {
    x: {
      title: { display: true, text: 'Dzienny zwrot logarytmiczny [%]' },
    },
    y: {
      title: { display: true, text: 'Liczba dni' },
      beginAtZero: true,
    },
  },
}
</script>

<template>
  <section v-if="data" class="histogram-section">
    <h2>Rozkład dziennych zwrotów</h2>
    <div class="chart-wrapper">
      <Bar v-if="chartData" :data="chartData as any" :options="chartOptions" />
    </div>
    <p class="explanation">
      Wysokość słupka to liczba dni, w których zwrot mieścił się w danym przedziale. Słupki na prawo
      od zera to dni wzrostowe, na lewo spadkowe. Krzywa pokazuje rozkład normalny o tej samej
      średniej i odchyleniu. Słupki wyższe od krzywej w środku i na brzegach oznaczają, że dni
      skrajne zdarzały się częściej, niż zakłada rozkład normalny. Przy dużych ruchach zwrot
      logarytmiczny różni się od zmiany procentowej, np. spadek o 10% to zwrot logarytmiczny
      −10,5%.
    </p>
  </section>
</template>

<style scoped>
.histogram-section { margin-bottom: 32px; }
h2 { font-size: 1.2rem; margin: 0 0 8px; }
.chart-wrapper { height: 300px; }
.explanation { font-size: 0.8rem; color: var(--text-muted); margin-top: 8px; line-height: 1.5; }
</style>
