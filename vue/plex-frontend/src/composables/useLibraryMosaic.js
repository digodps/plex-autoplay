import { ref, computed, onUnmounted } from "vue";
import { apiFetch, extractErrorMessage } from "../api";

/**
 * Composable for managing library mosaic posters with hover shuffle animation.
 * Fetches up to 6 series posters and cycles them on hover.
 */
export function useLibraryMosaic(libraryKey) {
  const posters = ref([]);
  const isLoading = ref(false);
  const error = ref("");
  const isHovered = ref(false);

  let shuffleInterval = null;
  let cache = useLibraryMosaic.cache || (useLibraryMosaic.cache = new Map());
  let abortController = null;

  // Reduced motion preference
  const prefersReducedMotion = computed(() => {
    if (typeof window === "undefined") return false;
    return window.matchMedia("(prefers-reduced-motion: reduce)").matches;
  });

  async function fetchPosters() {
    if (!libraryKey) return;

    // Check cache first
    if (cache.has(libraryKey)) {
      posters.value = cache.get(libraryKey);
      return;
    }

    isLoading.value = true;
    error.value = "";

    // Cancel any in-flight request
    if (abortController) {
      abortController.abort();
    }
    abortController = new AbortController();

    try {
      const params = new URLSearchParams({
        page: "1",
        pageSize: "6",
      });

      const res = await apiFetch(`/api/series/${encodeURIComponent(libraryKey)}?${params}`, {
        signal: abortController.signal,
      });

      if (res.status === 502) {
        const msg =
          (await extractErrorMessage(res)) ||
          "Não foi possível consultar o Plex. Verifique a conexão com o servidor.";
        throw new Error(msg);
      }

      if (!res.ok) {
        throw new Error("Erro ao carregar séries");
      }

      const data = await res.json();
      const items = data.items ?? [];

      // Map to poster format: need thumb (poster URL) and title
      const posterData = items
        .filter((s) => s.thumb) // Only series with posters
        .slice(0, 6)
        .map((s) => ({
          ratingKey: s.ratingKey,
          thumb: s.thumb,
          title: s.title || "Série sem título",
        }));

      posters.value = posterData;
      cache.set(libraryKey, posterData);
    } catch (err) {
      if (err.name === "AbortError") return;
      error.value = err.message || "Erro de conexão";
      posters.value = [];
    } finally {
      isLoading.value = false;
    }
  }

  function startHover() {
    if (prefersReducedMotion.value) return;
    if (shuffleInterval) return;
    isHovered.value = true;

    // Shuffle posters every 400ms
    shuffleInterval = setInterval(() => {
      // Create shuffled copy for animation
      posters.value = [...posters.value].sort(() => Math.random() - 0.5);
    }, 400);
  }

  function stopHover() {
    if (shuffleInterval) {
      clearInterval(shuffleInterval);
      shuffleInterval = null;
    }
    isHovered.value = false;

    // Restore original order (cached version)
    if (cache.has(libraryKey)) {
      posters.value = [...cache.get(libraryKey)];
    }
  }

  // Cleanup on unmount
  onUnmounted(() => {
    stopHover();
    if (abortController) {
      abortController.abort();
    }
  });

  // Initial fetch
  fetchPosters();

  return {
    posters,
    isLoading,
    error,
    isHovered,
    startHover,
    stopHover,
    fetchPosters, // Expose for manual refresh if needed
  };
}