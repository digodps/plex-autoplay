<script setup>
import { ref, computed, watch, nextTick, onMounted } from 'vue'
import { useI18n } from 'vue-i18n'
import LogCard from './cards/LogCard.vue'
import { parseHierarchy, formatTimestamp } from './cards/logHierarchy.js'

const { t } = useI18n()

const props = defineProps({
  items: {
    type: Array,
    default: () => [],
  },
  groupBy: {
    type: String,
    default: 'date',
  },
  loading: {
    type: Boolean,
    default: false,
  },
})

const emit = defineEmits(['toggle-expand'])

const expandedIndex = ref(null)
const focusedIndex = ref(-1)

const groups = computed(() => {
  if (!props.items.length) return []

  const groupMap = new Map()
  
  for (const [index, log] of props.items.entries()) {
    let key
    if (props.groupBy === 'date') {
      key = formatTimestamp(log.timestamp).date || t("logs.noDate")
    } else if (props.groupBy === 'status') {
      key = log.jobStatus || log.level || t("logs.noStatus")
    } else if (props.groupBy === 'level') {
      key = log.level || t("logs.noType")
    } else if (props.groupBy === 'library') {
      const hierarchy = parseHierarchy(log.message, log.suggestion)
      key = hierarchy.library || t("logs.noLibrary")
    } else {
      key = formatTimestamp(log.timestamp).date || t("logs.noDate")
    }
    
    if (!groupMap.has(key)) {
      groupMap.set(key, { key, items: [], indices: [] })
    }
    const group = groupMap.get(key)
    group.items.push(log)
    group.indices.push(index)
  }
  
  return [...groupMap.values()]
    .sort((a, b) => b.key.localeCompare(a.key))
})

function toggleCard(globalIndex) {
  expandedIndex.value = expandedIndex.value === globalIndex ? null : globalIndex
  emit('toggle-expand', globalIndex)
}

function handleKeydown(event, globalIndex) {
  const totalItems = props.items.length
  
  switch (event.key) {
    case 'ArrowDown':
      event.preventDefault()
      focusNext(globalIndex, totalItems)
      break
    case 'ArrowUp':
      event.preventDefault()
      focusPrev(globalIndex)
      break
    case 'Home':
      event.preventDefault()
      focusedIndex.value = 0
      break
    case 'End':
      event.preventDefault()
      focusedIndex.value = totalItems - 1
      break
    case 'Escape':
      if (expandedIndex.value !== null) {
        event.preventDefault()
        expandedIndex.value = null
      }
      break
    case 'Enter':
    case ' ':
      if (!event.target.closest('button, a, input')) {
        event.preventDefault()
        toggleCard(globalIndex)
      }
      break
  }
}

function focusNext(currentIndex, total) {
  const next = currentIndex + 1 < total ? currentIndex + 1 : 0
  focusedIndex.value = next
  scrollToCard(next)
}

function focusPrev(currentIndex) {
  const total = props.items.length
  const prev = currentIndex > 0 ? currentIndex - 1 : total - 1
  focusedIndex.value = prev
  scrollToCard(prev)
}

function scrollToCard(index) {
  nextTick(() => {
    const el = document.querySelector(`[data-testid="log-card-${index}"]`)
    if (el) {
      el.scrollIntoView({ behavior: 'smooth', block: 'nearest' })
      const button = el.querySelector('button')
      if (button) button.focus()
    }
  })
}

function getGlobalIndex(groupIndex, itemIndex) {
  let count = 0
  for (let i = 0; i < groupIndex; i++) {
    count += groups.value[i].items.length
  }
  return count + itemIndex
}

watch(() => props.items, () => {
  if (expandedIndex.value !== null && expandedIndex.value >= props.items.length) {
    expandedIndex.value = null
  }
})

onMounted(() => {
  if (props.items.length > 0) {
    focusedIndex.value = 0
  }
})
</script>

