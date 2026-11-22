<script setup lang="ts">
import type { DescriptiveStats, StatResult, DayResult } from '@/stats/descriptive'
import type { MaxDrawdownInfo } from '@/stats/drawdown'
import { formatPercent, formatPrice, formatDatePL, formatIndicator } from '@/stats/formatting'

const props = defineProps<{
  stats: DescriptiveStats
  drawdown: MaxDrawdownInfo | null
  from: string
  to: string
  gapCount: number
}>()

interface Row {
  label: string
  tooltip: string
  value: string
  warning?: string
  empty?: string
}

function statRow(label: string, tooltip: string, r: StatResult, fmt: (v: number) => string): Row {
  return {
    label,
    tooltip,
    value: r.value !== null ? fmt(r.value) : '—',
    warning: r.warning,
    empty: r.empty,
  }
}

function dayRow(label: string, tooltip: string, r: DayResult): Row {
  const val =
    r.value !== null && r.date
      ? `${formatPercent(r.value)} (${formatDatePL(r.date)})`
      : '—'
  return { label, tooltip, value: val, warning: r.warning, empty: r.empty }
}

function getRows(): Row[] {
  const s = props.stats
  const rows: Row[] = [
    {
      label: 'Liczba cen',
      tooltip: 'Liczba dni w wybranym okresie, dla których jest poprawna cena zamknięcia.',
      value: String(s.priceCount),
    },
    {
      label: 'Liczba zwrotów',
      tooltip:
        'Liczba dziennych zmian ceny, z których liczone są statystyki. Zmiana powstaje z cen dwóch kolejnych dni, więc zwrotów jest o jeden mniej niż cen, a przy brakującym dniu jeszcze mniej. Od tej liczby zależy wiarygodność pozostałych wartości.',
      value: String(s.returnCount),
    },
    statRow(
      'Średni dzienny zwrot',
      'O ile procent średnio zmieniała się cena z dnia na dzień. Przy dużych wahaniach może być dodatni, mimo że cena w całym okresie spadła, dlatego wynik okresu pokazuje zwrot za cały okres.',
      s.meanReturn,
      formatPercent,
    ),
    statRow(
      'Mediana zwrotu',
      'Zwrot typowego dnia. Połowa dni miała wynik niższy, a połowa wyższy.',
      s.medianReturn,
      formatPercent,
    ),
    statRow(
      'Odchylenie standardowe dzienne',
      'Rozrzut dziennych zmian ceny, liczony na zwrotach logarytmicznych. Im wyższe, tym większe dzienne wahania.',
      s.dailyStdDev,
      formatPercent,
    ),
    statRow(
      'Zmienność roczna',
      'Siła wahań ceny w skali roku, czyli odchylenie dzienne pomnożone przez pierwiastek z 365. Wyższa wartość oznacza bardziej nerwowy rynek.',
      s.annualVolatility,
      formatPercent,
    ),
    dayRow(
      'Najgorszy dzień',
      'Największy dzienny spadek ceny w wybranym okresie.',
      s.worstDay,
    ),
    dayRow(
      'Najlepszy dzień',
      'Największy dzienny wzrost ceny w wybranym okresie.',
      s.bestDay,
    ),
    statRow(
      'Odsetek dni wzrostowych',
      'Część dni ze zmianą ceny, w których cena wzrosła. Dni bez zmiany ceny są pomijane.',
      s.percentUpDays,
      formatPercent,
    ),
    statRow(
      'Skośność',
      'Wartość ujemna oznacza, że skrajne spadki były większe niż skrajne wzrosty, czyli dłuższy lewy ogon rozkładu.',
      s.skewness,
      formatIndicator,
    ),
    statRow(
      'Kurtoza',
      'Wartość powyżej 0 oznacza, że dni skrajne zdarzały się częściej niż w rozkładzie normalnym, w którym wartości skrajne są bardzo rzadkie.',
      s.kurtosis,
      formatIndicator,
    ),
  ]

  const mdd = props.drawdown
  if (mdd) {
    rows.push({
      label: 'Maksymalne obsunięcie',
      tooltip: 'Największy spadek ceny od szczytu w wybranym okresie.',
      value: formatPercent(mdd.value),
    })
  } else {
    rows.push({
      label: 'Maksymalne obsunięcie',
      tooltip: 'Największy spadek ceny od szczytu w wybranym okresie.',
      value: '—',
      empty: s.maxDrawdown.empty,
    })
  }

  rows.push(
    statRow(
      'Zwrot za cały okres',
      'O ile procent zmieniła się cena między pierwszym a ostatnim dniem okresu.',
      s.totalReturn,
      formatPercent,
    ),
    statRow(
      'Średni roczny zwrot (CAGR)',
      'Średnie roczne tempo zmiany ceny przy założeniu stałego tempa zmian. Liczony tylko dla okresów co najmniej rocznych.',
      s.cagr,
      formatPercent,
    ),
  )

  return rows
}
</script>

<template>
  <section class="stats-table-section">
    <h2>Statystyki dla wybranego okresu</h2>
    <p class="subtitle">{{ formatDatePL(from) }} do {{ formatDatePL(to) }}, {{ stats.calendarDays }} dni kalendarzowych</p>
    <p v-if="gapCount > 0" class="gap-warning">
      Brak danych z {{ gapCount }} dni. Zmiany ceny obejmujące te dni są pomijane.
    </p>
    <div class="table-wrapper">
      <table>
        <tbody>
          <tr v-for="row in getRows()" :key="row.label">
            <td class="label-cell">
              {{ row.label }}
              <span class="help" :title="row.tooltip">?</span>
            </td>
            <td class="value-cell">
              <template v-if="row.empty">
                <span class="empty">{{ row.empty }}</span>
              </template>
              <template v-else>
                {{ row.value }}
                <span v-if="row.warning" class="warning">{{ row.warning }}</span>
              </template>
            </td>
          </tr>
        </tbody>
      </table>
    </div>
  </section>
</template>

<style scoped>
.stats-table-section {
  margin-bottom: 32px;
}
h2 {
  font-size: 1.2rem;
  margin: 0 0 4px;
}
.subtitle {
  color: var(--text-muted);
  font-size: 0.85rem;
  margin: 0 0 12px;
}
.gap-warning {
  color: var(--text-error);
  font-size: 0.85rem;
  margin: 0 0 12px;
}
.table-wrapper {
  overflow-x: auto;
}
table {
  width: 100%;
  border-collapse: collapse;
}
td {
  padding: 8px 12px;
  border-bottom: 1px solid var(--border);
  font-size: 0.9rem;
}
.label-cell {
  font-weight: 600;
  white-space: nowrap;
}
.value-cell {
  text-align: right;
}
.help {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  width: 16px;
  height: 16px;
  border-radius: 50%;
  border: 1px solid var(--text-muted);
  color: var(--text-muted);
  font-size: 0.7rem;
  cursor: help;
  margin-left: 6px;
  vertical-align: middle;
}
.empty {
  color: var(--text-muted);
  font-style: italic;
  font-size: 0.85rem;
}
.warning {
  display: block;
  color: var(--text-warning);
  font-size: 0.75rem;
  margin-top: 2px;
}
</style>
