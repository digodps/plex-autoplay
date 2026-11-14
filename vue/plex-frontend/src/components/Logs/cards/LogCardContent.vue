<script setup>
import { computed } from 'vue'
import { parseHierarchy, getHierarchyParts, formatTimestamp, getLevelConfig } from './logHierarchy.js'

const props = defineProps({
  log: {
    type: Object,
    required: true,
  },
})

const hierarchy = computed(() => parseHierarchy(props.log.message, props.log.suggestion))
const hierarchyParts = computed(() => getHierarchyParts(hierarchy.value))
const timestamp = computed(() => formatTimestamp(props.log.timestamp))
const levelConfig = computed(() => getLevelConfig(props.log.level))

const copyJobId = async () => {
  if (props.log.jobId) {
    await navigator.clipboard.writeText(props.log.jobId)
  }
}
</script>

<template>
  <div class="log-card-content animate-in slide-in-from-top-2 fade-in duration-200 ease-out" role="region" aria-label="Detalhes do log">
    <!-- Hierarchy Tree -->
    <div v-if="hierarchyParts.length > 0" class="mb-4 p-4 rounded-lg bg-gray-900/50 border border-gray-800">
      <div class="flex items-center gap-2 text-xs font-medium text-gray-400 uppercase tracking-wider mb-3">
        <span>Hierarquia do processamento</span>
      </div>
      <div class="space-y-2">
        <template v-for="(part, index) in hierarchyParts" :key="index">
          <div class="flex items-start gap-3 pl-2 border-l border-gray-700/50" :class="{ 'pb-2': index < hierarchyParts.length - 1 }">
            <div class="flex-shrink-0 w-6 h-6 flex items-center justify-center text-lg" :style="{ marginTop: '2px' }">
              {{ part.icon }}
            </div>
            <div class="flex-1 min-w-0">
              <div class="flex items-center gap-2">
                <span class="text-xs font-medium text-gray-500 uppercase tracking-wide">{{ part.label }}</span>
                <span class="text-sm text-gray-200 truncate">{{ part.value }}</span>
              </div>
            </div>
          </div>
        </template>
      </div>
    </div>

    <!-- Full Message -->
    <div class="mb-4">
      <div class="flex items-center gap-2 text-xs font-medium text-gray-400 uppercase tracking-wider mb-2">
        <span>Mensagem completa</span>
      </div>
      <pre class="p-4 rounded-lg bg-gray-900/50 border border-gray-800 text-gray-300 text-sm font-mono whitespace-pre-wrap break-words">{{ props.log.message }}</pre>
    </div>

    <!-- Suggestion -->
    <div v-if="props.log.suggestion" class="mb-4 p-4 rounded-lg bg-yellow-500/10 border border-yellow-500/30">
      <div class="flex items-center gap-2 text-xs font-medium text-yellow-400 uppercase tracking-wider mb-2">
        <span>Sugestão</span>
      </div>
      <p class="text-yellow-300 text-sm">{{ props.log.suggestion }}</p>
    </div>

    <!-- Metadata -->
    <div class="grid grid-cols-1 sm:grid-cols-2 gap-4 text-sm">
      <div class="p-3 rounded-lg bg-gray-900/50 border border-gray-800">
        <div class="text-xs font-medium text-gray-500 uppercase tracking-wider mb-1">Timestamp</div>
        <div class="text-gray-200 font-mono">{{ timestamp.full }}</div>
      </div>
      <div class="p-3 rounded-lg bg-gray-900/50 border border-gray-800">
        <div class="text-xs font-medium text-gray-500 uppercase tracking-wider mb-1">Nível</div>
        <div class="flex items-center gap-2">
          <span :class="['inline-flex items-center px-2 py-0.5 rounded text-xs font-medium border', levelConfig.class]">
            {{ levelConfig.label }}
          </span>
        </div>
      </div>
      <div v-if="props.log.jobId" class="p-3 rounded-lg bg-gray-900/50 border border-gray-800 sm:col-span-2">
        <div class="flex items-center justify-between">
          <div class="text-xs font-medium text-gray-500 uppercase tracking-wider">Job ID</div>
          <button
            @click="copyJobId"
            class="text-xs text-gray-400 hover:text-white transition-colors flex items-center gap-1"
            title="Copiar Job ID"
            aria-label="Copiar Job ID"
          >
            <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 16H6a2 2 0 01-2-2V6a2 2 0 012-2h8a2 2 0 012 2v2m-6 12h8a2 2 0 002-2v-8a2 2 0 00-2-2h-8a2 2 0 00-2 2v8a2 2 0 002 2z"/></svg>
            Copiar
          </button>
        </div>
        <code class="block mt-1 text-xs text-gray-300 font-mono break-all">{{ props.log.jobId }}</code>
      </div>
      <div v-if="props.log.jobStatus" class="p-3 rounded-lg bg-gray-900/50 border border-gray-800">
        <div class="text-xs font-medium text-gray-500 uppercase tracking-wider mb-1">Status do Job</div>
        <span class="inline-flex items-center px-2 py-0.5 rounded text-xs font-medium bg-gray-800 text-gray-300 border border-gray-700">
          {{ props.log.jobStatus }}
        </span>
      </div>
    </div>
  </div>
</template>

<style scoped>
/* Ensure smooth animation */
.log-card-content {
  overflow: hidden;
}
</style>