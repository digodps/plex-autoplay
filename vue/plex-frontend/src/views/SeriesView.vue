<script setup>
import { computed } from "vue";
import { useRoute } from "vue-router";
import AppIcon from "../components/AppIcon.vue";
import Breadcrumb from "../components/Breadcrumb.vue";
import { useSeriesCatalog } from "../composables/useSeriesCatalog";

const route = useRoute();
console.log("SeriesView montado!");

defineEmits(["open-markers"]);

const libraryKey = computed(() => route.params.key);

const {
  seriesList,
  seriesTotalSize,
  seriesQuery,
  seriesLoading,
  isLoadingMore,
  seriesError,
  hasMore,
  sentinelRef,
  refresh,
} = useSeriesCatalog({ libraryKey: libraryKey.value });

// Busca título da biblioteca para breadcrumb (assumindo que vem de uma store ou contexto)
// Por enquanto, usamos o key como fallback; idealmente viria de uma store de bibliotecas
const libraryTitle = computed(() => {
  // TODO: Buscar título real da biblioteca via store ou API
  // Por enquanto, formatamos a key
  return libraryKey.value ? `Biblioteca ${libraryKey.value}` : "Biblioteca";
});

const breadcrumbItems = computed(() => [
  { label: "Bibliotecas", to: { name: "libraries" } },
  { label: libraryTitle.value, current: true },
]);
</script>

<template>
  <div class="series-view">
    <!-- Breadcrumb -->
    <Breadcrumb :items="breadcrumbItems" />

    <!-- Toolbar -->
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

    <!-- Error State -->
    <div v-if="seriesError" class="alert" role="alert">
      <AppIcon name="warning" size="sm" />
      <span>{{ seriesError }}</span>
      <button class="btn btn-ghost btn-sm" @click="refresh">Tentar novamente</button>
    </div>

    <!-- Loading Skeletons -->
    <div v-else-if="seriesLoading && seriesList.length === 0" class="series-grid">
      <div v-for="n in 24" :key="n" class="series-card skeleton">
        <div class="series-card-poster skeleton"></div>
        <div class="series-card-info">
          <div class="skeleton-line"></div>
          <div class="skeleton-line short"></div>
        </div>
      </div>
    </div>

    <!-- Series Grid -->
    <div v-else-if="seriesList.length > 0" class="series-grid-wrapper">
      <div class="series-grid">
        <div
          v-for="series in seriesList"
          :key="series.ratingKey"
          class="series-card"
          @click="$emit('open-markers', series)"
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

        <!-- Loading skeletons for next page -->
        <template v-if="isLoadingMore">
          <div v-for="n in 6" :key="'more-' + n" class="series-card skeleton">
            <div class="series-card-poster skeleton"></div>
            <div class="series-card-info">
              <div class="skeleton-line"></div>
              <div class="skeleton-line short"></div>
            </div>
          </div>
        </template>

        <!-- Sentinel for IntersectionObserver -->
        <div ref="sentinelRef" class="sentinel" aria-hidden="true"></div>
      </div>

      <!-- End of catalog indicator -->
      <div v-if="!hasMore && seriesList.length > 0" class="end-of-catalog">
        <span>Fim do catálogo</span>
      </div>
    </div>

    <!-- Empty State -->
    <div v-else-if="!seriesLoading && !seriesError" class="empty">
      <AppIcon name="tv" size="lg" />
      <p>Nenhuma série encontrada.</p>
    </div>
  </div>
</template>

<style scoped>
.series-view {
  padding: 1.5rem;
  max-width: 1200px;
  margin: 0 auto;
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
  min-height: 400px;
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

/* Buttons */
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
