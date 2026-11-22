import { defineStore } from 'pinia';
import { ref } from 'vue';

export const useSidebarStore = defineStore('sidebar', () => {
  const isOpen = ref(true);

  function toggleSidebar() {
    isOpen.value = !isOpen.value;
  }

  function openSidebar() {
    isOpen.value = true;
  }

  function closeSidebar() {
    isOpen.value = false;
  }

  return { isOpen, toggleSidebar, openSidebar, closeSidebar };
});
