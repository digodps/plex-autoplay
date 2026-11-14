<script setup>
import { computed, ref } from "vue";
import { useRoute, useRouter } from "vue-router";
import AppIcon from "@/components/AppIcon.vue";
import GenreCarousel from "@/components/GenreCarousel.vue";
import { useSeriesCatalog } from "@/composables/useSeriesCatalog";
import { useSeriesByGenre } from "@/composables/useSeriesByGenre";
import { useLibraryTitle } from "@/composables/useLibraryTitle";
import { reprocessMarkers } from "@/api";

const route = useRoute();
const router = useRouter();

const libraryKey = computed(() => route.params.key);
const queryTitle = computed(() => route.query.libraryTitle);

const {
  title: libraryTitle,
  loading: libraryTitleLoading,
  error: libraryError,
} = useLibraryTitle(libraryKey, queryTitle);

const {
  seriesList,
  seriesTotalSize,
  seriesLoading,
  seriesError,
  sentinelRef,
  seriesQuery,
  syncLoading,
  syncMessage,
  syncType,
  forceSyncCatalog,
  loadMore,
  hasMore,
  isLoadingMore,
} = useSeriesCatalog();

const {
  genreGroups,
  isLoading: carouselLoading,
  error: carouselError,
  sentinelRef: carouselSentinelRef,
  hasMore: carouselHasMore,
  allSeriesLoaded,
} = useSeriesByGenre({
  libraryKey: libraryKey.value,
});

const totalSeriesInCarousel = computed(() =>
  genreGroups.value.reduce((sum, g) => sum + g.series.length, 0),
);

// Tab state: 'carousel' | 'list'
const activeTab = ref("carousel");

const tabs = [
  { id: "carousel", label: "Por Gênero", icon: "grid", count: computed(() => genreGroups.value.length) },
  { id: "list", label: "Todas as Séries", icon: "list", count: computed(() => seriesTotalSize.value) },
];

// Reprocessing state per series
const reprocessingKey = ref(null);

async function handleReprocess(seriesKey, e) {
  e.stopPropagation();
  reprocessingKey.value = seriesKey;
  try {
    const res = await reprocessMarkers(seriesKey);
    if (!res.ok) throw new Error("Falha ao reprocessar marcadores");
    // Optionally refresh series list or show success toast
    syncMessage.value = "Marcadores reprocessados com sucesso";
    syncType.value = "success";
  } catch (e) {
    syncMessage.value = e.message || "Falha ao reprocessar";
    syncType.value = "error";
  } finally {
    reprocessingKey.value = null;
  }
}

function openMarkers(series) {
  if (libraryTitleLoading.value) return;
  const query = {
    libraryKey: libraryKey.value,
    seriesTitle: series.title,
  };
  if (libraryTitle.value) query.libraryTitle = libraryTitle.value;
  router.push({ name: "series-markers", params: { seriesKey: series.ratingKey }, query });
}
</script>

