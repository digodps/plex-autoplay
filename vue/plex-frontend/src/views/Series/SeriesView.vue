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
    <div
      v-if="viewState === 'error'"
      class="mb-6 flex items-center gap-3 rounded-sm border border-meta-1/30 bg-meta-1/10 px-5 py-4 text-sm font-medium text-meta-1"
      role="alert"
    >
      <AppIcon name="warning" size="sm" />
      {{ libraryError }}
    </div>

    <div
      v-else-if="seriesError"
      class="mb-6 flex items-center gap-3 rounded-sm border border-meta-1/30 bg-meta-1/10 px-5 py-4 text-sm font-medium text-meta-1"
      role="alert"
    >
      <AppIcon name="warning" size="sm" />
      {{ seriesError }}
    </div>

    <div
      v-if="viewState === 'loading' || viewState === 'title-loading'"
      class="grid grid-cols-2 gap-5 sm:grid-cols-3 lg:grid-cols-4 xl:grid-cols-5 2xl:gap-7.5"
      aria-hidden="true"
    >
      <div v-for="n in 10" :key="n" class="overflow-hidden rounded-sm border border-stroke bg-white dark:border-strokedark dark:bg-boxdark">
        <div class="aspect-[2/3] animate-pulse bg-gray-2 motion-reduce:animate-none dark:bg-meta-4"></div>
        <div class="space-y-2 p-3">
          <div class="h-4 w-3/4 animate-pulse rounded bg-gray-2 motion-reduce:animate-none dark:bg-meta-4"></div>
          <div class="h-3 w-1/2 animate-pulse rounded bg-gray-2 motion-reduce:animate-none dark:bg-meta-4"></div>
        </div>
      </div>
    </div>

    <div v-else-if="viewState === 'list'">
      <p class="mb-4 text-sm text-body dark:text-bodydark" role="status">
        {{ seriesTotalSize }} séries
      </p>

      <div class="grid grid-cols-2 gap-5 sm:grid-cols-3 lg:grid-cols-4 xl:grid-cols-5 2xl:gap-7.5">
        <button
          v-for="series in seriesList"
          :key="series.ratingKey"
          type="button"
          class="group overflow-hidden rounded-sm border border-stroke bg-white text-left shadow-default transition hover:border-primary dark:border-strokedark dark:bg-boxdark"
          @click="openMarkers(series)"
        >
          <div class="aspect-[2/3] bg-gray-2 dark:bg-meta-4">
            <img
              v-if="series.thumb"
              :src="series.thumb"
              :alt="series.title"
              loading="lazy"
              class="h-full w-full object-cover"
            />
            <div v-else class="flex h-full items-center justify-center text-body dark:text-bodydark">
              <AppIcon name="tv" size="lg" />
            </div>
          </div>
          <div class="p-3">
            <h4 class="truncate text-sm font-semibold text-black dark:text-white">{{ series.title }}</h4>
          </div>
        </button>

        <div
          v-for="n in isLoadingMore ? 5 : 0"
          :key="'more-' + n"
          class="overflow-hidden rounded-sm border border-stroke bg-white dark:border-strokedark dark:bg-boxdark"
          aria-hidden="true"
        >
          <div class="aspect-[2/3] animate-pulse bg-gray-2 motion-reduce:animate-none dark:bg-meta-4"></div>
          <div class="p-3">
            <div class="h-4 w-3/4 animate-pulse rounded bg-gray-2 motion-reduce:animate-none dark:bg-meta-4"></div>
          </div>
        </div>
      </div>

      <div ref="sentinelRef" class="h-1" aria-hidden="true"></div>
    </div>

    <div
      v-else-if="viewState === 'empty'"
      class="rounded-sm border border-stroke bg-white px-6 py-11 text-center dark:border-strokedark dark:bg-boxdark"
    >
      <AppIcon name="tv" size="lg" class="mx-auto text-body dark:text-bodydark" />
      <p class="mt-3 text-sm font-medium text-body dark:text-bodydark">Nenhuma série encontrada.</p>
    </div>
  </DefaultLayout>
</template>
