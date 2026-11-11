import { ref, computed, watch } from "vue";
import { useRoute } from "vue-router";
import { apiFetch, extractErrorMessage } from "../api";
import { useInfiniteScroll } from "./useInfiniteScroll";

<<<<<<< HEAD
export function useSeriesCatalog(options = {}) {
  const { libraryKey: propLibraryKey, pageSize: propPageSize = 24 } = options;
  const route = useRoute();
  const PAGE_SIZE = propPageSize;

  const seriesList = ref([]);
  const seriesTotalSize = ref(0);
  const internalPage = ref(1);
  const hasMore = ref(false);
  const seriesQuery = ref("");
  const seriesLoading = ref(false);
  const isLoadingMore = ref(false);
=======
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
>>>>>>> origin/feature/dashboard-redesign-31-10-2026-13-29-43-856
  const seriesError = ref("");

  let seriesDebounceTimer = null;

<<<<<<< HEAD
  const libraryKey = computed(() => propLibraryKey || route.params.key || null);

  async function fetchSeriesPage(key, pageToFetch) {
    if (!key) return null;
=======
  // Resolve libraryKey: prop > route param > null
  const libraryKey = computed(() => {
    return propLibraryKey || route.params.key || null;
  });

  async function fetchSeriesPage(key, pageToFetch) {
    if (!key) return null;

>>>>>>> origin/feature/dashboard-redesign-31-10-2026-13-29-43-856
    const params = new URLSearchParams({
      page: String(pageToFetch),
      pageSize: String(PAGE_SIZE),
    });
<<<<<<< HEAD
    if (seriesQuery.value.trim()) params.set("query", seriesQuery.value.trim());

    const res = await apiFetch(`/api/series/${encodeURIComponent(key)}?${params}`);
=======
    if (seriesQuery.value.trim()) {
      params.set("query", seriesQuery.value.trim());
    }

    const res = await apiFetch(`/api/series/${encodeURIComponent(key)}?${params}`);

>>>>>>> origin/feature/dashboard-redesign-31-10-2026-13-29-43-856
    if (res.status === 502) {
      const msg =
        (await extractErrorMessage(res)) ||
        "Não foi possível consultar o Plex. Verifique a conexão com o servidor.";
      throw new Error(msg);
    }
<<<<<<< HEAD
    if (!res.ok) throw new Error("Erro ao carregar séries");
=======

    if (!res.ok) {
      throw new Error("Erro ao carregar séries");
    }

>>>>>>> origin/feature/dashboard-redesign-31-10-2026-13-29-43-856
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
<<<<<<< HEAD
=======

>>>>>>> origin/feature/dashboard-redesign-31-10-2026-13-29-43-856
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
<<<<<<< HEAD
    if (!key || !hasMore.value || seriesLoading.value) return false;
=======
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
>>>>>>> origin/feature/dashboard-redesign-31-10-2026-13-29-43-856

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

<<<<<<< HEAD
=======
      // Evita duplicatas por ratingKey
>>>>>>> origin/feature/dashboard-redesign-31-10-2026-13-29-43-856
      const existingKeys = new Set(seriesList.value.map((s) => s.ratingKey));
      const deduplicated = newItems.filter((s) => !existingKeys.has(s.ratingKey));

      seriesList.value = [...seriesList.value, ...deduplicated];
      seriesTotalSize.value = data.totalSize ?? seriesList.value.length;
      internalPage.value = nextPage;

<<<<<<< HEAD
      if (seriesList.value.length >= seriesTotalSize.value) hasMore.value = false;
      return hasMore.value;
    } catch (err) {
      seriesError.value = err.message || "Erro ao carregar mais séries";
=======
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
>>>>>>> origin/feature/dashboard-redesign-31-10-2026-13-29-43-856
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

<<<<<<< HEAD
=======
  // Busca com debounce - reseta para o início
>>>>>>> origin/feature/dashboard-redesign-31-10-2026-13-29-43-856
  watch(seriesQuery, () => {
    clearTimeout(seriesDebounceTimer);
    seriesDebounceTimer = setTimeout(() => {
      const key = libraryKey.value;
<<<<<<< HEAD
      if (key) loadInitialSeries(key);
    }, 300);
  });

=======
      if (key) {
        loadInitialSeries(key);
      }
    }, 300);
  });

  // Recarrega quando libraryKey muda (ex: navegação entre bibliotecas)
>>>>>>> origin/feature/dashboard-redesign-31-10-2026-13-29-43-856
  watch(
    libraryKey,
    (newKey, oldKey) => {
      if (newKey && newKey !== oldKey) {
        seriesQuery.value = "";
        seriesError.value = "";
        loadInitialSeries(newKey);
      }
    },
<<<<<<< HEAD
    { immediate: true }
  );

  return {
=======
    { immediate: true },
  );

  // Disparo manual se a key já estiver disponível
  if (libraryKey.value) {
    loadInitialSeries(libraryKey.value);
  }

  return {
    // Estado
>>>>>>> origin/feature/dashboard-redesign-31-10-2026-13-29-43-856
    libraryKey,
    seriesList,
    seriesTotalSize,
    seriesQuery,
    seriesLoading,
    isLoadingMore,
    seriesError,
    hasMore,
    sentinelRef,
<<<<<<< HEAD
=======
    // Métodos
>>>>>>> origin/feature/dashboard-redesign-31-10-2026-13-29-43-856
    loadSeries: loadInitialSeries,
    loadMore,
    refresh: () => libraryKey.value && loadInitialSeries(libraryKey.value),
  };
<<<<<<< HEAD
}
=======
}
>>>>>>> origin/feature/dashboard-redesign-31-10-2026-13-29-43-856
