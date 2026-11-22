<script setup lang="ts">
defineProps<{
  message: string
}>()
const emit = defineEmits<{
  retry: []
}>()
</script>

<template>
  <div class="error-state">
    <p>{{ message }}</p>
    <button @click="emit('retry')">Spróbuj ponownie</button>
  </div>
</template>

<style scoped>
.error-state {
  text-align: center;
  padding: 48px 16px;
  color: var(--text-error);
}
.error-state p {
  margin: 0 0 16px;
}
button {
  padding: 8px 20px;
  border: 1px solid var(--border);
  border-radius: 6px;
  background: var(--bg);
  color: var(--text);
  cursor: pointer;
  font-size: 0.95rem;
}
button:hover {
  background: var(--bg-hover);
}
</style>
