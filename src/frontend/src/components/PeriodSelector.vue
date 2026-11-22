<script setup lang="ts">
import { ref } from 'vue'
import { addDays, formatDatePL } from '@/stats/formatting'

const props = defineProps<{
  analysisStart: string
  lastClosedDate: string
}>()

const emit = defineEmits<{
  change: [from: string, to: string]
}>()

const from = ref(props.analysisStart)
const to = ref(props.lastClosedDate)
const validationError = ref<string | null>(null)

function setShortcut(days: number | 'all') {
  to.value = props.lastClosedDate
  if (days === 'all') {
    from.value = props.analysisStart
  } else {
    from.value = addDays(props.lastClosedDate, -days)
  }
  submit()
}

function validate(): string | null {
  if (!from.value || !to.value) return 'Wybierz datę początkową i końcową.'
  if (from.value > to.value) return 'Data początkowa nie może być późniejsza niż końcowa.'
  if (from.value < props.analysisStart) return `Najwcześniejsza dostępna data to ${formatDatePL(props.analysisStart)}.`
  if (to.value > props.lastClosedDate)
    return `Nie można wybrać daty późniejszej niż ${formatDatePL(props.lastClosedDate)}, ponieważ świeca bieżącego dnia nie jest jeszcze zamknięta.`
  return null
}

function submit() {
  const err = validate()
  validationError.value = err
  if (!err) emit('change', from.value, to.value)
}

defineExpose({ submit })
</script>

<template>
  <div class="period-selector">
    <div class="fields">
      <label>
        Od
        <input type="date" v-model="from" :min="analysisStart" :max="lastClosedDate" />
      </label>
      <label>
        Do
        <input type="date" v-model="to" :min="analysisStart" :max="lastClosedDate" />
      </label>
      <button class="submit-btn" @click="submit">Pokaż</button>
    </div>
    <div class="shortcuts">
      <button @click="setShortcut(30)">30 dni</button>
      <button @click="setShortcut(90)">90 dni</button>
      <button @click="setShortcut(365)">365 dni</button>
      <button @click="setShortcut('all')">Całość od {{ analysisStart.slice(0, 4) }}</button>
    </div>
    <p class="hint">
      Daty w UTC. Skróty są liczone wstecz od ostatniego zamkniętego dnia,
      {{ formatDatePL(lastClosedDate) }}.
    </p>
    <p v-if="validationError" class="validation-error">{{ validationError }}</p>
  </div>
</template>

<style scoped>
.period-selector {
  margin-bottom: 24px;
}
.fields {
  display: flex;
  gap: 12px;
  align-items: flex-end;
  flex-wrap: wrap;
}
label {
  display: flex;
  flex-direction: column;
  gap: 4px;
  font-size: 0.85rem;
  font-weight: 600;
}
input[type='date'] {
  padding: 6px 10px;
  border: 1px solid var(--border);
  border-radius: 6px;
  background: var(--bg);
  color: var(--text);
  font-size: 0.95rem;
}
.submit-btn {
  padding: 6px 20px;
  border: 1px solid var(--accent);
  border-radius: 6px;
  background: var(--accent);
  color: #fff;
  cursor: pointer;
  font-size: 0.95rem;
  font-weight: 600;
}
.submit-btn:hover {
  opacity: 0.9;
}
.shortcuts {
  display: flex;
  gap: 8px;
  margin-top: 10px;
  flex-wrap: wrap;
}
.shortcuts button {
  padding: 4px 14px;
  border: 1px solid var(--border);
  border-radius: 16px;
  background: var(--bg);
  color: var(--text);
  cursor: pointer;
  font-size: 0.85rem;
}
.shortcuts button:hover {
  background: var(--bg-hover);
}
.hint {
  margin: 8px 0 0;
  font-size: 0.8rem;
  color: var(--text-muted);
}
.validation-error {
  margin: 8px 0 0;
  color: var(--text-error);
  font-size: 0.9rem;
}
</style>
