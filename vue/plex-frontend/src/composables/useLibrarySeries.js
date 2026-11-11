import { ref, computed, watch } from "vue";
import { useRoute, useRouter } from "vue-router";
import { apiFetch, extractErrorMessage } from "../api";
import { useInfiniteScroll } from "./useInfiniteScroll";

/**
 * Composable para gerenciar o estado e lógica de exibição de séries inline
 * na view de Bibliotecas com infinite scroll contínuo estilo Netflix.
 */
export function useLibrarySeries() {
  const route = useRoute();
  const router = useRouter();

  const PAGE_SIZE = 24;

  // Estado das séries para expansão inline
  const expandedLibraryKey = ref(null);
  const seriesList = ref([]);
  const seriesTotalSize = ref(0);
  const internalPage = ref(1);
  const hasMore = ref(true);
  const seriesQuery = ref("");
  const seriesLoading = ref(false); // Carregamento inicial / reset
  const isLoadingMore = ref(false); // Carregamento de páginas subsequentes
  const seriesError = ref("");

  let seriesDebounceTimer = null;

  // Placeholder para compatibilidade caso consumido externamente
  const expandedLibrary = computed(() => null);

  async function fetchSeriesPage(libraryKey, pageToFetch) {
    if (!libraryKey) return null;

    const params = new URLSearchParams({
      page: String(pageToFetch),
      pageSize: String(PAGE_SIZE),
    });
    if (seriesQuery.value.trim()) {
      params.set("query", seriesQuery.value.trim());
    }

    const res = await apiFetch(`/api/series/${encodeURIComponent(libraryKey)}?${params}`);

    if (res.status === 502) {
      const msg =
        (await extractErrorMessage(res)) ||
        "Não foi possível consultar o Plex. Verifique a conexão com o servidor.";
      throw new Error(msg);
    }

    if (!res.ok) {
      throw new Error("Erro ao carregar séries");
    }

    return await res.json();
  }

  async function loadInitialSeries(libraryKey) {
    if (!libraryKey) return;

    seriesLoading.value = true;
    seriesError.value = "";
    internalPage.value = 1;
    hasMore.value = true;

    try {
      const data = await fetchSeriesPage(libraryKey, 1);
      if (!data) return;

      const items = data.items ?? [];
      seriesList.value = items;
      seriesTotalSize.value = data.totalSize ?? 0;
      hasMore.value = seriesList.value.length < seriesTotalSize.value && items.length >= PAGE_SIZE;
    } catch (err) {
      seriesError.value = err.message || "Erro de conexão";
      seriesList.value = [];
      seriesTotalSize.value = 0;
      hasMore.value = false;
    } finally {
      seriesLoading.value = false;
    }
  }

  async function loadNextPage() {
    if (!expandedLibraryKey.value || !hasMore.value || seriesLoading.value || isLoadingMore.value) {
      return false;
    }

    isLoadingMore.value = true;
    const nextPage = internalPage.value + 1;

    try {
      const data = await fetchSeriesPage(expandedLibraryKey.value, nextPage);
      if (!data) return false;

      const newItems = data.items ?? [];
      if (newItems.length === 0) {
        hasMore.value = false;
        return false;
      }

      // Evita duplicatas por ratingKey
      const existingKeys = new Set(seriesList.value.map((s) => s.ratingKey));
      const deduplicated = newItems.filter((s) => !existingKeys.has(s.ratingKey));

      seriesList.value = [...seriesList.value, ...deduplicated];
      seriesTotalSize.value = data.totalSize ?? seriesList.value.length;
      internalPage.value = nextPage;

      if (seriesList.value.length >= seriesTotalSize.value || newItems.length < PAGE_SIZE) {
        hasMore.value = false;
      }

      return hasMore.value;
    } catch (err) {
      seriesError.value = err.message || "Erro ao carregar mais séries";
      return false;
    } finally {
      isLoadingMore.value = false;
    }
  }

  const { sentinelRef, loadMore } = useInfiniteScroll({
    onLoadMore: loadNextPage,
    hasMore,
    isLoading: isLoadingMore,
    rootMargin: "250px",
  });

  function toggleLibrary(libraryKey) {
    if (expandedLibraryKey.value === libraryKey) {
      // Recolher
      expandedLibraryKey.value = null;
      seriesList.value = [];
      seriesTotalSize.value = 0;
      internalPage.value = 1;
      hasMore.value = false;
      seriesQuery.value = "";
      seriesError.value = "";
      router.replace({ name: "libraries" });
    } else {
      // Expandir nova biblioteca
      expandedLibraryKey.value = libraryKey;
      seriesQuery.value = "";
      seriesError.value = "";
      loadInitialSeries(libraryKey);
      router.replace({ name: "libraries", params: { key: libraryKey } });
    }
  }

  function openSeriesMarkers(series) {
    router.push({ name: "series-markers", params: { seriesKey: series.ratingKey } });
  }

  // Monitorar parâmetro de rota para deep-linking
  watch(
    () => route.params.key,
    (newKey) => {
      if (newKey && newKey !== expandedLibraryKey.value) {
        expandedLibraryKey.value = newKey;
        seriesQuery.value = "";
        loadInitialSeries(newKey);
      } else if (!newKey && expandedLibraryKey.value) {
        expandedLibraryKey.value = null;
        seriesList.value = [];
        hasMore.value = false;
      }
    },
  );

  // Busca com debounce - reseta para o início
  watch(seriesQuery, () => {
    clearTimeout(seriesDebounceTimer);
    seriesDebounceTimer = setTimeout(() => {
      if (expandedLibraryKey.value) {
        loadInitialSeries(expandedLibraryKey.value);
      }
    }, 300);
  });

  return {
    // Estado
    expandedLibraryKey,
    seriesList,
    seriesTotalSize,
    seriesQuery,
    seriesLoading,
    isLoadingMore,
    seriesError,
    hasMore,
    sentinelRef,
    // Computados
    expandedLibrary,
    // Métodos
    loadSeries: loadInitialSeries,
    loadMore,
    toggleLibrary,
    openSeriesMarkers,
  };
}
