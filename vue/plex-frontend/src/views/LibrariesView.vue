<script setup>
import { ref, computed, watch, onMounted } from "vue";
import { useRoute, useRouter } from "vue-router";
import AppIcon from "../components/AppIcon.vue";
import { apiFetch, extractErrorMessage } from "../api";

const route = useRoute();
const router = useRouter();

const libraries = ref([]);
const error = ref("");

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

// Computed to get the expanded library object
const expandedLibrary = computed(() => {
  if (!expandedLibraryKey.value) return null;
  return libraries.value.find((l) => l.key === expandedLibraryKey.value) || null;
});

// Debounce timer for search
let seriesDebounceTimer;

onMounted(async () => {
  try {
    const response = await apiFetch("/api/plex/libraries");
    if (response.ok) {
      libraries.value = await response.json();
      // Deep-link: if route has key param, expand that library
      if (route.params.key) {
        expandedLibraryKey.value = route.params.key;
        await loadSeries(route.params.key);
      }
    } else {
      error.value = "Erro ao carregar bibliotecas";
    }
  } catch {
    error.value = "Erro de conexão";
  }
});

// Watch route param for deep-linking
watch(
  () => route.params.key,
  (newKey) => {
    if (newKey && newKey !== expandedLibraryKey.value) {
      expandedLibraryKey.value = newKey;
      seriesPage.value = 1;
      seriesQuery.value = "";
      loadSeries(newKey);
    } else if (!newKey && expandedLibraryKey.value) {
      // Navigated away from a library series view
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
      loadSeries(expandedLibraryKey.value);
    }
  }, 300);
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
    // Update URL to remove key param
    router.replace({ name: "libraries" });
  } else {
    // Expand new library
    expandedLibraryKey.value = libraryKey;
    seriesPage.value = 1;
    seriesQuery.value = "";
    seriesError.value = "";
    loadSeries(libraryKey);
    // Update URL with key param
    router.replace({ name: "libraries", params: { key: libraryKey } });
  }
}

function goToSeriesPage(page) {
  if (page < 1 || page > totalSeriesPages.value || page === seriesPage.value) return;
  seriesPage.value = page;
  loadSeries(expandedLibraryKey.value);
}

function openSeriesMarkers(series) {
  router.push({ name: "series-markers", params: { seriesKey: series.ratingKey } });
}
</script>

