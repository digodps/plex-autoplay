import { defineStore } from "pinia";
import { ref } from "vue";

const empty = () => ({ libraryTitle: "", seriesTitle: "", title: "" });

export const useUiStore = defineStore("ui", () => {
  const libraryTitle = ref("");
  const seriesTitle = ref("");
  const breadcrumbTitle = ref("");

  function setLibraryTitle(title) {
    libraryTitle.value = title;
  }

  function setSeriesTitle(title) {
    seriesTitle.value = title;
  }

  function reset() {
    libraryTitle.value = "";
    seriesTitle.value = "";
    breadcrumbTitle.value = "";
  }

  return {
    libraryTitle,
    seriesTitle,
    breadcrumbTitle,
    setLibraryTitle,
    setSeriesTitle,
    reset,
  };
});

export default empty();
