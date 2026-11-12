import { ref, computed, watch } from "vue";
import { useRoute } from "vue-router";
import { apiFetch, extractErrorMessage } from "../api";
import { useInfiniteScroll } from "./useInfiniteScroll";

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
  const seriesError = ref("");

  // Estados para sincronização forçada do catálogo
  const syncLoading = ref(false);
  const syncMessage = ref("");
  const syncType = ref(""); // "success" | "error" | ""

  let seriesDebounceTimer = null;

  const libraryKey = computed(() => propLibraryKey || route.params.key || null);

  async function fetchSeriesPage(key, pageToFetch) {
    if (!key) return null;
    const params = new URLSearchParams({
      page: String(pageToFetch),
      pageSize: String(PAGE_SIZE),
    });
    if (seriesQuery.value.trim()) params.set("query", seriesQuery.value.trim());

    const res = await apiFetch(`/api/series/${encodeURIComponent(key)}?${params}`);
    if (res.status === 502) {
      const msg =
        (await extractErrorMessage(res)) ||
        "Não foi possível consultar o Plex. Verifique a conexão com o servidor.";
      throw new Error(msg);
    }
    if (!res.ok) throw new Error("Erro ao carregar séries");
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
    if (!key || !hasMore.value || seriesLoading.value) return false;

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

      const existingKeys = new Set(seriesList.value.map((s) => s.ratingKey));
      const deduplicated = newItems.filter((s) => !existingKeys.has(s.ratingKey));

      seriesList.value = [...seriesList.value, ...deduplicated];
      seriesTotalSize.value = data.totalSize ?? seriesList.value.length;
      internalPage.value = nextPage;

      if (seriesList.value.length >= seriesTotalSize.value) hasMore.value = false;
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
    rootMargin: "500px",
  });

  watch(seriesQuery, () => {
    clearTimeout(seriesDebounceTimer);
    seriesDebounceTimer = setTimeout(() => {
      const key = libraryKey.value;
      if (key) loadInitialSeries(key);
    }, 300);
  });

  watch(
    libraryKey,
    (newKey, oldKey) => {
      if (newKey && newKey !== oldKey) {
        seriesQuery.value = "";
        seriesError.value = "";
        loadInitialSeries(newKey);
      }
    },
    { immediate: true }
  );

  async function forceSyncCatalog() {
    syncLoading.value = true;
    syncMessage.value = "";
    syncType.value = "";
    try {
      const res = await apiFetch("/api/sync/catalog", { method: "POST" });
      if (!res.ok) {
        const body = await res.json().catch(() => ({}));
        syncType.value = "error";
        syncMessage.value = body.detail || body.message || `Erro ${res.status} ao sincronizar`;
        return false;
      }
      const result = await res.json();
      syncType.value = "success";
      syncMessage.value = `Catálogo sincronizado: ${result.librariesInserted + result.librariesUpdated} biblioteca(s), ${result.seriesInserted + result.seriesUpdated} série(s)`;
      // Recarrega a lista de séries após sincronizar
      const key = libraryKey.value;
      if (key) loadInitialSeries(key);
      return true;
    } catch (err) {
      syncType.value = "error";
      syncMessage.value = err.message || "Erro de conexão ao sincronizar";
      return false;
    } finally {
      syncLoading.value = false;
    }
  }

  return {
    libraryKey,
    seriesList,
    seriesTotalSize,
    seriesQuery,
    seriesLoading,
    isLoadingMore,
    seriesError,
    hasMore,
    sentinelRef,
    syncLoading,
    syncMessage,
    syncType,
    loadSeries: loadInitialSeries,
    loadMore,
    refresh: () => libraryKey.value && loadInitialSeries(libraryKey.value),
    forceSyncCatalog,
  };
}