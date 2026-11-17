<script setup>
import { ref, onMounted } from "vue";
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

function navigateToSeries(libraryKey, libraryTitle) {
  router.push({
    name: "series",
    params: { key: libraryKey },
    query: { libraryTitle },
  });
}
</script>

<template>
  <div class="libraries-view" data-enter>
    <header class="page-header" style="margin-bottom: var(--s-6);">
      <h1 class="t-h2" style="font: var(--font-display); font-weight: var(--display-weight); letter-spacing: var(--display-tracking); margin-bottom: var(--s-2);">{{ t("libraries.title") }}</h1>
      <p class="t-body muted" style="margin: 0;">{{ t("libraries.subtitle") }}</p>
      <div style="margin-top: var(--s-4);">
        <button class="btn btn--secondary" :disabled="loading" @click="loadLibraries">
          <AppIcon :name="loading ? 'spinner' : 'refresh-cw'" size="sm" :class="{ 'animate-spin': loading }" />
          {{ t("common.refresh") }}
        </button>
      </div>
    </header>

    <div v-if="error" class="alert alert--error" role="alert" style="margin-bottom: var(--s-6); display: flex; align-items: center; gap: var(--s-3);">
      <AppIcon name="triangle-alert" size="lg" />
      {{ error }}
    </div>

    <div v-if="loading" class="library-grid" style="display: grid; grid-template-columns: 1fr; gap: var(--s-4);">
      <div class="skeleton-card" style="height: 220px;"></div>
      <div class="skeleton-card" style="height: 220px;"></div>
      <div class="skeleton-card" style="height: 220px;"></div>
    </div>

    <div v-else-if="libraries.length" class="library-grid" style="display: grid; grid-template-columns: 1fr; gap: var(--s-4);">
      <button
        v-for="lib in libraries"
        :key="lib.key"
        type="button"
        data-testid="library-card"
        class="library-card"
        @click="navigateToSeries(lib.key, lib.title)"
      >
        <LibraryMosaic :library-key="lib.key" />
        <h3 class="card__title">{{ lib.title }}</h3>
        <p class="card__meta">{{ t("series.seriesCount", { count: lib.seriesCount || 0 }) }}</p>
      </button>
    </div>

    <div v-else class="card panel empty-state" style="--p-h: auto; padding: var(--s-8); text-align: center;">
      <AppIcon name="tv" size="xl" style="margin: 0 auto var(--s-4); color: var(--muted);" />
      <p class="t-body muted" style="margin: 0;">{{ t("libraries.noLibraries") }}</p>
    </div>
  </div>
</template>

<style scoped>
.libraries-view {
  width: 100%;
}

.page-header {
  display: flex;
  flex-direction: column;
  gap: var(--s-4);
  margin-bottom: var(--s-6);
}

@media (min-width: 640px) {
  .page-header {
    flex-direction: row;
    align-items: center;
    justify-content: space-between;
  }
}

.library-grid {
  display: grid;
  grid-template-columns: 1fr;
  gap: var(--s-4);
}

@media (min-width: 640px) {
  .library-grid {
    grid-template-columns: repeat(2, 1fr);
  }
}

@media (min-width: 1024px) {
  .library-grid {
    grid-template-columns: repeat(3, 1fr);
  }
}

.library-card {
  display: flex;
  flex-direction: column;
  padding: var(--s-4);
  background: var(--card);
  border: 1px solid var(--border);
  border-radius: var(--r-lg);
  cursor: pointer;
  transition: border-color var(--m-fast) var(--m-ease), box-shadow var(--m-fast) var(--m-ease);
  text-align: left;
}

@media (hover: hover) and (pointer: fine) {
  .library-card:hover {
    border-color: color-mix(in oklab, var(--action) 50%, transparent);
    box-shadow: var(--shadow-md);
  }
}

.library-card:focus-visible {
  outline: 2px solid var(--action);
  outline-offset: 2px;
}

.card__title {
  margin: var(--s-4) 0 var(--s-2);
  font: var(--font-display);
  font-size: var(--t-h3);
  font-weight: var(--display-weight);
  color: var(--ink);
  line-height: 1.2;
}

.card__meta {
  margin-top: auto;
  font-size: var(--t-small);
  font-weight: 500;
  color: var(--muted);
  display: flex;
  align-items: center;
  gap: var(--s-1);
}

.card__meta::after {
  content: "›";
  display: inline-block;
  transition: transform var(--m-fast) var(--m-ease);
}

@media (hover: hover) and (pointer: fine) {
  .library-card:hover .card__meta::after {
    transform: translateX(4px);
  }
}

.empty-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: var(--s-3);
}

.skeleton-card {
  background: linear-gradient(90deg, var(--surface) 25%, var(--border) 50%, var(--surface) 75%);
  background-size: 200% 100%;
  animation: skeleton-loading 1.5s infinite;
  border-radius: var(--r-lg);
  border: 1px solid var(--border);
}

@keyframes skeleton-loading {
  0% { background-position: 200% 0; }
  100% { background-position: -200% 0; }
}

.alert {
  display: flex;
  align-items: center;
  gap: var(--s-3);
  padding: var(--s-3) var(--s-4);
  border-radius: var(--r-md);
  font-size: var(--t-body);
  line-height: 1.5;
}

.alert--error {
  background: color-mix(in oklab, var(--danger, #e03e3e) 10%, transparent);
  border: 1px solid color-mix(in oklab, var(--danger, #e03e3e) 25%, transparent);
  color: var(--ink);
}
</style>