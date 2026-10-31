import { ref, computed, watch } from "vue";
import { useRoute } from "vue-router";
import { apiFetch, extractErrorMessage } from "../api";
import { useInfiniteScroll } from "./useInfiniteScroll";

/**
 * Composable para gerenciar o estado e lógica do catálogo de séries
 * em uma página dedicada (full-page view) com infinite scroll e busca.
 *
 * @param {Object} options
 * @param {string} [options.libraryKey] - Chave da biblioteca (opcional, lê da rota se não fornecido)
 * @param {number} [options.pageSize=24] - Tamanho da página
 */
export function useSeriesCatalog(options = {}) {
  const { libraryKey: propLibraryKey, pageSize: propPageSize = 24 } = options;

  const route = useRoute();

  const PAGE_SIZE = propPageSize;

  // Estado das séries
  const seriesList = ref([]);
  const seriesTotalSize = ref(0);
  const internalPage = ref(1);
  const hasMore = ref(false); // Começa false, só true após carregar primeira página
  const seriesQuery = ref("");
  const seriesLoading = ref(false); // Carregamento inicial / reset
  const isLoadingMore = ref(false); // Carregamento de páginas subsequentes
  const seriesError = ref("");

  let seriesDebounceTimer = null;

  // Resolve libraryKey: prop > route param > null
  const libraryKey = computed(() => {
    return propLibraryKey || route.params.key || null;
  });

  async function fetchSeriesPage(key, pageToFetch) {
    if (!key) return null;

    const params = new URLSearchParams({
      page: String(pageToFetch),
      pageSize: String(PAGE_SIZE),
    });
    if (seriesQuery.value.trim()) {
      params.set("query", seriesQuery.value.trim());
    }

    const res = await apiFetch(`/api/series/${encodeURIComponent(key)}?${params}`);

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

  async function loadInitialSeries(key) {
    const targetKey = key || libraryKey.value;
    if (!targetKey) return;

    seriesLoading.value = true;
    seriesError.value = "";
    internalPage.value = 1;
    hasMore.value = true;

    try {
      const data = await fetchSeriesPage(targetKey, 1);
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
    const key = libraryKey.value;
    if (import.meta.env.DEV) {
      console.log("[useSeriesCatalog] loadNextPage called:", {
        key,
        hasMore: hasMore.value,
        internalPage: internalPage.value,
      });
    }
    if (!key || !hasMore.value || seriesLoading.value) {
      if (import.meta.env.DEV) {
        console.log("[useSeriesCatalog] loadNextPage skipped:", {
          key,
          hasMore: hasMore.value,
          seriesLoading: seriesLoading.value,
        });
      }
      return false;
    }

    isLoadingMore.value = true;
    const nextPage = internalPage.value + 1;

    try {
      const data = await fetchSeriesPage(key, nextPage);
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

      if (seriesList.value.length >= seriesTotalSize.value) {
        hasMore.value = false;
      }

      if (import.meta.env.DEV) {
        console.log("[useSeriesCatalog] loadNextPage success:", {
          page: nextPage,
          newItemsLength: newItems.length,
          totalSeries: seriesList.value.length,
          totalSize: seriesTotalSize.value,
          hasMore: hasMore.value,
        });
      }

      return hasMore.value;
    } catch (err) {
      seriesError.value = err.message || "Erro ao carregar mais séries";
      if (import.meta.env.DEV) {
        console.error("[useSeriesCatalog] loadNextPage error:", err);
      }
      return false;
    } finally {
      isLoadingMore.value = false;
    }
  }

  const { sentinelRef, loadMore } = useInfiniteScroll({
    onLoadMore: loadNextPage,
    hasMore,
    isLoading: isLoadingMore,
    rootMargin: "500px",
  });

  // Busca com debounce - reseta para o início
  watch(seriesQuery, () => {
    clearTimeout(seriesDebounceTimer);
    seriesDebounceTimer = setTimeout(() => {
      const key = libraryKey.value;
      if (key) {
        loadInitialSeries(key);
      }
    }, 300);
  });

  // Recarrega quando libraryKey muda (ex: navegação entre bibliotecas)
  watch(
    libraryKey,
    (newKey, oldKey) => {
      if (newKey && newKey !== oldKey) {
        seriesQuery.value = "";
        seriesError.value = "";
        loadInitialSeries(newKey);
      }
    },
    { immediate: true },
  );

  // Disparo manual se a key já estiver disponível
  if (libraryKey.value) {
    loadInitialSeries(libraryKey.value);
  }

  return {
    // Estado
    libraryKey,
    seriesList,
    seriesTotalSize,
    seriesQuery,
    seriesLoading,
    isLoadingMore,
    seriesError,
    hasMore,
    sentinelRef,
    // Métodos
    loadSeries: loadInitialSeries,
    loadMore,
    refresh: () => libraryKey.value && loadInitialSeries(libraryKey.value),
  };
}