<template>
  <div class="series-view">
    <!-- Tab Navigation -->
    <nav class="tabs-nav" role="tablist" aria-label="Visualização do catálogo">
      <button
        v-for="tab in tabs"
        :key="tab.id"
        role="tab"
        :aria-selected="activeTab === tab.id"
        :aria-controls="`panel-${tab.id}`"
        :id="`tab-${tab.id}`"
        class="tab-btn"
        :class="{ 'tab-btn--active': activeTab === tab.id }"
        @click="activeTab = tab.id"
        type="button"
      >
        <AppIcon :name="tab.icon" size="sm" aria-hidden="true" />
        <span>{{ tab.label }}</span>
        <span v-if="tab.count !== undefined && tab.count !== null" class="tab-count">
          {{ tab.count }}
        </span>
      </button>
    </nav>

    <div class="series-toolbar">
      <div class="search-box" v-show="activeTab === 'list'">
        <AppIcon name="search" size="sm" />
        <input
          class="input"
          type="search"
          v-model="seriesQuery"
          placeholder="Buscar série..."
          aria-label="Buscar série"
        />
      </div>
      <span v-if="!seriesLoading && !carouselLoading" class="count">
        {{ seriesTotalSize }} série(s) • {{ totalSeriesInCarousel }} em {{ genreGroups.length }} gênero(s)
      </span>
    </div>

    <div v-if="libraryError" class="alert" role="alert">
      <span>{{ libraryError }}</span>
    </div>

    <div v-else-if="libraryTitleLoading" class="loading-skeleton">
      <div class="h-24 rounded-lg border border-stroke bg-skeleton"></div>
      <div class="h-20 rounded-lg border border-stroke bg-skeleton"></div>
      <div class="h-16 rounded-lg border border-stroke bg-skeleton"></div>
    </div>

    <!-- Tab Panel: Por Gênero (Carousel) -->
    <section
      v-show="activeTab === 'carousel'"
      id="panel-carousel"
      role="tabpanel"
      aria-labelledby="tab-carousel"
      class="tab-panel"
    >
      <div v-if="carouselError" class="alert" role="alert">
        <span>{{ carouselError }}</span>
      </div>

      <div v-else-if="carouselLoading" class="loading-skeleton">
        <div class="h-24 rounded-lg border border-stroke bg-skeleton"></div>
        <div class="h-20 rounded-lg border border-stroke bg-skeleton"></div>
        <div class="h-16 rounded-lg border border-stroke bg-skeleton"></div>
      </div>

      <div v-else-if="genreGroups.length > 0">
        <GenreCarousel
          :genre-groups="genreGroups"
          :is-loading="false"
          :error="carouselError"
        />

        <!-- Load More for Carousel -->
        <div v-if="carouselHasMore && !allSeriesLoaded" class="load-more-area">
          <div v-if="carouselLoading" class="loading-indicator">
            <AppIcon name="loader" size="md" class="animate-spin" />
            <span>Carregando mais gêneros...</span>
          </div>
          <div ref="carouselSentinelRef" class="h-1" aria-hidden="true"></div>
        </div>
      </div>

      <div v-else class="empty-state">
        <AppIcon name="grid" size="lg" class="mx-auto text-muted" />
        <p class="mt-3 text-sm font-medium text-secondary">Nenhum gênero encontrado.</p>
        <p class="mt-2 text-xs text-tertiary">
          O catálogo desta biblioteca ainda não foi sincronizado.
        </p>
      </div>
    </section>

    <!-- Tab Panel: Todas as Séries (Infinite Scroll List) -->
    <section
      v-show="activeTab === 'list'"
      id="panel-list"
      role="tabpanel"
      aria-labelledby="tab-list"
      class="tab-panel"
    >
      <div v-if="seriesError" class="alert" role="alert">
        <span>{{ seriesError }}</span>
      </div>

      <div v-else-if="seriesLoading && seriesList.length === 0" class="loading-skeleton">
        <div class="h-24 rounded-lg border border-stroke bg-skeleton"></div>
        <div class="h-20 rounded-lg border border-stroke bg-skeleton"></div>
        <div class="h-16 rounded-lg border border-stroke bg-skeleton"></div>
      </div>

      <div v-else-if="seriesList.length > 0" class="series-grid-section">
        <p class="series-grid-count" role="status">
          {{ seriesTotalSize }} série(s)
        </p>

        <div class="series-grid" role="list">
          <div
            v-for="series in seriesList"
            :key="series.ratingKey"
            data-testid="series-card"
            class="series-card group relative flex flex-col rounded-xl overflow-hidden border border-stroke bg-card shadow-sm hover:shadow-lg transition-all duration-300"
            role="listitem"
          >
            <button
              type="button"
              class="series-card-main flex flex-col focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-primary focus-visible:ring-inset"
              @click="openMarkers(series)"
            >
              <div class="series-card-poster relative aspect-[2/3] w-full overflow-hidden bg-sunken">
                <div
                  data-testid="series-thumb-inner"
                  class="h-full w-full transition-transform duration-500 group-hover:scale-105 group-focus-visible:scale-105"
                >
                  <img
                    v-if="series.thumb"
                    :src="series.thumb"
                    :alt="series.title"
                    loading="lazy"
                    class="h-full w-full object-cover"
                  />
                  <div v-else class="flex h-full w-full items-center justify-center text-muted">
                    <AppIcon name="tv" size="lg" />
                  </div>
                </div>
                <div
                  aria-hidden="true"
                  class="pointer-events-none absolute inset-0 bg-gradient-to-t from-black/60 via-black/10 to-transparent"
                ></div>
                <div class="absolute bottom-0 left-0 right-0 p-3 text-white">
                  <h4 class="truncate text-sm font-semibold">{{ series.title }}</h4>
                </div>
              </div>
              <span class="sr-only">Abrir marcadores de {{ series.title }}</span>
            </button>
            <button
              type="button"
              class="series-reprocess-btn"
              :disabled="reprocessingKey !== null"
              data-testid="series-reprocess"
              :title="`Forçar reprocessamento de ${series.title}`"
              :aria-label="`Forçar reprocessamento de ${series.title}`"
              @click="handleReprocess(series.ratingKey, $event)"
            >
              <AppIcon
                name="refresh-cw"
                size="sm"
                :class="{ 'animate-spin': reprocessingKey === series.ratingKey }"
              />
              <span>{{ reprocessingKey === series.ratingKey ? "Reprocessando…" : "Reprocessar" }}</span>
            </button>
          </div>
        </div>

        <div v-if="hasMore" class="load-more-area">
          <div v-if="isLoadingMore" class="loading-indicator">
            <AppIcon name="loader" size="md" class="animate-spin" />
            <span>Carregando mais...</span>
          </div>
          <div ref="sentinelRef" class="h-1" aria-hidden="true"></div>
        </div>
      </div>

      <div v-else class="empty-state">
        <AppIcon name="tv" size="lg" class="mx-auto text-muted" />
        <p class="mt-3 text-sm font-medium text-secondary">Nenhuma série encontrada.</p>
        <p class="mt-2 text-xs text-tertiary">
          O catálogo desta biblioteca ainda não foi sincronizado.
        </p>
        <button
          v-if="!syncLoading"
          type="button"
          class="mt-4 px-4 py-2 rounded-lg border border-primary bg-primary text-white text-sm font-medium hover:bg-primary/90 transition-colors"
          @click="forceSyncCatalog"
        >
          <AppIcon name="refresh-cw" size="sm" class="inline mr-1" />
          Forçar sincronização
        </button>
        <button
          v-else
          type="button"
          class="mt-4 px-4 py-2 rounded-lg border border-border bg-muted text-muted text-sm font-medium cursor-not-allowed"
          disabled
        >
          <AppIcon name="loader" size="sm" class="inline mr-1 animate-spin" />
          Sincronizando...
        </button>
      </div>
    </section>

    <div v-if="syncMessage" class="sync-message" :class="syncType">
      {{ syncMessage }}
    </div>
  </div>
