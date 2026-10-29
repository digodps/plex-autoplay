<script setup>
import { ref, computed, onMounted } from "vue";
import { useRoute } from "vue-router";
import AppIcon from "../components/AppIcon.vue";
import { apiFetch } from "../api";
import { useLibrarySeries } from "../composables/useLibrarySeries";

const route = useRoute();

const libraries = ref([]);
const error = ref("");

const {
  expandedLibraryKey,
  seriesList,
  seriesTotalSize,
  seriesQuery,
  seriesLoading,
  isLoadingMore,
  seriesError,
  hasMore,
  sentinelRef,
  loadSeries,
  toggleLibrary,
  openSeriesMarkers,
} = useLibrarySeries();

const expandedLibrary = computed(() => {
  if (!expandedLibraryKey.value) return null;
  return libraries.value.find((l) => l.key === expandedLibraryKey.value) || null;
});

onMounted(async () => {
  try {
    const response = await apiFetch("/api/plex/libraries");
    if (response.ok) {
      libraries.value = await response.json();
      // Deep-link: se a rota possuir o parâmetro key, expande a biblioteca correspondente
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

    <!-- Expansão full-width de séries da biblioteca selecionada -->
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

      <!-- Skeletons durante carregamento inicial -->
      <div v-if="seriesLoading && seriesList.length === 0" class="series-grid">
        <div v-for="n in 24" :key="n" class="series-card skeleton">
          <div class="series-card-poster skeleton"></div>
          <div class="series-card-info">
            <div class="skeleton-line"></div>
            <div class="skeleton-line short"></div>
          </div>
        </div>
      </div>

      <!-- Grid de séries com wrapper de scrollbar oculta -->
      <div v-else-if="seriesList.length > 0" class="series-grid-wrapper">
        <div class="series-grid">
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

          <!-- Skeletons discretos no fim durante carregamento de próxima página -->
          <template v-if="isLoadingMore">
            <div v-for="n in 6" :key="'more-' + n" class="series-card skeleton">
              <div class="series-card-poster skeleton"></div>
              <div class="series-card-info">
                <div class="skeleton-line"></div>
                <div class="skeleton-line short"></div>
              </div>
            </div>
          </template>

          <!-- Sentinela do IntersectionObserver -->
          <div ref="sentinelRef" class="sentinel" aria-hidden="true"></div>
        </div>

        <!-- Indicador de fim de catálogo -->
        <div v-if="!hasMore && seriesList.length > 0" class="end-of-catalog">
          <span>Fim do catálogo</span>
        </div>
      </div>

      <div v-else-if="!seriesLoading && !seriesError" class="empty">
        <AppIcon name="tv" size="lg" />
        <p>Nenhuma série encontrada.</p>
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

/* Expansão full-width */
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

/* Infinite scroll container */
.series-grid-wrapper {
  overflow-y: auto;
  scrollbar-width: none; /* Firefox */
  -ms-overflow-style: none; /* IE/Edge Legacy */
}
.series-grid-wrapper::-webkit-scrollbar {
  display: none; /* Chrome, Safari, Edge */
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

.sentinel {
  width: 100%;
  height: 1px;
  grid-column: 1 / -1;
  pointer-events: none;
  visibility: hidden;
}

.end-of-catalog {
  text-align: center;
  padding: 1.5rem 0;
  color: var(--text-tertiary);
  font-size: 0.875rem;
  font-style: italic;
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

/* Skeletons */
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

/* Botões */
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
