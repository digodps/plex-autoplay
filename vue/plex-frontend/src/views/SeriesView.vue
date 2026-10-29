<script setup>
import { ref, computed, watch, onMounted } from "vue";
import { useRoute, useRouter } from "vue-router";
import AppIcon from "../components/AppIcon.vue";
import SeriesCard from "../components/SeriesCard.vue";
import SeriesPagination from "../components/SeriesPagination.vue";
import { apiFetch, extractErrorMessage } from "../api";

const route = useRoute();
const router = useRouter();

const libraries = ref([]);
const libraryKey = ref(route.params.key ?? "");
const seriesList = ref([]);
const totalSize = ref(0);
const page = ref(1);
const pageSize = ref(24);
const query = ref("");
const loading = ref(false);
const error = ref("");
const searching = computed(() => loading.value);
const PAGES_SHOWN = 5;

const totalPages = computed(() => Math.max(1, Math.ceil(totalSize.value / pageSize.value)));
const pageWindow = computed(() => {
  const total = totalPages.value;
  const start = Math.max(
    1,
    Math.min(page.value - Math.floor(PAGES_SHOWN / 2), total - PAGES_SHOWN + 1),
  );
  const end = Math.min(total, start + PAGES_SHOWN - 1);
  return Array.from({ length: end - start + 1 }, (_, i) => start + i);
});

onMounted(async () => {
  if (!libraryKey.value) {
    try {
      const res = await apiFetch("/api/plex/libraries");
      if (res.ok) {
        libraries.value = await res.json();
        if (libraries.value.length === 1) {
          router.replace({ name: "series", params: { key: libraries.value[0].key } });
          return;
        }
      }
    } catch {
      error.value = "Erro de conexão";
    }
  }
  await loadSeries();
});

// Recarrega quando o parâmetro de biblioteca mudar
watch(
  () => route.params.key,
  (newKey) => {
    if (newKey && newKey !== libraryKey.value) {
      libraryKey.value = newKey;
      page.value = 1;
      query.value = "";
      loadSeries();
    }
  },
);

// Busca com debounce de 300ms
let debounceTimer;
watch(query, () => {
  clearTimeout(debounceTimer);
  debounceTimer = setTimeout(() => {
    page.value = 1;
    loadSeries();
  }, 300);
});

function goToPage(p) {
  if (p < 1 || p > totalPages.value || p === page.value) return;
  page.value = p;
  loadSeries(scrollToTop);
}

function scrollToTop() {
  window.scrollTo({ top: 0, behavior: "smooth" });
}

async function loadSeries() {
  if (!libraryKey.value) return;
  loading.value = true;
  error.value = "";
  try {
    const params = new URLSearchParams({
      page: String(page.value),
      pageSize: String(pageSize.value),
    });
    if (query.value.trim()) params.set("query", query.value.trim());

    const res = await apiFetch(`/api/series/${encodeURIComponent(libraryKey.value)}?${params}`);

    if (res.status === 502) {
      error.value = await extractErrorMessage(res) || "Não foi possível consultar o Plex. Verifique a conexão com o servidor.";
      seriesList.value = [];
      return;
    }

    if (res.ok) {
      const data = await res.json();
      seriesList.value = data.items ?? [];
      totalSize.value = data.totalSize ?? 0;
    } else {
      error.value = "Erro ao carregar séries";
    }
  } catch {
    error.value = "Erro de conexão";
  } finally {
    loading.value = false;
  }
}

function openSeries(series) {
  router.push({ name: "series-markers", params: { seriesKey: series.ratingKey } });
}
</script>

<template>
  <div class="series-page">
    <header class="series-header">
      <h1>Séries</h1>
      <p class="subtitle">
        Todas as séries das suas bibliotecas — clique para editar os marcadores.
      </p>
    </header>

    <div v-if="libraries.length > 1 && !libraryKey" class="library-picker">
      <button
        v-for="lib in libraries"
        :key="lib.key"
        class="btn btn-ghost"
        @click="router.replace({ name: 'series', params: { key: lib.key } })"
      >
        <AppIcon name="library" size="sm" />
        {{ lib.title }}
      </button>
    </div>

    <div class="toolbar">
      <div class="search-box">
        <AppIcon name="search" size="sm" />
        <input
          v-model="query"
          class="input"
          type="search"
          placeholder="Buscar série..."
          aria-label="Buscar série"
        />
      </div>
      <span v-if="!loading" class="count">{{ totalSize }} série(s)</span>
    </div>

    <div v-if="error" class="alert" role="alert">
      <AppIcon name="warning" size="sm" />
      <span>{{ error }}</span>
      <button class="btn btn-ghost btn-sm" @click="loadSeries">Tentar novamente</button>
    </div>

    <!-- grade de séries -->
    <div v-if="loading && seriesList.length === 0" class="grid">
      <SeriesCard v-for="n in pageSize" :key="n" loading />
    </div>

    <div v-else-if="seriesList.length > 0" class="grid" :class="{ 'is-refreshing': searching }">
      <SeriesCard
        v-for="series in seriesList"
        :key="series.ratingKey"
        :series="series"
        @open="openSeries(series)"
      />
    </div>

    <div v-else-if="!loading && !error" class="empty">
      <AppIcon name="tv" size="lg" />
      <p>Nenhuma série encontrada.</p>
    </div>

    <!-- paginação -->
    <SeriesPagination
      v-if="totalPages > 1"
      :page="page"
      :total-pages="totalPages"
      :page-window="pageWindow"
      @go="goToPage"
    />
  </div>
</template>

<style scoped src="./series.css"></style>