</template>

<style scoped>
.series-view {
  padding: 1.5rem;
  max-width: 100%;
}

/* Tab Navigation */
.tabs-nav {
  display: flex;
  gap: 0.5rem;
  margin-bottom: 1.5rem;
  border-bottom: 1px solid var(--border);
  padding-bottom: 0.5rem;
  flex-wrap: wrap;
}

.tab-btn {
  display: inline-flex;
  align-items: center;
  gap: 0.5rem;
  padding: 0.625rem 1rem;
  border: 1px solid transparent;
  border-radius: 8px;
  background: transparent;
  color: var(--text-secondary);
  font-size: 0.875rem;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.2s;
  white-space: nowrap;
}

.tab-btn:hover {
  background: var(--bg-sunken);
  color: var(--text);
  border-color: var(--border);
}

.tab-btn:focus-visible {
  outline: none;
  box-shadow: 0 0 0 2px var(--primary);
  border-color: var(--primary);
}

.tab-btn--active {
  background: var(--primary);
  color: var(--primary-foreground);
  border-color: var(--primary);
}

.tab-btn--active:hover {
  opacity: 0.9;
  color: var(--primary-foreground);
  border-color: var(--primary);
}

.tab-count {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  min-width: 1.5rem;
  height: 1.25rem;
  padding: 0 0.375rem;
  border-radius: 9999px;
  background: var(--bg-sunken);
  color: var(--text-tertiary);
  font-size: 0.6875rem;
  font-weight: 600;
}

.tab-btn--active .tab-count {
  background: color-mix(in srgb, var(--primary-foreground) 20%, transparent);
  color: var(--primary-foreground);
}

.tab-panel {
  animation: fadeIn 0.2s ease;
}

@keyframes fadeIn {
  from { opacity: 0; transform: translateY(4px); }
  to { opacity: 1; transform: translateY(0); }
}

/* Hide content when not active (v-show uses display: none) */
.tab-panel[style*="display: none"] {
  display: none !important;
}

.series-toolbar {
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  justify-content: space-between;
  gap: 1rem;
  margin-bottom: 1.5rem;
}

.search-box {
  position: relative;
  flex: 1;
  min-width: 200px;
  max-width: 400px;
}

.search-box .input {
  width: 100%;
  padding: 0.625rem 0.875rem 0.625rem 2.5rem;
  border: 1px solid var(--border);
  border-radius: 8px;
  background: var(--bg-elevated);
  color: var(--text);
  font-size: 0.875rem;
  transition: border-color 0.2s, box-shadow 0.2s;
}

