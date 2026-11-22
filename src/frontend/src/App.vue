<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { fetchDataStatus, type DataStatus } from '@/api/dataStatus'
import { useCandles } from '@/composables/useCandles'
import { computeReturns, countGaps } from '@/stats/returns'
import { computeDescriptiveStats } from '@/stats/descriptive'
import { computeDrawdownSeries, computeMaxDrawdown } from '@/stats/drawdown'
import { computeRollingVolatility } from '@/stats/volatility'
import { computeSma } from '@/stats/movingAverages'
import { computeCorrelation, computeRollingCorrelation } from '@/stats/correlation'
import { computeHistogram } from '@/stats/histogram'
import { calendarDays } from '@/stats/formatting'

import BnbInfo from '@/components/BnbInfo.vue'
import PeriodSelector from '@/components/PeriodSelector.vue'
import PriceChart from '@/components/PriceChart.vue'
import StatsTable from '@/components/StatsTable.vue'
import VolatilityChart from '@/components/VolatilityChart.vue'
import DrawdownChart from '@/components/DrawdownChart.vue'
import BtcComparison from '@/components/BtcComparison.vue'
import RollingCorrelation from '@/components/RollingCorrelation.vue'
import ReturnHistogram from '@/components/ReturnHistogram.vue'
import AppFooter from '@/components/AppFooter.vue'
import LoadingState from '@/components/LoadingState.vue'
import ErrorState from '@/components/ErrorState.vue'

const { bnbCandles, btcCandles, loading, error, loadedFrom, loadedTo, load } = useCandles()
const dataStatus = ref<DataStatus | null>(null)
const initError = ref<string | null>(null)
const periodFrom = ref('')
const periodTo = ref('')
const periodSelectorRef = ref<InstanceType<typeof PeriodSelector> | null>(null)

function filterByPeriod(candles: { date: string }[], from: string, to: string) {
  return candles.filter((c) => c.date >= from && c.date <= to)
}

const bnbPeriod = computed(() =>
  loadedFrom.value && loadedTo.value
    ? filterByPeriod(bnbCandles.value, loadedFrom.value, loadedTo.value)
    : [],
)
const btcPeriod = computed(() =>
  loadedFrom.value && loadedTo.value
    ? filterByPeriod(btcCandles.value, loadedFrom.value, loadedTo.value)
    : [],
)

const bnbReturns = computed(() => computeReturns(bnbPeriod.value))
const btcReturns = computed(() => computeReturns(btcPeriod.value))
const gapCount = computed(() => countGaps(bnbPeriod.value))

const sma20 = computed(() => {
  const all = computeSma(bnbCandles.value, 20)
  return loadedFrom.value ? all.filter((p) => p.date >= loadedFrom.value!) : all
})
const sma50 = computed(() => {
  const all = computeSma(bnbCandles.value, 50)
  return loadedFrom.value ? all.filter((p) => p.date >= loadedFrom.value!) : all
})
const sma200 = computed(() => {
  const all = computeSma(bnbCandles.value, 200)
  return loadedFrom.value ? all.filter((p) => p.date >= loadedFrom.value!) : all
})

const stats = computed(() => {
  const p = bnbPeriod.value
  const days = loadedFrom.value && loadedTo.value ? calendarDays(loadedFrom.value, loadedTo.value) : 0
  return computeDescriptiveStats(
    bnbReturns.value,
    p.length,
    days,
    p.length > 0 ? p[0].close : null,
    p.length > 0 ? p[p.length - 1].close : null,
  )
})

const drawdownSeries = computed(() => computeDrawdownSeries(bnbPeriod.value))
const maxDrawdown = computed(() => computeMaxDrawdown(bnbPeriod.value))

const volatilitySeries = computed(() => {
  const allReturns = computeReturns(bnbCandles.value)
  const all = computeRollingVolatility(allReturns, 30)
  return loadedFrom.value ? all.filter((p) => p.date >= loadedFrom.value!) : all
})

const correlationResult = computed(() => computeCorrelation(bnbReturns.value, btcReturns.value))
const rollingCorrelation = computed(() => {
  const allBnb = computeReturns(bnbCandles.value)
  const allBtc = computeReturns(btcCandles.value)
  const all = computeRollingCorrelation(allBnb, allBtc, 90)
  return loadedFrom.value ? all.filter((p) => p.date >= loadedFrom.value!) : all
})

