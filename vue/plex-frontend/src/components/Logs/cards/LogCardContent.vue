<script setup>
import { computed, ref, h } from 'vue'
import { 
  parseHierarchy, 
  parseFullHierarchy, 
  getHierarchyTree, 
  hasHierarchyTree,
  getHierarchyParts, 
  formatTimestamp, 
  getLevelConfig 
} from './logHierarchy.js'

const props = defineProps({
  log: {
    type: Object,
    required: true,
  },
})

const hierarchy = computed(() => parseHierarchy(props.log.message, props.log.suggestion))
const fullHierarchy = computed(() => parseFullHierarchy(props.log.message, props.log.suggestion))
const hierarchyTree = computed(() => getHierarchyTree(fullHierarchy.value))
const hasTree = computed(() => hasHierarchyTree(fullHierarchy.value))
const hierarchyParts = computed(() => getHierarchyParts(hierarchy.value))
const timestamp = computed(() => formatTimestamp(props.log.timestamp))
const levelConfig = computed(() => getLevelConfig(props.log.level))

// Track expanded state for each hierarchy node (by unique key)
const expandedNodes = ref(new Set())

function getNodeKey(node, level, parentKey = '') {
  return `${parentKey}|${level}|${node.name}`
}

function toggleNode(node, level, parentKey = '') {
  const key = getNodeKey(node, level, parentKey)
  if (expandedNodes.value.has(key)) {
    expandedNodes.value.delete(key)
  } else {
    expandedNodes.value.add(key)
  }
}

const copyJobId = async () => {
  if (props.log.jobId) {
    await navigator.clipboard.writeText(props.log.jobId)
  }
}

// Recursive Hierarchy Node Component
const HierarchyNode = {
  name: 'HierarchyNode',
  props: {
    node: { type: Object, required: true },
    level: { type: Number, required: true },
    parentKey: { type: String, required: true },
    isLast: { type: Boolean, default: false }
  },
  setup(props) {
    const indentSize = 16 // px per level
    
    const isLib = props.level === 0
    const isSeries = props.level === 1
    
    const children = isLib ? props.node.series : (isSeries ? props.node.episodes : [])
    const hasChildren = children && children.length > 0
    
    const nodeKey = `${props.parentKey}|${props.level}|${props.node.name}`
    const isExpanded = () => expandedNodes.value.has(nodeKey)
    
    const handleClick = (e) => {
      e.stopPropagation()
      if (hasChildren) {
        toggleNode(props.node, props.level, props.parentKey)
      }
    }
    
    const handleKeydown = (e) => {
      if (e.key === 'Enter' || e.key === ' ') {
        e.preventDefault()
        e.stopPropagation()
        if (hasChildren) {
          toggleNode(props.node, props.level, props.parentKey)
        }
      }
    }
    
    const levelStyles = {
      paddingLeft: `${props.level * indentSize + 8}px`,
      borderLeft: props.level > 0 ? '1px solid rgba(55, 65, 81, 0.5)' : 'none',
    }
    
    const valueClass = 'text-gray-200'
    
    return () => {
      const nodeElements = []
      
      // Connector line
      if (props.level > 0) {
        nodeElements.push(
          h('div', {
            class: 'flex-shrink-0 w-4 h-full relative',
            style: { marginTop: '2px' }
          }, [
            h('div', {
              class: 'absolute left-1/2 top-0 bottom-0 w-px bg-gray-700/50',
              style: { transform: 'translateX(-50%)' }
            }),
            !props.isLast && h('div', {
              class: 'absolute left-1/2 bottom-0 w-4 h-px bg-gray-700/50',
              style: { transform: 'translateX(-50%)' }
            })
          ])
        )
      }
      
      // Expand/Collapse Chevron
      if (hasChildren) {
        nodeElements.push(
          h('div', {
            class: 'flex-shrink-0 w-6 h-6 flex items-center justify-center',
            style: { marginTop: '2px' }
          }, [
            h('svg', {
              class: `w-4 h-4 text-gray-500 transition-transform ${isExpanded() ? 'rotate-90' : ''}`,
              fill: 'none',
              stroke: 'currentColor',
              viewBox: '0 0 24 24',
              'aria-hidden': 'true'
            }, [
              h('path', {
                'stroke-linecap': 'round',
                'stroke-linejoin': 'round',
                'stroke-width': '2',
                d: 'M9 5l7 7-7 7'
              })
            ])
          ])
        )
      } else {
        nodeElements.push(
          h('div', {
            class: 'flex-shrink-0 w-6 h-6',
            style: { marginTop: '2px' }
          })
        )
      }
      
      // Icon
      nodeElements.push(
        h('div', {
          class: 'flex-shrink-0 w-6 h-6 flex items-center justify-center text-lg',
          style: { marginTop: '2px' }
        }, props.node.icon)
      )
      
      // Label & Value
      nodeElements.push(
        h('div', { class: 'flex-1 min-w-0' }, [
          h('div', { class: 'flex items-center gap-2' }, [
            h('span', { 
              class: `text-xs font-medium text-gray-500 uppercase tracking-wide ${isLib ? 'font-semibold' : ''}`
            }, isLib ? 'Biblioteca' : (isSeries ? 'Série' : 'Episódio')),
            h('span', { class: `text-sm truncate ${valueClass}` }, props.node.name)
          ])
        ])
      )
      
      // Children
      if (hasChildren && isExpanded()) {
        nodeElements.push(
          h('div', { 
            class: 'hierarchy-children', 
            role: 'group',
            style: { marginLeft: '24px' }
          }, children.map((child, idx) => 
            h(HierarchyNode, {
              key: `${nodeKey}|${idx}-${child.name}`,
              node: child,
              level: props.level + 1,
              parentKey: nodeKey,
              isLast: idx === children.length - 1
            })
          ))
        )
      }
      
      return h('div', {
        class: 'hierarchy-node flex items-start gap-3 py-1.5 cursor-pointer hover:bg-gray-800/50 rounded transition-colors',
        style: levelStyles,
        role: 'treeitem',
        'aria-level': props.level + 1,
        'aria-expanded': hasChildren ? isExpanded() : undefined,
        tabindex: hasChildren ? 0 : -1,
        onClick: handleClick,
        onKeydown: handleKeydown
      }, nodeElements)
    }
  }
}
</script>

