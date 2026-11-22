<script setup>
import { computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'

const props = defineProps({
  to: {
    type: String,
    required: true
  },
  icon: {
    type: String,
    required: true
  },
  label: {
    type: String,
    required: true
  },
  isOpen: {
    type: Boolean,
    default: true
  }
})

const route = useRoute()
const router = useRouter()

const isActive = computed(() => {
  return route.name === props.to
})

function navigate() {
  router.push({ name: props.to })
}
</script>

<template>
  <li>
    <button
      @click="navigate"
      :class="[
        'flex items-center w-full p-3 rounded-lg transition-colors',
        isActive 
          ? 'bg-blue-100 text-blue-600 dark:bg-blue-900/30 dark:text-blue-400' 
          : 'text-gray-700 hover:bg-gray-200 dark:text-gray-300 dark:hover:bg-gray-700'
      ]"
    >
      <svg 
        xmlns="http://www.w3.org/2000/svg" 
        class="h-5 w-5 flex-shrink-0"
        fill="none" 
        viewBox="0 0 24 24" 
        stroke="currentColor"
      >
        <path 
          :class="isOpen ? '' : 'w-4 h-4'"
          stroke-linecap="round" 
          stroke-linejoin="round" 
          stroke-width="2"
          :d="getIconPath(icon)"
        />
      </svg>
      
      <span 
        v-if="isOpen" 
        class="ml-3 font-medium truncate"
      >
        {{ label }}
      </span>
    </button>
  </li>
</template>

<script>
// Define icon paths
const iconPaths = {
  home: "M3 12l2-2m0 0l7-7 7 7M5 10v10a1 1 0 001 1h3m10-11l2 2m-2-2v10a1 1 0 01-1 1h-3m-6 0a1 1 0 001-1v-4a1 1 0 011-1h2a1 1 0 011 1v4a1 1 0 001 1m-6 0h6",
  library: "M8 14v3m4-3v3m4-3v3M3 21h18M3 10h18M3 7l9-4 9 4M4 10h16v11H4V10z",
  tv: "M9.75 17L9 20l-1 1h8l-1-1-.75-3M3 13h18M5 17h14a2 2 0 002-2V5a2 2 0 00-2-2H5a2 2 0 00-2 2v10a2 2 0 002 2z",
  sync: "M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15",
  marker: "M15.232 5.232l3.536 3.536m-2.036-5.464a2.5 2.5 0 113.536 3.536L6.5 21.036H3v-3.572L16.732 3.732z",
  terminal: "M8 9l3 3m0 0l3-3m-3 3v12m0-12V3",
  settings: "M10.325 4.317c.426-1.756 2.924-1.756 3.35 0a1.724 1.724 0 002.573 1.066c1.543-.94 3.31.826 2.37 2.37a1.724 1.724 0 001.065 2.572c1.756.426 1.756 2.924 0 3.35a1.724 1.724 0 00-2.572 1.065c-.94 1.543-3.31.826-2.37 2.37a1.724 1.724 0 00-1.065 2.572c-.426 1.756-2.924 1.756-3.35 0a1.724 1.724 0 00-2.572-1.065c-1.543.94-3.31-.826-2.37-2.37a1.724 1.724 0 00-1.065-2.572c-1.756-.426-1.756-2.924 0-3.35a1.724 1.724 0 002.572-1.065c.94-1.543 3.31-.826 2.37-2.37a1.724 1.724 0 001.065-2.572z",
}

function getIconPath(iconName) {
  return iconPaths[iconName] || "M12 6v6m0 0v6m0-6h6m-6 0H6"
}
</script>