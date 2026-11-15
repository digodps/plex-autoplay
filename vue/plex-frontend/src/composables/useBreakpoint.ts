import { ref, computed, onMounted, onUnmounted } from "vue";

export type ViewMode = "desktop" | "tablet" | "mobile";

export function useBreakpoint() {
  const viewMode = ref<ViewMode>("desktop");

  function update() {
    const width = window.innerWidth;
    if (width >= 1024) {
      viewMode.value = "desktop";
    } else if (width >= 768) {
      viewMode.value = "tablet";
    } else {
      viewMode.value = "mobile";
    }
  }

  onMounted(() => {
    update();
    window.addEventListener("resize", update);
  });

  onUnmounted(() => {
    window.removeEventListener("resize", update);
  });

  const isDesktop = computed(() => viewMode.value === "desktop");
  const isTablet = computed(() => viewMode.value === "tablet");
  const isMobile = computed(() => viewMode.value === "mobile");

  return { viewMode, isDesktop, isTablet, isMobile, update };
}
