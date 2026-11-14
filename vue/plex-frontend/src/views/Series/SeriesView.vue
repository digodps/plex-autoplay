<script setup>
import { computed } from "vue";
import { useRoute } from "vue-router";
import AppIcon from "@/components/AppIcon.vue";
import Breadcrumb from "@/components/Breadcrumb.vue";
import GenreCarousel from "@/components/GenreCarousel.vue";
import { useSeriesByGenre } from "@/composables/useSeriesByGenre";
import "@/views/SeriesView.css";

const route = useRoute();

const libraryKey = computed(() => route.params.key);
const queryTitle = computed(() => route.query.libraryTitle);

const { genreGroups, isLoading, error } = useSeriesByGenre({
  libraryKey: libraryKey.value,
});

const libraryTitle = computed(
  () => queryTitle.value || `Biblioteca ${libraryKey.value ?? ""}`.trim(),
);

const breadcrumbItems = computed(() => [
  { label: "Bibliotecas", to: { name: "libraries" } },
  { label: libraryTitle.value, current: true },
]);

const totalSeries = computed(() =>
  genreGroups.value.reduce((sum, g) => sum + g.series.length, 0),
);
</script>

<template>
  <div class="series-view">
    <Breadcrumb :items="breadcrumbItems" />

    <div class="series-toolbar">
      <div class="search-box">
        <AppIcon name="search" size="sm" />
        <input
          class="input"
          type="search"
          placeholder="Buscar série... (use a busca no topo)"
          aria-label="Buscar série"
          disabled
        />
      </div>
      <span v-if="!isLoading" class="count">
        {{ totalSeries }} série(s) em {{ genreGroups.length }} gênero(s)
      </span>
    </div>

    <GenreCarousel
      :genre-groups="genreGroups"
      :is-loading="isLoading"
      :error="error"
    />
  </div>
</template>
