import { ref, computed, watch } from "vue";
import { useRoute } from "vue-router";
import { apiFetch, extractErrorMessage } from "../api";

export function useSeriesByGenre(options = {}) {
  const { libraryKey: propLibraryKey, pageSize: propPageSize = 1000 } = options;
  const route = useRoute();
  const PAGE_SIZE = propPageSize;

  const genreGroups = ref([]);
  const isLoading = ref(false);
  const error = ref("");
  const allSeriesLoaded = ref(false);

  const libraryKey = computed(() => propLibraryKey || route.params.key || null);

  async function fetchAllSeries(key) {
    if (!key) return null;
    const params = new URLSearchParams({
      page: "1",
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

  function groupByGenre(series) {
    const groups = {};
    const noGenreKey = "Outros";

    for (const s of series) {
      const genre = s.genre?.trim() || noGenreKey;
      if (!groups[genre]) {
        groups[genre] = [];
      }
      groups[genre].push(s);
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

  async function loadSeriesByGenre(key) {
    const targetKey = key || libraryKey.value;
    if (!targetKey) return;

    isLoading.value = true;
    error.value = "";
    allSeriesLoaded.value = false;

    try {
      const data = await fetchAllSeries(targetKey);
      if (!data) return;

      const items = data.items ?? [];
      genreGroups.value = groupByGenre(items);
      allSeriesLoaded.value = true;
    } catch (err) {
      error.value = err.message || "Erro de conexão";
      genreGroups.value = [];
    } finally {
      isLoading.value = false;
    }
  }

  watch(
    libraryKey,
    (newKey, oldKey) => {
      if (newKey && newKey !== oldKey) {
        loadSeriesByGenre(newKey);
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
    refresh: () => libraryKey.value && loadSeriesByGenre(libraryKey.value),
  };
}