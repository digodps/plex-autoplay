<script setup>
import { onMounted } from 'vue'
import { useSidebarStore } from '@/stores/sidebar'
import SidebarItem from './SidebarItem.vue'

const props = defineProps({
  menuGroups: {
    type: Array,
    required: true
  }
})

const sidebar = useSidebarStore()

// Toggle sidebar on mobile
function toggleSidebar() {
  sidebar.toggleSidebar()
}

onMounted(() => {
  // Set initial state based on screen size
  if (window.innerWidth < 768) {
    sidebar.closeSidebar()
  } else {
    sidebar.openSidebar()
  }
})
</script>

<template>
  <aside 
    :class="[
      'fixed md:static z-30 h-screen transition-all duration-300 ease-in-out',
      sidebar.isOpen ? 'w-64' : 'w-20'
    ]"
  >
    <div class="bg-gray-50 dark:bg-gray-800 border-r border-gray-200 dark:border-gray-700 h-full flex flex-col">
      <!-- Sidebar header -->
      <div class="flex items-center justify-between p-4 border-b border-gray-200 dark:border-gray-700">
        <div v-if="sidebar.isOpen" class="flex items-center space-x-2">
          <div class="bg-blue-600 text-white p-2 rounded-lg">
            <svg xmlns="http://www.w3.org/2000/svg" class="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 10l4.553-2.276A1 1 0 0121 8.618v6.764a1 1 0 01-1.447.894L15 14M5 18h8a2 2 0 002-2V8a2 2 0 00-2-2H5a2 2 0 00-2 2v8a2 2 0 002 2z" />
            </svg>
          </div>
          <span class="text-xl font-bold text-gray-800 dark:text-white">PlexAutoplay</span>
        </div>
        
        <button 
          @click="toggleSidebar"
          class="p-2 rounded-lg hover:bg-gray-200 dark:hover:bg-gray-700 transition-colors"
          :class="sidebar.isOpen ? 'md:hidden' : ''"
        >
          <svg 
            xmlns="http://www.w3.org/2000/svg" 
            class="h-5 w-5 text-gray-600 dark:text-gray-300" 
            fill="none" 
            viewBox="0 0 24 24" 
            stroke="currentColor"
          >
            <path 
              :class="sidebar.isOpen ? 'hidden' : ''" 
              stroke-linecap="round" 
              stroke-linejoin="round" 
              stroke-width="2" 
              d="M4 6h16M4 12h16M4 18h16" 
            />
            <path 
              :class="sidebar.isOpen ? '' : 'hidden'" 
              stroke-linecap="round" 
              stroke-linejoin="round" 
              stroke-width="2" 
              d="M6 18L18 6M6 6l12 12" 
            />
          </svg>
        </button>
      </div>

      <!-- Navigation items -->
      <nav class="flex-1 py-4 overflow-y-auto">
        <div v-for="(group, groupIndex) in props.menuGroups" :key="groupIndex" class="px-2">
          <div v-if="sidebar.isOpen && group.name" class="text-xs font-semibold text-gray-500 dark:text-gray-400 uppercase tracking-wider py-2 px-2">
            {{ group.name }}
          </div>
          <ul class="space-y-1">
            <SidebarItem 
              v-for="(item, itemIndex) in group.menuItems" 
              :key="itemIndex"
              :to="item.to"
              :icon="item.icon" 
              :label="item.name"  
              :is-open="sidebar.isOpen"
            />
          </ul>
        </div>
      </nav>

      <!-- Sidebar footer -->
      <div class="p-4 border-t border-gray-200 dark:border-gray-700">
        <div class="flex items-center space-x-3">
          <img 
            src="/avatar.png" 
            alt="User avatar" 
            class="w-10 h-10 rounded-full object-cover"
          />
          <div v-if="sidebar.isOpen" class="flex-1 min-w-0">
            <p class="text-sm font-medium text-gray-900 dark:text-white truncate">Admin User</p>
            <p class="text-xs text-gray-500 dark:text-gray-400 truncate">admin@example.com</p>
          </div>
        </div>
      </div>
    </div>
  </aside>
</template>