<template>
  <div class="libraries">
    <h1>Bibliotecas</h1>
    <p class="subtitle">Bibliotecas de séries detectadas no Plex.</p>
    <div v-if="error" class="alert">
      {{ error }}
    </div>
    <div v-else class="library-grid">
      <div v-for="lib in libraries" :key="lib.key" class="library-card-wrapper">
        <div
          class="library-card"
          :class="{ 'is-expanded': expandedLibraryKey === lib.key }"
          @click="toggleLibrary(lib.key)"
        >
          <div class="library-card-icon"><AppIcon name="tv" /></div>
          <h3>{{ lib.title }}</h3>
          <span class="library-open">
            <AppIcon
              :name="expandedLibraryKey === lib.key ? 'chevron-down' : 'chevron-right'"
              size="sm"
            />
            {{ expandedLibraryKey === lib.key ? "Ocultar séries" : "Ver séries" }}
          </span>
        </div>
      </div>
    </div>

    <!-- Full-width series expansion (outside grid) -->
    <div v-if="expandedLibraryKey" class="library-series-expansion-full">
      <div class="expansion-header">
        <h2>
          <AppIcon name="tv" />
          {{ expandedLibrary?.title }}
        </h2>
        <button class="btn btn-ghost" @click="toggleLibrary(expandedLibraryKey)">
          <AppIcon name="chevron-left" size="sm" />
          Voltar às bibliotecas
        </button>
      </div>

      <div class="series-toolbar">
        <div class="search-box">
          <AppIcon name="search" size="sm" />
          <input
            v-model="seriesQuery"
            class="input"
            type="search"
            placeholder="Buscar série..."
            aria-label="Buscar série"
          />
        </div>
        <span v-if="!seriesLoading" class="count">{{ seriesTotalSize }} série(s)</span>
      </div>

      <div v-if="seriesError" class="alert" role="alert">
        <AppIcon name="warning" size="sm" />
        <span>{{ seriesError }}</span>
        <button class="btn btn-ghost btn-sm" @click="loadSeries(expandedLibraryKey)">
          Tentar novamente
        </button>
      </div>

      <!-- Skeleton loading -->
      <div v-if="seriesLoading && seriesList.length === 0" class="series-grid">
        <div v-for="n in seriesPageSize" :key="n" class="series-card skeleton">
          <div class="series-card-poster skeleton"></div>
          <div class="series-card-info">
            <div class="skeleton-line"></div>
            <div class="skeleton-line short"></div>
          </div>
        </div>
      </div>

      <!-- Series grid -->
      <div v-else-if="seriesList.length > 0" class="series-grid">
        <div
          v-for="series in seriesList"
          :key="series.ratingKey"
          class="series-card"
          @click="openSeriesMarkers(series)"
        >
          <div class="series-card-poster">
            <img v-if="series.thumb" :src="series.thumb" :alt="series.title" loading="lazy" />
            <AppIcon v-else name="tv" size="lg" />
          </div>
          <div class="series-card-info">
            <h4>{{ series.title }}</h4>
            <p v-if="series.summary" class="series-summary">{{ series.summary }}</p>
          </div>
        </div>
      </div>

      <div v-else-if="!seriesLoading && !seriesError" class="empty">
        <AppIcon name="tv" size="lg" />
        <p>Nenhuma série encontrada.</p>
      </div>

      <!-- Pagination -->
      <div v-if="totalSeriesPages > 1" class="series-pagination">
        <button
          class="btn btn-ghost btn-sm"
          :disabled="seriesPage === 1"
          @click="goToSeriesPage(seriesPage - 1)"
        >
          <AppIcon name="chevron-left" size="sm" />
          Anterior
        </button>
        <span class="page-info"> Página {{ seriesPage }} de {{ totalSeriesPages }} </span>
        <button
          class="btn btn-ghost btn-sm"
          :disabled="seriesPage === totalSeriesPages"
          @click="goToSeriesPage(seriesPage + 1)"
        >
          Próxima
          <AppIcon name="chevron-right" size="sm" />
        </button>
      </div>
    </div>

    <div v-if="!error && !expandedLibraryKey && libraries.length === 0" class="empty">
      Nenhuma biblioteca de séries encontrada.
    </div>
  </div>
</template>

<style scoped>
.libraries {
  max-width: 1200px;
  margin: 0 auto;
  padding: 2rem;
}
.libraries h1 {
  font-size: 1.6rem;
}
.subtitle {
  color: var(--text-secondary);
  margin: 0.25rem 0 1.5rem;
}
.library-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(250px, 1fr));
  gap: 1rem;
}
.library-card-wrapper {
  display: flex;
  flex-direction: column;
}
.library-card {
  padding: 1.25rem;
  border: 1px solid var(--border);
  border-radius: var(--radius);
  background: var(--surface);
  transition:
    transform var(--speed) var(--ease),
    box-shadow var(--speed) var(--ease);
  cursor: pointer;
}
.library-card:hover {
  transform: translateY(-2px);
  box-shadow: var(--shadow-2);
}
.library-card.is-expanded {
  border-color: var(--accent);
  box-shadow: var(--shadow-2);
}
.library-card-icon {
  color: var(--accent);
  margin-bottom: 0.5rem;
}
.library-card h3 {
  font-size: 1rem;
}
.library-open {
  display: inline-flex;
  align-items: center;
  gap: 0.25rem;
  margin-top: 0.4rem;
  font-size: 0.8rem;
  color: var(--text-secondary);
}
/* Full-width series expansion */
.library-series-expansion-full {
  margin-top: 1.5rem;
  padding: 1.5rem;
  border: 1px solid var(--border);
  border-radius: var(--radius);
  background: var(--bg-sunken);
  animation: slideDown 0.3s ease-out;
}

.expansion-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 1rem;
  margin-bottom: 1.5rem;
  padding-bottom: 1rem;
  border-bottom: 1px solid var(--border);
  flex-wrap: wrap;
}

.expansion-header h2 {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  font-size: 1.25rem;
  margin: 0;
  color: var(--text);
}

.expansion-header h2 .icon {
  color: var(--accent);
}

/* Keep existing library-series-expansion styles for reference but they're no longer used */
.library-series-expansion {
  display: none;
}

