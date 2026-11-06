import { defineStore } from "pinia";
import { ref } from "vue";

export const useUiStore = defineStore("ui", () => {
  const libraryTitle = ref("");
  const seriesTitle = ref("");

  function setLibraryTitle(title) {
    libraryTitle.value = title || "";
  }

  function setSeriesTitle(title) {
    seriesTitle.value = title || "";
  }

  function reset() {
    libraryTitle.value = "";
    seriesTitle.value = "";
  }

  return {
    libraryTitle,
    seriesTitle,
    setLibraryTitle,
    setSeriesTitle,
    reset,
  };
});
