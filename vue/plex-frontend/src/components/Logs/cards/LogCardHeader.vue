<script setup>
import { computed } from 'vue'
import { formatTimestamp, getLevelConfig, truncate } from './logHierarchy.js'

const props = defineProps({
  log: {
    type: Object,
    required: true,
  },
  isExpanded: {
    type: Boolean,
    required: true,
  },
  onToggle: {
    type: Function,
    required: true,
  },
})

const timestamp = computed(() => formatTimestamp(props.log.timestamp))
const levelConfig = computed(() => getLevelConfig(props.log.level))
const preview = computed(() => truncate(props.log.message, 80))

const handleKeydown = (event) => {
  if (event.key === 'Enter' || event.key === ' ') {
    event.preventDefault()
    props.onToggle()
  }
}
</script>

<template>
  <header class="log-card-header" role="heading" :aria-level="3">
    <button
      :id="`log-card-button-${props.log.timestamp}`"
      type="button"
      class="w-full flex items-start gap-4 p-4 focus:outline-none focus-visible:ring-2 focus-visible:ring-accent/50 focus-visible:ring-offset-2 focus-visible:ring-offset-gray-900 rounded-xl transition-all duration-200"
      :aria-expanded="isExpanded"
      :aria-controls="`log-card-content-${props.log.timestamp}`"
      @click="onToggle"
      @keydown="handleKeydown"
    >
      <!-- Timestamp -->
      <div class="flex-shrink-0 flex flex-col items-center gap-1 min-w-[72px]" aria-hidden="true">
        <time class="text-xs font-mono text-gray-400 font-medium" :datetime="props.log.timestamp">
          {{ timestamp.time }}
        </time>
        <span class="text-[10px] text-gray-600 dark:text-gray-500 font-medium">{{ timestamp.date }}</span>
      </div>

      <!-- Level Badge & Message Preview -->
      <div class="flex-1 min-w-0">
        <div class="flex items-center gap-2 mb-1.5">
          <!-- Level Badge -->
          <span
            class="inline-flex items-center gap-1.5 px-2.5 py-0.5 rounded-full text-xs font-medium border transition-all duration-200"
            :class="levelConfig.class"
            role="status"
            :aria-label="`Nível: ${levelConfig.label}`"
          >
            <span class="w-1.5 h-1.5 rounded-full" :class="levelConfig.dotClass"></span>
            {{ levelConfig.label }}
          </span>

          <!-- Job Status Badge -->
          <span v-if="props.log.jobStatus" class="inline-flex items-center px-2 py-0.5 rounded text-[10px] font-medium bg-gray-800 text-gray-300 border border-gray-700 uppercase tracking-wider">
            {{ props.log.jobStatus }}
          </span>
        </div>

        <!-- Message Preview -->
        <p class="text-gray-300 text-sm line-clamp-1 truncate">{{ preview }}</p>
      </div>

      <!-- Chevron Icon -->
      <div class="flex-shrink-0 flex items-center justify-center w-10 h-10 text-gray-500 transition-transform duration-200 ease-out" :class="{ 'rotate-180': isExpanded }" aria-hidden="true">
        <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 9l-7 7-7-7" />
        </svg>
      </div>
    </button>
  </header>
</template>

<style scoped>
.log-card-header {
  /* Header doesn't need special styles - handled by parent card */
}

/* Focus visible for keyboard navigation */
button:focus-visible {
  outline: none;
  ring: 2px;
  ring-color: var(--color-accent);
  ring-offset: 2px;
  ring-offset-color: #111827;
}

/* Reduced motion support */
@media (prefers-reduced-motion: reduce) {
  .log-card-header button,
  .log-card-header svg {
    transition: none !important;
  }
}
</style>