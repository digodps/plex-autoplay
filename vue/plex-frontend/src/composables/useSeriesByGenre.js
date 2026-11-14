import { ref, computed, watch } from "vue";
import { useRoute } from "vue-router";
import { apiFetch, extractErrorMessage } from "../api";
import { useInfiniteScroll } from "./useInfiniteScroll";

export function useSeriesByGenre(options = {}) {
  const { libraryKey: propLibraryKey, pageSize: propPageSize = 100 } = options;
  const route = useRoute();
  const PAGE_SIZE = Math.min(propPageSize, 100); // API max is 100

  const genreGroups = ref([]);
  const isLoading = ref(false);
  const error = ref("");
  const internalPage = ref(1);
  const hasMore = ref(true);
  const allSeriesLoaded = ref(false);

  const libraryKey = computed(() => propLibraryKey || route.params.key || null);

  // Track loaded ratingKeys per genre to avoid duplicates
  const loadedKeysByGenre = ref({});

  async function fetchSeriesPage(key, pageToFetch) {
    if (!key) return null;
    const params = new URLSearchParams({
      page: String(pageToFetch),
      pageSize: String(PAGE_SIZE),
    });

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

  function groupByGenre(series, existingGroups = {}) {
    const groups = { ...existingGroups };
    const noGenreKey = "Outros";

    for (const s of series) {
      const genre = s.genre?.trim() || noGenreKey;
      if (!groups[genre]) {
        groups[genre] = [];
      }
      // Avoid duplicates by ratingKey
      if (!loadedKeysByGenre.value[genre]) {
        loadedKeysByGenre.value[genre] = new Set();
      }
      if (!loadedKeysByGenre.value[genre].has(s.ratingKey)) {
        groups[genre].push(s);
        loadedKeysByGenre.value[genre].add(s.ratingKey);
      }
    }

    // Sort genres alphabetically, but put "Outros" at the end
    const sortedGenres = Object.keys(groups).sort((a, b) => {
      if (a === noGenreKey) return 1;
      if (b === noGenreKey) return -1;
      return a.localeCompare(b);
    });

    return sortedGenres.map((genre) => ({
      genre,
      series: groups[genre],
    }));
  }

  async function loadNextPage() {
    const key = libraryKey.value;
    if (!key || !hasMore.value || isLoading.value) return false;

    isLoading.value = true;
    const nextPage = internalPage.value + 1;

    try {
      const data = await fetchSeriesPage(key, nextPage);
      if (!data) return false;

      const items = data.items ?? [];
      if (items.length === 0) {
        hasMore.value = false;
        allSeriesLoaded.value = true;
        return false;
      }

      // Merge new items into existing genre groups
      genreGroups.value = groupByGenre(items, Object.fromEntries(
        genreGroups.value.map(g => [g.genre, g.series])
      ));

      internalPage.value = nextPage;

      // Check if we've loaded all items
      if (items.length < PAGE_SIZE) {
        hasMore.value = false;
        allSeriesLoaded.value = true;
      }

      return hasMore.value;
    } catch (err) {
      error.value = err.message || "Erro de conexão";
      return false;
    } finally {
      isLoading.value = false;
    }
  }

  async function loadInitialPage(key) {
    const targetKey = key || libraryKey.value;
    if (!targetKey) return;

    isLoading.value = true;
    error.value = "";
    allSeriesLoaded.value = false;
    internalPage.value = 1;
    hasMore.value = true;
    loadedKeysByGenre.value = {};

    try {
      const data = await fetchSeriesPage(targetKey, 1);
      if (!data) return;

      const items = data.items ?? [];
      genreGroups.value = groupByGenre(items);
      internalPage.value = 1;

      if (items.length < PAGE_SIZE) {
        hasMore.value = false;
        allSeriesLoaded.value = true;
      } else {
        hasMore.value = true;
      }
    } catch (err) {
      error.value = err.message || "Erro de conexão";
      genreGroups.value = [];
    } finally {
      isLoading.value = false;
    }
  }

  const { sentinelRef, loadMore } = useInfiniteScroll({
    onLoadMore: loadNextPage,
    hasMore,
    isLoading,
    rootMargin: "300px",
  });

  watch(
    libraryKey,
    (newKey, oldKey) => {
      if (newKey && newKey !== oldKey) {
        loadInitialPage(newKey);
      }
    },
    { immediate: true }
  );

  return {
    libraryKey,
    genreGroups,
    isLoading,
    error,
    allSeriesLoaded,
    hasMore,
    sentinelRef,
    loadMore,
    refresh: () => libraryKey.value && loadInitialPage(libraryKey.value),
  };
}