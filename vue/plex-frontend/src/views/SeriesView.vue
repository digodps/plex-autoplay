<script setup>
import { computed } from "vue";
import { useRoute } from "vue-router";
import AppIcon from "../components/AppIcon.vue";
import Breadcrumb from "../components/Breadcrumb.vue";
import { useSeriesCatalog } from "../composables/useSeriesCatalog";
import "../views/SeriesView.css";

const route = useRoute();
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
