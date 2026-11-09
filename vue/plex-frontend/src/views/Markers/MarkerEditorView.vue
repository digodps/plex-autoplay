<script setup>
import { computed, onMounted } from "vue";
import { useRoute } from "vue-router";
import AppIcon from "@/components/AppIcon.vue";
import MarkerRow from "@/components/MarkerRow.vue";
import { useMarkerStore } from "@/stores/markers";

const route = useRoute();
const store = useMarkerStore();
const seriesKey = computed(() => route.params.seriesKey);

onMounted(() => {
  store.fetchSeries(seriesKey.value);
});

const seasons = computed(() => {
  const bySeason = new Map();
  for (const ep of Object.values(store.episodesMap)) {
    if (!bySeason.has(ep.season)) bySeason.set(ep.season, []);
    bySeason.get(ep.season).push(ep);
  }
  for (const list of bySeason.values()) list.sort((a, b) => a.episode - b.episode);
  return [...bySeason.entries()]
    .sort((a, b) => a[0] - b[0])
    .map(([season, list]) => ({ season, list }));
});
</script>

<template>
  <div class="page">
    <div class="bg-gray-50 dark:bg-gray-900 min-h-screen">
      <div class="border-b border-gray-200 dark:border-gray-800 sticky top-0 z-20 bg-gray-50 dark:bg-gray-900">
        <div class="max-w-2xl mx-auto px-4 py-3 flex items-center gap-2">
          <h1 class="text-28 font-bold text-gray-900 dark:text-gray-100">
            Marcadores da série
          </h1>
          <p class="text-sm text-gray-500 dark:text-gray-400">
            Selecione um campo de intro ou créditos para editar ou adicionar.
          </p>
        </div>
      </div>

      <div class="p-4 md:p-6 lg:p-8 flex flex-col gap-6">
        <div
          v-if="store.error"
          class="rounded-lg border border-gray-200 dark:border-gray-600 bg-gray-900 dark:bg-gray-800 p-4 text-sm"
          role="alert"
        >
          <svg
            class="w-4 h-4 text-gray-400 dark:text-gray-600"
            fill="none"
            stroke="currentColor"
            viewBox="0 0 24 24"
          >
            <path
              stroke-linecap="round"
              stroke-linejoin="round"
              stroke-width="2"
              d="M12 9v2m0 4h.01m-6.938 4h13.856c1.535-3 0-3.506 0-5 0-1.506.376-2.926 1.029-4.2m-6.938 4L12 9m-6.938 4h13.856c1.535-3 0-3.506 0-5 0-1.506.376-2.926 1.029-4.2"
            />
          </svg>
          <span class="ml-2">{{ store.error }}</span>
        </div>

        <div v-if="store.loading" class="space-y-4 animate-pulse">
          <div class="h-24 rounded-lg border border-gray-200 dark:border-gray-700 bg-gray-100 dark:bg-gray-800"></div>
          <div class="h-20 rounded-lg border border-gray-200 dark:border-gray-700 bg-gray-100 dark:bg-gray-800"></div>
          <div class="h-16 rounded-lg border border-gray-200 dark:border-gray-700 bg-gray-100 dark:bg-gray-800"></div>
        </div>

        <div v-else class="space-y-6">
          <section v-for="{ season, list } in seasons" :key="season" class="rounded-lg border border-gray-200 dark:border-gray-600 bg-white dark:bg-gray-800">
            <h2 class="border-b border-gray-200 dark:border-gray-600 px-6 py-3 text-title-sm font-semibold text-gray-900 dark:text-gray-100">
              Temporada {{ season }}
            </h2>

            <div class="divide-y divide-gray-200 dark:divide-gray-600">
              <article v-for="ep in list" :key="ep.rating_key" class="flex flex-col items-stretch gap-4 px-6 py-4 sm:flex-row sm:items-start">
                <div class="aspect-video w-full shrink-0 overflow-hidden rounded-lg bg-gray-100 dark:bg-gray-700 sm:w-48">
                  <img
                    v-if="ep.thumb"
                    :src="ep.thumb"
                    :alt="ep.title"
                    loading="lazy"
                    class="h-full w-full object-cover"
                  />
                  <div v-else class="flex h-full w-full items-center justify-center text-gray-400 dark:text-gray-500">
                    <AppIcon name="tv" size="sm" />
                  </div>
                </div>
                <div class="flex-1 min-w-0">
                  <div class="mb-2 flex flex-wrap items-center gap-2">
                    <span
                      class="inline-flex rounded-full bg-gray-100 px-2.5 py-0.5 text-xs font-semibold text-gray-700 dark:bg-gray-800 dark:text-gray-200"
                    >
                      T{{ ep.season }} · E{{ ep.episode }}
                    </span>
                    <h3 class="text-title-sm font-semibold text-gray-900 dark:text-gray-100">{{ ep.title }}</h3>
                  </div>

                  <div class="space-y-1">
                    <!---->
                    <MarkerRow
                      v-for="kind in ['intro', 'credits']"
                      :key="kind"
                      :rating-key="ep.rating_key"
                      :kind="kind"
                    />
                  </div>
                </div>
              </article>
            </div>
          </section>

          <div
            v-if="seasons.length === 0"
            class="rounded-lg border border-gray-200 dark:border-gray-600 bg-gray-900 dark:bg-gray-800 p-8 text-center"
          >
            <AppIcon name="tv" size="lg" class="mx-auto text-gray-400 dark:text-gray-600" />
            <p class="mt-3 text-sm font-medium text-gray-300 dark:text-gray-400">
              Nenhum episódio nesta série.
            </p>
          </div>
        </div>
      </div>
      </div>
    </div>
  </template>