const histogramData = computed(() => {
  const logReturns = bnbReturns.value.map((r) => r.log)
  return computeHistogram(logReturns)
})

const lastDate = computed(() => {
  const p = bnbPeriod.value
  return p.length > 0 ? p[p.length - 1].date : null
})

function loadCandles() {
  if (!dataStatus.value) return
  const symbols = dataStatus.value.symbols.map((s) => s.symbol)
  load(periodFrom.value, periodTo.value, dataStatus.value.earliestDate, symbols)
}

function handlePeriodChange(from: string, to: string) {
  periodFrom.value = from
  periodTo.value = to
  loadCandles()
}

function retry() {
  if (dataStatus.value) {
    loadCandles()
  } else {
    init()
  }
}

async function init() {
  initError.value = null
  try {
    dataStatus.value = await fetchDataStatus()
    periodFrom.value = dataStatus.value.analysisStartDate
    periodTo.value = dataStatus.value.lastClosedDate
    loadCandles()
  } catch (e) {
    initError.value = e instanceof Error ? e.message : 'Nie udało się pobrać konfiguracji.'
  }
}

onMounted(init)
</script>

<template>
  <div class="app">
    <header class="app-header">
      <h1>Analiza rynku BNB/USDT</h1>
      <p class="subtitle">Statystyczna analiza dziennych notowań z giełdy Binance na tle bitcoina</p>
    </header>

    <main>
      <template v-if="dataStatus">
        <BnbInfo />
        <PeriodSelector
          ref="periodSelectorRef"
          :analysis-start="dataStatus.analysisStartDate"
          :last-closed-date="dataStatus.lastClosedDate"
          @change="handlePeriodChange"
        />

        <LoadingState v-if="loading" />
        <ErrorState v-else-if="error" :message="error" @retry="retry" />

        <template v-else-if="bnbPeriod.length > 0">
          <PriceChart
          :candles="bnbPeriod"
          :sma20="sma20"
          :sma50="sma50"
          :sma200="sma200"
        />

        <StatsTable
          :stats="stats"
          :drawdown="maxDrawdown"
          :from="loadedFrom!"
          :to="loadedTo!"
          :gap-count="gapCount"
        />

        <VolatilityChart :data="volatilitySeries" />

        <DrawdownChart :data="drawdownSeries" :max-drawdown="maxDrawdown" />

        <ReturnHistogram :data="histogramData" />

        <BtcComparison
          :bnb-candles="bnbPeriod"
          :btc-candles="btcPeriod"
          :correlation="correlationResult"
        />

        <RollingCorrelation :data="rollingCorrelation" />
        </template>
      </template>
      <LoadingState v-else-if="!initError" />
      <ErrorState v-else :message="initError!" @retry="retry" />
    </main>

    <AppFooter :last-date="lastDate" />
  </div>
</template>

<style>
:root {
  --bg: #ffffff;
  --bg-hover: #f5f5f5;
  --text: #1a1a1a;
  --text-muted: #6b7280;
  --text-error: #dc2626;
  --text-warning: #d97706;
  --border: #e5e7eb;
  --accent: #2563eb;
}

@media (prefers-color-scheme: dark) {
  :root {
    --bg: #111827;
    --bg-hover: #1f2937;
    --text: #f3f4f6;
    --text-muted: #9ca3af;
    --text-error: #f87171;
    --text-warning: #fbbf24;
    --border: #374151;
    --accent: #3b82f6;
  }
}

*,
*::before,
*::after {
  box-sizing: border-box;
}

body {
  margin: 0;
  font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
  background: var(--bg);
  color: var(--text);
  line-height: 1.5;
}
</style>

<style scoped>
.app {
  max-width: 960px;
  margin: 0 auto;
  padding: 24px 16px;
}

.app-header {
  margin-bottom: 24px;
}

.app-header h1 {
  margin: 0;
  font-size: 1.5rem;
}

.subtitle {
  margin: 4px 0 0;
  color: var(--text-muted);
  font-size: 0.9rem;
}
</style>
