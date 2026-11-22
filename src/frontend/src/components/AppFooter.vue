<script setup lang="ts">
import { formatDatePL } from '@/stats/formatting'

defineProps<{
  lastDate: string | null
}>()
</script>

<template>
  <footer class="app-footer">
    <p>
      Źródło danych: Binance, pary BNB/USDT i BTC/USDT, świece dzienne w UTC.<template v-if="lastDate">
        Dane do dnia {{ formatDatePL(lastDate) }}.</template>
    </p>
    <p>Projekt edukacyjny. Wyniki analiz nie stanowią porady inwestycyjnej.</p>
  </footer>
</template>

<style scoped>
.app-footer {
  margin-top: 48px;
  padding: 24px 0;
  border-top: 1px solid var(--border);
  color: var(--text-muted);
  font-size: 0.85rem;
  text-align: center;
}
.app-footer p {
  margin: 0 0 4px;
}
</style>
