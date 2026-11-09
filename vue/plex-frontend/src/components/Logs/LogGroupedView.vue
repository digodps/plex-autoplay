<script setup>
import { computed } from "vue";

const props = defineProps({
  items: { type: Array, default: () => [] },
  groupBy: { type: String, default: "date" },
  loading: { type: Boolean, default: false },
});

function keyFor(log) {
  if (props.groupBy === "date") return (log.timestamp || "").split("T")[0];
  if (props.groupBy === "status") return log.jobStatus || "sem-status";
  return log.level || "sem-tipo";
}

const groups = computed(() => {
  const map = new Map();
  for (const log of props.items) {
    const key = keyFor(log);
    if (!map.has(key)) map.set(key, []);
    map.get(key).push(log);
  }
  return [...map.entries()]
    .sort((a, b) => b[0].localeCompare(a[0]))
    .map(([key, items]) => ({ key, items }));
});

const levelColors = {
  info: "text-gray-500 dark:text-gray-400",
  success: "text-meta-3",
  skip: "text-gray-500 dark:text-gray-400",
  warning: "text-meta-6",
  error: "text-meta-1",
};

function formatTime(ts) {
  const d = new Date(ts);
  return Number.isNaN(d.getTime()) ? ts : d.toLocaleTimeString("pt-BR");
}
</script>

<template>
  <div v-if="loading" class="p-6 text-center text-gray-500 dark:text-gray-400">Carregando logs...</div>
  <div v-else-if="groups.length === 0" class="p-6 text-center text-gray-500 dark:text-gray-400" data-testid="log-empty">
    Nenhum log encontrado.
  </div>
  <div v-else class="space-y-4">
    <section
      v-for="group in groups"
      :key="group.key"
      class="overflow-hidden rounded-lg border border-gray-200 bg-white dark:border-gray-600 dark:bg-gray-800"
      :data-testid="`log-group-${group.key}`"
    >
      <header class="flex items-center justify-between border-b border-gray-200 px-4 py-3 dark:border-gray-600">
        <h3 class="text-title-sm font-semibold text-gray-900 dark:text-gray-100">{{ group.key }}</h3>
        <span class="rounded-full bg-gray-100 px-2.5 py-0.5 text-xs font-medium text-gray-600 dark:bg-gray-700 dark:text-gray-300">
          {{ group.items.length }}
        </span>
      </header>
      <div class="max-h-96 overflow-y-auto p-4 font-mono text-sm overscroll-contain">
        <div
          v-for="(entry, index) in group.items"
          :key="`${entry.timestamp}-${index}`"
          class="flex flex-wrap gap-2 leading-relaxed"
          data-testid="log-entry"
        >
          <span class="flex-none text-xs text-gray-500 dark:text-gray-400">{{ formatTime(entry.timestamp) }}</span>
          <span class="flex-none text-xs capitalize" :class="levelColors[entry.level] || 'text-gray-500'">
            {{ entry.level }}
          </span>
          <span class="text-gray-900 dark:text-gray-100">{{ entry.message }}</span>
          <div v-if="entry.suggestion" class="w-full pl-14 text-xs text-gray-600 dark:text-gray-300">
            <strong>Sugestão:</strong> {{ entry.suggestion }}
          </div>
        </div>
      </div>
    </section>
  </div>
</template>