<template>
  <div class="log-carousel-view" data-testid="log-carousel-view">
    <!-- Loading State -->
    <div v-if="loading" class="flex items-center justify-center py-12">
      <div class="flex flex-col items-center gap-4 text-gray-400">
        <div class="w-8 h-8 border-3 border-accent/30 border-t-accent rounded-full animate-spin" aria-hidden="true"></div>
        <span class="text-sm">{{ t("logs.loading") }}</span>
      </div>
    </div>

    <!-- Empty State -->
    <div v-else-if="!loading && !props.items.length" class="mx-auto max-w-6xl px-4 py-12 text-center" data-testid="log-empty">
      <div class="flex flex-col items-center gap-4">
        <svg class="w-16 h-16 text-gray-700 dark:text-gray-600" fill="none" stroke="currentColor" viewBox="0 0 24 24" aria-hidden="true">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.5" d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
        </svg>
        <div>
          <p class="text-lg font-medium text-gray-300">{{ t("logs.noLogs") }}</p>
          <p class="text-sm text-gray-500 mt-1">{{ t("logs.noLogsHelp") }}</p>
        </div>
      </div>
    </div>

    <!-- Carousel View -->
    <div v-else class="space-y-6">
      <template v-for="(group, groupIndex) in groups" :key="group.key">
        <!-- Group Header -->
        <header class="flex items-center justify-between px-2" :data-testid="`log-group-header-${group.key}`">
          <h3 class="text-lg font-semibold text-gray-200 tracking-tight">{{ group.key }}</h3>
          <span class="text-sm font-medium text-gray-500 bg-gray-800/50 px-3 py-1 rounded-full border border-gray-700/50">
            {{ group.items.length }} {{ group.items.length === 1 ? t("logs.singleLog") : t("logs.multipleLogs") }}
          </span>
        </header>

        <!-- Cards Grid -->
        <div class="space-y-4" role="list" :aria-label="t('logs.groupAriaLabel', { group: group.key })">
          <LogCard
            v-for="(log, itemIndex) in group.items"
            :key="log.timestamp + '-' + itemIndex"
            :log="log"
            :index="getGlobalIndex(groupIndex, itemIndex)"
            :is-expanded="expandedIndex === getGlobalIndex(groupIndex, itemIndex)"
            :on-toggle="() => toggleCard(getGlobalIndex(groupIndex, itemIndex))"
            :tabindex="focusedIndex === getGlobalIndex(groupIndex, itemIndex) ? 0 : -1"
            role="listitem"
            @keydown="handleKeydown($event, getGlobalIndex(groupIndex, itemIndex), groupIndex, itemIndex)"
          />
        </div>
      </template>
    </div>
  </div>
</template>

<style scoped>
.log-carousel-view {
  /* Container for the carousel */
}

/* Group header separator line */
[data-testid^="log-group-header-"]::after {
  content: '';
  flex: 1;
  height: 1px;
  background: linear-gradient(90deg, transparent, #374151, transparent);
  margin-left: 1rem;
}

/* Scrollbar styling for webkit */
.log-carousel-view ::-webkit-scrollbar {
  width: 8px;
}

.log-carousel-view ::-webkit-scrollbar-track {
  background: #111827;
  border-radius: 4px;
}

.log-carousel-view ::-webkit-scrollbar-thumb {
  background: #374151;
  border-radius: 4px;
  border: 2px solid #111827;
}

.log-carousel-view ::-webkit-scrollbar-thumb:hover {
  background: #4b5563;
}

/* Focus visible for card navigation */
.log-carousel-view :focus-visible {
  outline: none;
  ring: 2px;
  ring-color: var(--color-accent);
  ring-offset: 2px;
  ring-offset-color: #111827;
}

/* Reduced motion */
@media (prefers-reduced-motion: reduce) {
  .log-carousel-view * {
    animation: none !important;
    transition: none !important;
  }
  .log-carousel-view .log-card {
    animation: none !important;
  }
}
</style>