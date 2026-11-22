<script setup>
import { ref, onMounted, computed, watch } from "vue";
import { useRouter } from "vue-router";
import { useI18n } from "vue-i18n";
import { apiFetch } from "@/api";
import LibraryMosaic from "@/components/LibraryMosaic.vue";
import AppIcon from "@/components/AppIcon.vue";

const { t } = useI18n();
const router = useRouter();

const libraries = ref([]);
const error = ref("");
const loading = ref(false);
const searchQuery = ref("");
const viewMode = ref("grid"); // grid | list
const sortBy = ref("title"); // title | seriesCount

async function loadLibraries() {
  loading.value = true;
  error.value = "";
  try {
    const response = await apiFetch("/api/plex/libraries");
    if (response.ok) libraries.value = await response.json();
    else error.value = t("libraries.error");
  } catch {
    error.value = t("errors.network");
  } finally {
    loading.value = false;
  }
}

onMounted(loadLibraries);

const filteredLibraries = computed(() => {
  let result = libraries.value;

  if (searchQuery.value.trim()) {
    const q = searchQuery.value.trim().toLowerCase();
    result = result.filter((lib) =>
      lib.title.toLowerCase().includes(q)
    );
  }

  result = [...result].sort((a, b) => {
    if (sortBy.value === "title") return a.title.localeCompare(b.title);
    return (b.seriesCount || 0) - (a.seriesCount || 0);
  });

  return result;
});

function navigateToSeries(libraryKey, libraryTitle) {
  router.push({
    name: "series",
    params: { key: libraryKey },
    query: { libraryTitle },
  });
}

function refresh() {
  loadLibraries();
}
</script>