.search-box .input:focus {
  outline: none;
  border-color: var(--primary);
  box-shadow: 0 0 0 3px color-mix(in srgb, var(--primary) 20%, transparent);
}

.search-box AppIcon {
  position: absolute;
  left: 0.75rem;
  top: 50%;
  transform: translateY(-50%);
  color: var(--text-tertiary);
  pointer-events: none;
}

.count {
  font-size: 0.8125rem;
  color: var(--text-secondary);
  white-space: nowrap;
}

.alert {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  padding: 0.75rem 1rem;
  border-radius: 8px;
  background: color-mix(in srgb, var(--danger) 10%, transparent);
  color: var(--danger);
  margin-bottom: 1rem;
}

.loading-skeleton {
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
}

.bg-skeleton {
  background: linear-gradient(90deg, var(--bg-sunken) 25%, var(--border) 50%, var(--bg-sunken) 75%);
  background-size: 200% 100%;
  animation: shimmer 1.5s infinite;
  border-radius: 8px;
}

@keyframes shimmer {
  0% { background-position: -200% 0; }
  100% { background-position: 200% 0; }
}

.series-grid-section {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.series-grid-count {
  font-size: 0.875rem;
  color: var(--text-secondary);
  margin: 0;
}

.series-grid {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: 0.75rem;
}

@media (min-width: 640px) {
  .series-grid { grid-template-columns: repeat(3, 1fr); }
}

@media (min-width: 768px) {
  .series-grid { grid-template-columns: repeat(4, 1fr); }
}

@media (min-width: 1024px) {
  .series-grid { grid-template-columns: repeat(5, 1fr); }
}

@media (min-width: 1280px) {
  .series-grid { grid-template-columns: repeat(6, 1fr); }
}

@media (min-width: 1536px) {
  .series-grid { grid-template-columns: repeat(7, 1fr); }
}

.series-card {
  border-radius: 12px;
  overflow: hidden;
  background: var(--bg-elevated);
  border: 1px solid var(--border);
  box-shadow: 0 1px 3px var(--shadow);
  transition: box-shadow 0.3s, transform 0.2s;
}

.series-card:hover {
  box-shadow: 0 10px 25px var(--shadow-lg);
  transform: translateY(-2px);
}

.series-card-poster {
  position: relative;
  aspect-ratio: 2/3;
  width: 100%;
  overflow: hidden;
  background: var(--bg-sunken);
}

.series-card-poster img {
  width: 100%;
  height: 100%;
  object-fit: cover;
  transition: transform 0.5s;
}

.series-card:hover .series-card-poster img {
  transform: scale(1.05);
}

.series-card-poster h4 {
  margin: 0;
  font-size: 0.8125rem;
  font-weight: 600;
  line-height: 1.3;
  text-overflow: ellipsis;
  white-space: nowrap;
  overflow: hidden;
}

.series-card-main {
  background: transparent;
  border: none;
  padding: 0;
  cursor: pointer;
  text-align: left;
  flex: 1;
}

.series-reprocess-btn {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  gap: 0.375rem;
  padding: 0.5rem;
  border: none;
  border-top: 1px solid var(--border);
  background: var(--bg-elevated);
  color: var(--text-secondary);
  font-size: 0.75rem;
  font-weight: 500;
  cursor: pointer;
  transition: background 0.2s, color 0.2s;
}

.series-reprocess-btn:hover:not(:disabled) {
  background: color-mix(in srgb, var(--primary) 12%, transparent);
  color: var(--primary);
}

.series-reprocess-btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.load-more-area {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 0.75rem;
  padding: 1.5rem;
}

.loading-indicator {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  color: var(--text-secondary);
  font-size: 0.875rem;
}

.empty-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  padding: 3rem;
  text-align: center;
  border: 1px solid var(--border);
  border-radius: 12px;
  background: var(--bg-elevated);
}

.sync-message {
  margin-top: 1rem;
  padding: 0.75rem 1rem;
  border-radius: 8px;
  font-size: 0.875rem;
}

.sync-message.success {
  background: color-mix(in srgb, var(--success) 10%, transparent);
  border: 1px solid color-mix(in srgb, var(--success) 30%, transparent);
  color: var(--success);
}

.sync-message.error {
  background: color-mix(in srgb, var(--danger) 10%, transparent);
  border: 1px solid color-mix(in srgb, var(--danger) 30%, transparent);
  color: var(--danger);
}
</style>