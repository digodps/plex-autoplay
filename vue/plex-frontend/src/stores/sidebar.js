import { defineStore } from "pinia";
import { ref } from "vue";

const PREF_KEY = "sidebar.collapsed.v2";

export const useSidebarStore = defineStore("sidebar", () => {
  const isCollapsed = ref(localStorage.getItem(PREF_KEY) === "true");

  function toggleCollapsed() {
    isCollapsed.value = !isCollapsed.value;
    localStorage.setItem(PREF_KEY, isCollapsed.value ? "true" : "false");
  }

  return {
    isCollapsed,
    toggleCollapsed,
  };
});