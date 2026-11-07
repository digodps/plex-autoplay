import { defineStore } from "pinia";
import { ref } from "vue";

export const useSidebarStore = defineStore("sidebar", () => {
  const isSidebarOpen = ref(false);
  const page = ref("Dashboard");
  const selected = ref("");
  const isLoading = ref(false);

  function toggleSidebar() {
    isSidebarOpen.value = !isSidebarOpen.value;
  }

  function closeSidebar() {
    isSidebarOpen.value = false;
  }

  function setPage(next) {
    page.value = next;
  }

  return {
    isSidebarOpen,
    page,
    selected,
    isLoading,
    toggleSidebar,
    closeSidebar,
    setPage,
  };
});