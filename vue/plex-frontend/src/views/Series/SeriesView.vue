<script setup>
import { computed } from "vue";
import { useRoute, useRouter } from "vue-router";
import DefaultLayout from "@/components/layouts/DefaultLayout.vue";
import AppIcon from "@/components/AppIcon.vue";
import { useSeriesCatalog } from "@/composables/useSeriesCatalog";
import { useLibraryTitle } from "@/composables/useLibraryTitle";

const route = useRoute();
const router = useRouter();

const libraryKey = computed(() => route.params.key);
const queryTitle = computed(() => route.query.libraryTitle);

const {
  title: libraryTitle,
  loading: libraryTitleLoading,
  error: libraryError,
} = useLibraryTitle(libraryKey, queryTitle);

const { seriesList, seriesTotalSize, seriesLoading, isLoadingMore, seriesError, sentinelRef } =
  useSeriesCatalog();

const viewState = computed(() => {
  if (libraryTitleLoading.value) return "title-loading";
  if (libraryError.value) return "error";
  if (seriesLoading.value && seriesList.value.length === 0) return "loading";
  if (seriesList.value.length > 0) return "list";
  return "empty";
});

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
  <DefaultLayout>
    <div class="bg-gray-50 dark:bg-gray-900 min-h-screen">
      <div class="border-b border-gray-200 dark:border-gray-800 sticky top-0 z-20 bg-gray-50 dark:bg-gray-900">
        <div class="max-w-2xl mx-auto px-4 py-3 flex items-center gap-2">
          <h1 class="text-28 font-bold text-gray-900 dark:text-gray-100">
            Séries {{ libraryTitle ? `→ ${libraryTitle}` : "" }}
          </h1>
          <p class="text-sm text-gray-500 dark:text-gray-400">
            Séries da biblioteca selecionada.
          </p>
        </div>
      </div>

      <div class="p-4 md:p-6 lg:p-8 flex flex-col gap-6">
        <div
          v-if="viewState === 'error'"
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
          <span class="ml-2">{{ libraryError }}</span>
        </div>

        <div
          v-if="viewState === 'seriesError'"
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
          <span class="ml-2">{{ seriesError }}</span>
        </div>

        <div
          v-if="viewState === 'loading' || viewState === 'title-loading'"
          class="space-y-4 animate-pulse"
        >
          <div class="h-24 rounded-lg border border-gray-200 dark:border-gray-700 bg-gray-100 dark:bg-gray-800"></div>
          <div class="h-20 rounded-lg border border-gray-200 dark:border-gray-700 bg-gray-100 dark:bg-gray-800"></div>
          <div class="h-16 rounded-lg border border-gray-200 dark:border-gray-700 bg-gray-100 dark:bg-gray-800"></div>
        </div>

        <div
          v-if="viewState === 'list'"
          class="space-y-4"
        >
          <p class="text-sm text-gray-500 dark:text-gray-400" role="status">
            {{ seriesTotalSize }} séries
          </p>

          <div class="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4">
            <button
              v-for="series in seriesList"
              :key="series.ratingKey"
              type="button"
              class="group flex flex-col rounded-lg border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-800 p-4 shadow-sm transition-colors hover:border-primary dark:hover:text-primary"
              @click="openMarkers(series)"
            >
              <div class="flex h-20 w-20 items-center justify-center rounded-lg bg-gray-100 dark:bg-gray-700 text-primary transition group-hover:bg-primary dark:group-hover:text-white mb-3">
                <AppIcon name="tv" size="lg" />
              </div>
              <h4 class="truncate text-title-sm font-semibold text-gray-900 dark:text-gray-100">
                {{ series.title }}
              </h4>
              <span class="mt-1 text-sm font-medium text-gray-600 dark:text-gray-400">Ver marcadores</span>
            </button>
          </div>

          <div ref="sentinelRef" class="h-1" aria-hidden="true"></div>
        </div>

        <div
          v-if="viewState === 'empty'"
          class="rounded-lg border border-gray-200 dark:border-gray-700 bg-gray-900 dark:bg-gray-800 p-8 text-center"
        >
          <AppIcon name="tv" size="lg" class="mx-auto text-gray-400 dark:text-gray-600" />
          <p class="mt-3 text-sm font-medium text-gray-300 dark:text-gray-400">Nenhuma série encontrada.</p>
        </div>
      </div>
    </DefaultLayout>
  </template>