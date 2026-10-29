import { ref, computed, watch } from "vue";
import { useRoute, useRouter } from "vue-router";
import { apiFetch, extractErrorMessage } from "../api";

/**
 * Composable para gerenciar o estado e lógica de exibição de séries inline
 * na view de Bibliotecas.
 */
export function useLibrarySeries() {
  const route = useRoute();
  const router = useRouter();

  // Series state for inline expansion
  const expandedLibraryKey = ref(null);
  const seriesList = ref([]);
  const seriesTotalSize = ref(0);
  const seriesPage = ref(1);
  const seriesPageSize = ref(24);
  const seriesQuery = ref("");
  const seriesLoading = ref(false);
  const seriesError = ref("");

  const totalSeriesPages = computed(() =>
    Math.max(1, Math.ceil(seriesTotalSize.value / seriesPageSize.value)),
  );

  // Debounce timer for search
  let seriesDebounceTimer;

  // Computed to get the expanded library object
  const expandedLibrary = computed(() => {
    if (!expandedLibraryKey.value) return null;
    return null; // Will be set by parent via libraries array
  });

  async function loadSeries(libraryKey) {
    if (!libraryKey) return;
    seriesLoading.value = true;
    seriesError.value = "";
    try {
      const params = new URLSearchParams({
        page: String(seriesPage.value),
        pageSize: String(seriesPageSize.value),
      });
      if (seriesQuery.value.trim()) params.set("query", seriesQuery.value.trim());

      const res = await apiFetch(`/api/series/${encodeURIComponent(libraryKey)}?${params}`);

      if (res.status === 502) {
        seriesError.value =
          (await extractErrorMessage(res)) ||
          "Não foi possível consultar o Plex. Verifique a conexão com o servidor.";
        seriesList.value = [];
        return;
      }

      if (res.ok) {
        const data = await res.json();
        seriesList.value = data.items ?? [];
        seriesTotalSize.value = data.totalSize ?? 0;
      } else {
        seriesError.value = "Erro ao carregar séries";
      }
    } catch {
      seriesError.value = "Erro de conexão";
    } finally {
      seriesLoading.value = false;
    }
  }

  function toggleLibrary(libraryKey) {
    if (expandedLibraryKey.value === libraryKey) {
      // Collapse
      expandedLibraryKey.value = null;
      seriesList.value = [];
      seriesTotalSize.value = 0;
      seriesPage.value = 1;
      seriesQuery.value = "";
      seriesError.value = "";
      router.replace({ name: "libraries" });
    } else {
      // Expand new library
      expandedLibraryKey.value = libraryKey;
      seriesPage.value = 1;
      seriesQuery.value = "";
      seriesError.value = "";
      router.replace({ name: "libraries", params: { key: libraryKey } });
    }
  }

  function goToSeriesPage(page) {
    if (page < 1 || page > totalSeriesPages.value || page === seriesPage.value) return;
    seriesPage.value = page;
  }

  function openSeriesMarkers(series) {
    router.push({ name: "series-markers", params: { seriesKey: series.ratingKey } });
  }

  // Watch route param for deep-linking
  watch(
    () => route.params.key,
    (newKey) => {
      if (newKey && newKey !== expandedLibraryKey.value) {
        expandedLibraryKey.value = newKey;
        seriesPage.value = 1;
        seriesQuery.value = "";
      } else if (!newKey && expandedLibraryKey.value) {
        expandedLibraryKey.value = null;
      }
    },
  );

  // Debounced search
  watch(seriesQuery, () => {
    clearTimeout(seriesDebounceTimer);
    seriesDebounceTimer = setTimeout(() => {
      seriesPage.value = 1;
      if (expandedLibraryKey.value) {
        // loadSeries will be called by parent
      }
    }, 300);
  });

  // Expose loadSeries for parent to call with libraries array
  const loadSeriesForKey = (libraryKey, libraries) => loadSeries(libraryKey, libraries);

  return {
    // State
    expandedLibraryKey,
    seriesList,
    seriesTotalSize,
    seriesPage,
    seriesPageSize,
    seriesQuery,
    seriesLoading,
    seriesError,
    totalSeriesPages,
    // Computed
    expandedLibrary,
    // Methods
    loadSeries: loadSeriesForKey,
    toggleLibrary,
    goToSeriesPage,
    openSeriesMarkers,
  };
}
