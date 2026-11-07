<script setup>
import { computed, ref, watch } from "vue";
import { useRoute, useRouter } from "vue-router";
import DefaultLayout from "@/components/layouts/DefaultLayout.vue";
import AppIcon from "@/components/AppIcon.vue";
import BreadcrumbDefault from "@/components/Breadcrumbs/BreadcrumbDefault.vue";
import { useSeriesCatalog } from "@/composables/useSeriesCatalog";
import { apiFetch } from "@/api";

const route = useRoute();
const router = useRouter();
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

const libraryTitleRef = ref("");

// Carrega título da biblioteca via API assim que o key fica disponível
watch(libraryKey, async (key) => {
  if (!key) {
    libraryTitleRef.value = "";
    return;
  }
  try {
    const res = await apiFetch("/api/plex/libraries");
    if (!res.ok) return;
    const libs = await res.json();
    const lib = libs.find(l => String(l.key) === String(key));
    libraryTitleRef.value = lib?.title ?? "";
  } catch {
    libraryTitleRef.value = "";
  }
}, { immediate: true });

// Título combinado: usa título da API se disponível, senão fallback por key
const computedLibraryTitle = computed(() =>
  libraryTitleRef.value || (libraryKey.value ? `Biblioteca ${libraryKey.value}` : "Biblioteca")
);

const breadcrumbItems = computed(() => [
  { label: "Bibliotecas", to: { name: "libraries" } },
  { label: computedLibraryTitle.value, isCurrent: true },
]);

function openMarkers(series) {
  router.push({
    name: "series-markers",
    params: { seriesKey: series.ratingKey },
    query: {
      libraryKey: libraryKey.value,
      libraryTitle: computedLibraryTitle.value,
      seriesTitle: series.title,
    },
  });
}
</script>

<template>
  <DefaultLayout>
    <BreadcrumbDefault
      :items="breadcrumbItems"
      @select="(to) => to && router.push(to)"
    />

    <div class="mb-6 flex flex-wrap items-center gap-4">
      <div class="relative flex-1">
        <span class="absolute left-3 top-1/2 -translate-y-1/2 text-body dark:text-bodydark">
          <AppIcon name="search" size="sm" />
        </span>
        <input
          v-model="seriesQuery"
          class="w-full rounded-lg border border-stroke bg-white py-3 pl-10 pr-4 text-sm text-black outline-none focus:border-primary dark:border-form-strokedark dark:bg-form-input dark:text-white"
          type="search"
          placeholder="Buscar série..."
          aria-label="Buscar série"
        />
      </div>
      <span v-if="!seriesLoading" class="text-sm font-medium text-body dark:text-bodydark">
        {{ seriesTotalSize }} série(s)
      </span>
    </div>

    <div
      v-if="seriesError"
      class="mb-6 flex flex-wrap items-center gap-3 rounded-sm border border-meta-1/30 bg-meta-1/10 px-5 py-4 text-sm text-meta-1"
      role="alert"
    >
      <AppIcon name="warning" size="sm" />
      <span class="flex-1">{{ seriesError }}</span>
      <button
        class="rounded-lg border border-meta-1/40 px-3 py-1.5 font-medium transition hover:bg-meta-1/10"
        @click="refresh"
      >
        Tentar novamente
      </button>
    </div>

    <div
      v-else-if="seriesLoading && seriesList.length === 0"
      class="grid grid-cols-2 gap-5 sm:grid-cols-3 lg:grid-cols-4 xl:grid-cols-5 2xl:gap-7.5"
    >
      <div v-for="n in 24" :key="n" class="overflow-hidden rounded-sm border border-stroke bg-white dark:border-strokedark dark:bg-boxdark">
        <div class="aspect-[2/3] animate-pulse bg-gray-2 dark:bg-meta-4"></div>
        <div class="space-y-2 p-3">
          <div class="h-4 w-3/4 animate-pulse rounded bg-gray-2 dark:bg-meta-4"></div>
          <div class="h-3 w-1/2 animate-pulse rounded bg-gray-2 dark:bg-meta-4"></div>
        </div>
      </div>
    </div>

    <div v-else-if="seriesList.length > 0">
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
            <p v-if="series.summary" class="mt-1 line-clamp-2 text-xs text-body dark:text-bodydark">
              {{ series.summary }}
            </p>
          </div>
        </button>

        <template v-if="isLoadingMore">
          <div v-for="n in 6" :key="'more-' + n" class="overflow-hidden rounded-sm border border-stroke bg-white dark:border-strokedark dark:bg-boxdark">
            <div class="aspect-[2/3] animate-pulse bg-gray-2 dark:bg-meta-4"></div>
            <div class="space-y-2 p-3">
              <div class="h-4 w-3/4 animate-pulse rounded bg-gray-2 dark:bg-meta-4"></div>
            </div>
          </div>
        </template>

        <div ref="sentinelRef" class="col-span-full h-1" aria-hidden="true"></div>
      </div>

      <div v-if="!hasMore" class="py-6 text-center text-sm font-medium text-body dark:text-bodydark">
        Fim do catálogo
      </div>
    </div>

    <div
      v-else-if="!seriesLoading && !seriesError"
      class="rounded-sm border border-stroke bg-white px-6 py-11 text-center dark:border-strokedark dark:bg-boxdark"
    >
      <AppIcon name="tv" size="lg" class="mx-auto text-body dark:text-bodydark" />
      <p class="mt-3 text-sm font-medium text-body dark:text-bodydark">Nenhuma série encontrada.</p>
    </div>
  </DefaultLayout>
</template>