/* Adjust library-card is-expanded since expansion is now full-width */
.library-card.is-expanded {
  border-color: var(--accent);
  box-shadow: var(--shadow-2);
}
.series-toolbar {
  display: flex;
  align-items: center;
  gap: 1rem;
  margin-bottom: 1rem;
  flex-wrap: wrap;
}
.search-box {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  flex: 1;
  min-width: 200px;
}
.search-box .input {
  flex: 1;
  padding: 0.5rem 0.75rem;
  border: 1px solid var(--border);
  border-radius: var(--radius-sm);
  background: var(--surface);
  color: var(--text);
}
.count {
  font-size: 0.875rem;
  color: var(--text-secondary);
}
.series-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(180px, 1fr));
  gap: 1rem;
}
.series-card {
  display: flex;
  flex-direction: column;
  padding: 0.75rem;
  border: 1px solid var(--border);
  border-radius: var(--radius);
  background: var(--surface);
  cursor: pointer;
  transition:
    transform var(--speed) var(--ease),
    box-shadow var(--speed) var(--ease);
}
.series-card:hover {
  transform: translateY(-2px);
  box-shadow: var(--shadow-2);
}
.series-card.skeleton {
  pointer-events: none;
}
.series-card-poster {
  aspect-ratio: 2/3;
  border-radius: var(--radius-sm);
  overflow: hidden;
  background: var(--bg-sunken);
  margin-bottom: 0.5rem;
}
.series-card-poster img {
  width: 100%;
  height: 100%;
  object-fit: cover;
}
.series-card-poster .icon {
  width: 100%;
  height: 100%;
  display: flex;
  align-items: center;
  justify-content: center;
  color: var(--text-tertiary);
}
.series-card-info h4 {
  font-size: 0.875rem;
  font-weight: 600;
  margin: 0 0 0.25rem;
  line-height: 1.3;
}
.series-summary {
  font-size: 0.75rem;
  color: var(--text-secondary);
  margin: 0;
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
}
.series-pagination {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 1rem;
  margin-top: 1rem;
  padding-top: 1rem;
  border-top: 1px solid var(--border);
}
.page-info {
  font-size: 0.875rem;
  color: var(--text-secondary);
}
.empty {
  color: var(--text-tertiary);
  font-style: italic;
  text-align: center;
  padding: 2rem;
}
.empty .icon {
  margin-bottom: 0.5rem;
  opacity: 0.5;
}

/* Skeleton styles */
.skeleton {
  animation: pulse 1.5s ease-in-out infinite;
}
@keyframes pulse {
  0%,
  100% {
    opacity: 1;
  }
  50% {
    opacity: 0.5;
  }
}
.skeleton-line {
  height: 1rem;
  background: linear-gradient(90deg, var(--bg-sunken) 25%, var(--border) 50%, var(--bg-sunken) 75%);
  background-size: 200% 100%;
  animation: shimmer 1.5s infinite;
  border-radius: var(--radius-sm);
  margin-bottom: 0.5rem;
}
.skeleton-line.short {
  width: 60%;
}
.series-card-poster.skeleton {
  background: var(--bg-sunken);
}
@keyframes shimmer {
  0% {
    background-position: -200% 0;
  }
  100% {
    background-position: 200% 0;
  }
}

/* Button styles (reused from SeriesView) */
.btn {
  display: inline-flex;
  align-items: center;
  gap: 0.375rem;
  padding: 0.5rem 1rem;
  border: 1px solid var(--border);
  border-radius: var(--radius-sm);
  background: var(--surface);
  color: var(--text);
  font-size: 0.875rem;
  font-weight: 500;
  cursor: pointer;
  transition:
    background var(--speed) var(--ease),
    border-color var(--speed) var(--ease),
    color var(--speed) var(--ease);
}
.btn:hover:not(:disabled) {
  background: var(--bg-sunken);
  border-color: var(--accent);
}
.btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}
.btn-ghost {
  background: transparent;
}
.btn-ghost:hover:not(:disabled) {
  background: var(--bg-sunken);
}
.btn-sm {
  padding: 0.375rem 0.75rem;
  font-size: 0.8125rem;
}
.alert {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  padding: 0.75rem 1rem;
  border-radius: var(--radius);
  background: var(--danger-bg, #fef2f2);
  color: var(--danger, #dc2626);
  margin-bottom: 1rem;
}
.alert .icon {
  flex-shrink: 0;
}
</style>
