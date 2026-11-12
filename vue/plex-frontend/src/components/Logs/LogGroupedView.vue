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
  info: "text-gray-400",
  success: "text-green-400",
  skip: "text-yellow-400",
  warning: "text-yellow-400",
  error: "text-red-400",
};

function formatTime(ts) {
  const d = new Date(ts);
  return Number.isNaN(d.getTime()) ? ts : d.toLocaleTimeString("pt-BR");
}
</script>

<template>
  <div v-if="loading" class="p-6 text-center text-gray-400">Carregando logs...</div>
  <div v-else-if="groups.length === 0" class="rounded-xl border border-gray-800 bg-[#1f2937] p-8 text-center text-gray-400 shadow-lg" data-testid="log-empty">
    Nenhum log encontrado.
  </div>
  <div v-else class="space-y-4">
    <section
      v-for="group in groups"
      :key="group.key"
      class="overflow-hidden rounded-xl border border-gray-800 bg-[#1f2937] shadow-lg"
      :data-testid="`log-group-${group.key}`"
    >
      <header class="flex items-center justify-between border-b border-gray-800/80 px-6 py-4">
        <h3 class="text-lg font-semibold text-white tracking-tight">{{ group.key }}</h3>
        <span class="rounded-lg bg-gray-800 px-3 py-1 text-xs font-medium text-gray-400 border border-gray-700/50">
          {{ group.items.length }}
        </span>
      </header>
      <div class="max-h-96 overflow-y-auto p-6 font-mono text-sm space-y-3 overscroll-contain">
        <div
          v-for="(entry, index) in group.items"
          :key="`${entry.timestamp}-${index}`"
          class="flex flex-wrap gap-x-3 gap-y-1 leading-relaxed border-b border-gray-800/40 pb-2 last:border-b-0 last:pb-0"
          data-testid="log-entry"
        >
          <span class="flex-none text-xs text-gray-500 font-sans">{{ formatTime(entry.timestamp) }}</span>
          <span class="flex-none text-xs capitalize font-sans font-medium" :class="levelColors[entry.level] || 'text-gray-400'">
            {{ entry.level }}
          </span>
          <span class="w-full text-gray-300 sm:w-auto sm:flex-1">{{ entry.message }}</span>
          <div v-if="entry.suggestion" class="w-full pl-14 text-xs text-red-400">
            <strong>Sugestão:</strong> {{ entry.suggestion }}
          </div>
        </div>
      </div>
    </section>
  </div>
</template>