<template>
  <div class="mx-auto max-w-7xl px-4 py-6">
    <!-- Header -->
    <header class="mb-6 flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
      <div>
        <h1 class="text-2xl font-semibold text-black dark:text-white">{{ t("libraries.title") }}</h1>
        <p class="mt-1 text-sm text-gray-500 dark:text-gray-400">{{ t("libraries.subtitle") }}</p>
      </div>

      <div class="flex flex-wrap items-center gap-3">
        <!-- Search -->
        <div class="relative w-full sm:w-64" role="search" aria-label="Buscar bibliotecas">
          <AppIcon name="search" size="sm" class="absolute left-3 top-1/2 -translate-y-1/2 text-gray-400" aria-hidden="true" />
          <input
            v-model="searchQuery"
            type="search"
            :placeholder="t('series.searchPlaceholder')"
            class="input w-full pl-10 pr-4 py-2"
            aria-label="Buscar bibliotecas"
            data-testid="library-search"
          />
        </div>

        <!-- Sort -->
        <div class="flex items-center gap-2" role="group" aria-label="Ordenar e visualizar">
          <label for="sort-by" class="sr-only">{{ t("series.sortBy") }}</label>
          <select
            id="sort-by"
            v-model="sortBy"
            class="input w-auto py-2 pr-8 text-sm"
            aria-label="Ordenar por"
            data-testid="library-sort"
          >
            <option value="title">{{ t("series.sortTitle") }}</option>
            <option value="seriesCount">{{ t("series.seriesCount", { count: "" }).trim() }}</option>
          </select>

          <!-- View mode toggle -->
          <div class="flex rounded-lg border border-stroke bg-white dark:bg-gray-800 dark:border-gray-700" role="group" aria-label="Modo de visualização">
            <button
              type="button"
              :aria-pressed="viewMode === 'grid'"
              :class="[
                'p-2 rounded-lg transition-colors flex items-center justify-center',
                viewMode === 'grid'
                  ? 'bg-primary text-white'
                  : 'text-gray-500 hover:bg-gray-100 dark:text-gray-400 dark:hover:bg-gray-700',
              ]"
              @click="viewMode = 'grid'"
              :aria-label="t('libraries.viewGrid')"
              data-testid="view-grid"
            >
              <AppIcon name="grid" size="sm" />
            </button>
            <button
              type="button"
              :aria-pressed="viewMode === 'list'"
              :class="[
                'p-2 rounded-lg transition-colors flex items-center justify-center',
                viewMode === 'list'
                  ? 'bg-primary text-white'
                  : 'text-gray-500 hover:bg-gray-100 dark:text-gray-400 dark:hover:bg-gray-700',
              ]"
              @click="viewMode = 'list'"
              :aria-label="t('libraries.viewList')"
              data-testid="view-list"
            >
              <AppIcon name="list" size="sm" />
            </button>
          </div>

          <button
            type="button"
            @click="refresh"
            :disabled="loading"
            class="btn btn-secondary py-2 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-primary focus-visible:ring-offset-2 dark:focus-visible:ring-offset-gray-900"
            :aria-label="t('common.refresh')"
            data-testid="library-refresh"
          >
            <AppIcon :name="loading ? 'spinner' : 'refresh-cw'" size="sm" :class="loading ? 'spin' : ''" aria-hidden="true" />
            <span class="hidden sm:inline">{{ t("common.refresh") }}</span>
          </button>
        </div>
      </div>
    </header>

    <!-- Content -->
    <main>
      <!-- Error state -->
      <div
        v-if="error"
        class="mb-6 rounded-lg border border-danger/30 bg-danger/10 p-4 text-sm text-danger flex items-center gap-2"
        role="alert"
        aria-live="assertive"
      >
        <AppIcon name="alert-circle" size="sm" class="flex-shrink-0" />
        {{ error }}
        <button
          type="button"
          @click="refresh"
          class="ml-auto btn btn-sm btn-danger"
        >
          {{ t("common.retry") }}
        </button>
      </div>

      <!-- Loading skeleton -->
      <div v-else-if="loading" class="grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-3" aria-busy="true" aria-label="Carregando bibliotecas">
        <div v-for="i in 6" :key="i" class="h-56 rounded-xl border border-stroke bg-white dark:bg-gray-800 animate-pulse dark:border-gray-700" />
      </div>

      <!-- Grid view -->
      <div
        v-else-if="viewMode === 'grid' && filteredLibraries.length"
        class="grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4"
        role="list"
        aria-label="Bibliotecas"
      >
        <article
          v-for="lib in filteredLibraries"
          :key="lib.key"
          class="group rounded-xl border border-stroke bg-white dark:bg-gray-800 overflow-hidden dark:border-gray-700 hover:border-accent/50 transition-all focus-within:ring-2 focus-within:ring-primary focus-within:ring-offset-2 dark:focus-within:ring-offset-gray-900"
        >
          <button
            type="button"
            @click="navigateToSeries(lib.key, lib.title)"
            class="w-full focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-primary focus-visible:ring-offset-2 dark:focus-visible:ring-offset-gray-900"
            :aria-label="t('libraries.openLibrary', { title: lib.title })"
            data-testid="library-card"
          >
            <LibraryMosaic :library-key="lib.key" />
            <div class="p-4">
              <h3 class="text-lg font-semibold text-black dark:text-white truncate">{{ lib.title }}</h3>
              <p class="mt-2 text-sm font-medium text-gray-500 dark:text-gray-400 flex items-center gap-1 group-hover:text-amber-600 dark:group-hover:text-amber-400 transition-colors">
                <span>{{ t("series.seriesCount", { count: lib.seriesCount || 0 }) }}</span>
                <span class="transform transition-transform group-hover:translate-x-0.5" aria-hidden="true">›</span>
              </p>
            </div>
          </button>
        </article>
      </div>

      <!-- List view -->
      <div
        v-else-if="viewMode === 'list' && filteredLibraries.length"
        class="divide-y divide-stroke dark:divide-gray-700"
        role="list"
        aria-label="Bibliotecas"
      >
        <article
          v-for="lib in filteredLibraries"
          :key="lib.key"
          class="group py-4 flex items-center gap-4 hover:bg-gray-50 dark:hover:bg-gray-800/50 transition-colors focus-within:ring-2 focus-within:ring-primary focus-within:ring-offset-2 dark:focus-within:ring-offset-gray-900 rounded-lg"
        >
          <button
            type="button"
            @click="navigateToSeries(lib.key, lib.title)"
            class="flex-1 text-left focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-primary focus-visible:ring-offset-2 dark:focus-visible:ring-offset-gray-900 rounded-lg"
            :aria-label="t('libraries.openLibrary', { title: lib.title })"
            data-testid="library-card"
          >
            <div class="flex items-center gap-4 min-w-0">
              <LibraryMosaic :library-key="lib.key" class="h-24 w-32 flex-shrink-0 rounded-lg" />
              <div class="min-w-0">
                <h3 class="text-lg font-semibold text-black dark:text-white truncate">{{ lib.title }}</h3>
                <p class="mt-1 text-sm text-gray-500 dark:text-gray-400">
                  {{ t("series.seriesCount", { count: lib.seriesCount || 0 }) }}
                </p>
              </div>
            </div>
          </button>
          <AppIcon name="chevron-right" size="sm" class="text-gray-400 group-hover:text-primary transition-colors flex-shrink-0" aria-hidden="true" />
        </article>
      </div>

      <!-- Empty state -->
      <div
        v-else-if="!loading && !error && (!libraries.length || !filteredLibraries.length)"
        class="rounded-xl border border-stroke bg-white dark:bg-gray-800 p-12 text-center dark:border-gray-700"
      >
        <AppIcon name="tv" size="xl" class="mx-auto text-gray-500 dark:text-gray-400" />
        <p class="mt-3 text-sm font-medium text-gray-500 dark:text-gray-400">
          {{ searchQuery.value ? t("series.noSeries") : t("libraries.noLibraries") }}
        </p>
        <p v-if="searchQuery.value" class="mt-1 text-xs text-gray-400 dark:text-gray-500">
          {{ t("series.clearSearch") }}
        </p>
      </div>
    </main>
  </div>
</template>

<style scoped>
.spin {
  animation: rotate 0.9s linear infinite;
}
@keyframes rotate {
  to { transform: rotate(360deg); }
}
</style>