<template>
  <div class="log-card-content animate-in slide-in-from-top-2 fade-in duration-200 ease-out" role="region" aria-label="Detalhes do log">
    <!-- Hierarchy Tree (New Nested Display) -->
    <div v-if="hasTree" class="mb-4 p-4 rounded-lg bg-gray-900/50 border border-gray-800">
      <div class="flex items-center gap-2 text-xs font-medium text-gray-400 uppercase tracking-wider mb-3">
        <span>Hierarquia do processamento</span>
      </div>
      <div class="hierarchy-tree" role="tree" aria-label="Hierarquia de processamento">
        <HierarchyNode
          v-for="(library, libIndex) in hierarchyTree"
          :key="`lib-${libIndex}-${library.name}`"
          :node="library"
          :level="0"
          :parent-key="''"
          :is-last="libIndex === hierarchyTree.length - 1"
        />
      </div>
    </div>

    <!-- Fallback: Flat Hierarchy Display (Backward Compatibility) -->
    <div v-else-if="hierarchyParts.length > 0" class="mb-4 p-4 rounded-lg bg-gray-900/50 border border-gray-800">
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
            class="text-xs text-gray-400 hover:text-white transition-colors flex items-center gap-1"
            title="Copiar Job ID"
            aria-label="Copiar Job ID"
            @click="copyJobId"
          >
            <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 16H6a2 2 0 01-2-2V6a2 2 0 012-2h8a2 2 0 012 2v2m-6 12h8a2 2 0 002-2v-8a2 2 0 00-2-2h-8a2 2 0 00-2 2v8a2 2 0 002 2z" /></svg>
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

/* Hierarchy tree styles */
.hierarchy-tree {
  font-family: inherit;
}

.hierarchy-node {
  user-select: none;
}

.hierarchy-node:focus-visible {
  outline: none;
  ring: 2px;
  ring-color: var(--color-accent);
  ring-offset: 2px;
  ring-offset-color: #1f2937;
}

/* Chevron animation */
.hierarchy-node svg {
  transition: transform 150ms ease-out;
}

/* Reduced motion */
@media (prefers-reduced-motion: reduce) {
  .hierarchy-node svg,
  .hierarchy-node {
    transition: none !important;
  }
}

/* Dark theme adjustments */
.dark .hierarchy-node:hover {
  background-color: rgba(55, 65, 81, 0.5);
}
</style>