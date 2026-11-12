<script setup>
import { computed } from "vue";
import { useRoute, useRouter } from "vue-router";
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

const {
  seriesList,
  seriesTotalSize,
  seriesLoading,
  seriesError,
  sentinelRef,
  syncLoading,
  syncMessage,
  syncType,
  forceSyncCatalog,
} = useSeriesCatalog();

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
  <div class="page">
    <div class="bg-gray-50 dark:bg-gray-900 min-h-screen">
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

          <div class="grid grid-cols-2 gap-3 sm:grid-cols-3 md:grid-cols-4 lg:grid-cols-5 xl:grid-cols-6 2xl:grid-cols-7 max-w-[90vw] md:max-w-7xl lg:max-w-[1200px] xl:max-w-[1400px] 2xl:max-w-[1600px] mx-auto">
            <button
              v-for="series in seriesList"
              :key="series.ratingKey"
              type="button"
              data-testid="series-card"
              class="group flex flex-col rounded-lg border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-800 p-2 sm:p-3 shadow-sm transition-colors hover:border-primary focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-primary focus-visible:ring-offset-2 dark:focus-visible:ring-offset-gray-900"
              @click="openMarkers(series)"
            >
              <div class="relative mb-2 aspect-[2/3] w-full overflow-hidden rounded-md bg-gray-100 dark:bg-gray-700">
                <div
                  data-testid="series-thumb-inner"
                  class="h-full w-full transition-transform duration-300 motion-safe:group-hover:scale-105 motion-safe:group-focus-visible:scale-105 motion-reduce:transition-none"
                >
                  <img
                    v-if="series.thumb"
                    :src="series.thumb"
                    :alt="series.title"
                    loading="lazy"
                    class="h-full w-full object-cover"
                  />
                  <div v-else class="flex h-full w-full items-center justify-center text-gray-400 dark:text-gray-500">
                    <AppIcon name="tv" size="lg" />
                  </div>
                </div>
                <div
                  aria-hidden="true"
                  class="pointer-events-none absolute inset-0 bg-black/0 transition-colors group-hover:bg-black/25 group-focus-visible:bg-black/25 motion-reduce:transition-none"
                ></div>
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
          <p class="mt-2 text-xs text-gray-500 dark:text-gray-400">
            O catálogo desta biblioteca ainda não foi sincronizado.
          </p>
          <button
            v-if="!syncLoading"
            type="button"
            class="mt-4 px-4 py-2 rounded-lg border border-primary bg-primary text-white text-sm font-medium hover:bg-primary/90 transition-colors dark:bg-primary dark:text-white dark:hover:bg-primary/90"
            @click="forceSyncCatalog"
          >
            <AppIcon name="refresh-cw" size="sm" class="inline mr-1" />
            Forçar sincronização
          </button>
          <button
            v-else
            type="button"
            class="mt-4 px-4 py-2 rounded-lg border border-gray-400 bg-gray-200 dark:bg-gray-700 text-gray-500 dark:text-gray-400 text-sm font-medium cursor-not-allowed"
            disabled
          >
            <AppIcon name="loader" size="sm" class="inline mr-1 animate-spin" />
            Sincronizandoîl...
          </button>
        </div>

        <div
          v-if="syncMessage"
          :class="[
            'mt-4 rounded-lg border p-4 text-sm',
            syncType === 'success'
              ? 'border-green-300 dark:border-green-700 bg-green-50 dark:bg-green-900/30 text-green-800 dark:text-green-200'
              : 'border-red-300 dark:border-red-700 bg-red-50 dark:bg-red-900/30 text-red-800 dark:text-red-200',
          ]"
          role="alert"
        >
          {{ syncMessage }}
        </div>
      </div>
      </div>
    </div>
  </template>