<script setup>
import { computed } from "vue";
import { useRoute, useRouter } from "vue-router";
import { useI18n } from "vue-i18n";
import AppIcon from "@/components/AppIcon.vue";
import Breadcrumb from "@/components/Breadcrumb.vue";
import { useSeriesCatalog } from "@/composables/useSeriesCatalog";

const { t } = useI18n();
const route = useRoute();
const router = useRouter();

const libraryKey = computed(() => route.params.key);

function openSeries(series) {
  router.push({ name: "series-detail", params: { seriesKey: series.ratingKey } });
}

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

const libraryTitle = computed(() => {
  return libraryKey.value ? `Biblioteca ${libraryKey.value}` : t("nav.libraries");
});

const breadcrumbItems = computed(() => [
  { label: t("nav.libraries"), to: { name: "libraries" } },
  { label: libraryTitle.value, current: true },
]);
</script>

<template>
  <div class="series-view" data-enter>
    <!-- Breadcrumb -->
    <Breadcrumb :items="breadcrumbItems" />

    <!-- Toolbar -->
    <div class="series-toolbar" style="display: flex; align-items: center; justify-content: space-between; gap: var(--s-4); margin-bottom: var(--s-4); flex-wrap: wrap;">
      <div class="search-box" style="position: relative; flex: 1; min-width: 280px;">
        <AppIcon name="search" size="sm" style="position: absolute; left: var(--s-3); top: 50%; transform: translateY(-50%); color: var(--muted); pointer-events: none;" />
        <input
          v-model="seriesQuery"
          class="field__input"
          type="search"
          :placeholder="t('series.searchPlaceholder')"
          :aria-label="t('series.searchPlaceholder')"
          style="padding-left: 44px;"
        />
      </div>
      <span v-if="!seriesLoading" class="count t-body muted" style="white-space: nowrap;">{{ seriesTotalSize }} {{ t('series.series') }}</span>
    </div>

    <!-- Error State -->
    <div v-if="seriesError" class="alert alert--error" role="alert" style="margin-bottom: var(--s-4); display: flex; align-items: center; gap: var(--s-3); flex-wrap: wrap;">
      <AppIcon name="triangle-alert" size="sm" />
      <span class="flex-1">{{ seriesError }}</span>
      <button class="btn btn--secondary btn-sm" @click="refresh">{{ t("common.retry") }}</button>
    </div>

    <!-- Loading Skeletons -->
    <div v-else-if="seriesLoading && seriesList.length === 0" class="series-grid" style="display: grid; grid-template-columns: 1fr; gap: var(--s-4);">
      <div v-for="n in 12" :key="n" class="series-card skeleton" style="--p-h: 320px;">
        <div class="series-card-poster skeleton"></div>
        <div class="series-card-info">
          <div class="skeleton-line"></div>
          <div class="skeleton-line short"></div>
        </div>
      </div>
    </div>

    <!-- Series Grid -->
    <div v-else-if="seriesList.length > 0" class="series-grid-wrapper" style="display: flex; flex-direction: column; gap: var(--s-6);">
      <div class="series-grid" style="display: grid; grid-template-columns: 1fr; gap: var(--s-4);">
        <div
          v-for="series in seriesList"
          :key="series.ratingKey"
          class="series-card"
          tabindex="0"
          role="button"
          :aria-label="`${series.title} - ${t('common.open')}`"
          @click="openSeries(series)"
          @keydown.enter="openSeries(series)"
          @keydown.space.prevent="openSeries(series)"
        >
          <div class="series-card-poster">
            <img v-if="series.thumb" :src="series.thumb" :alt="series.title" loading="lazy" style="width: 100%; height: 100%; object-fit: cover; border-radius: var(--r-md) var(--r-md) 0 0;" />
            <AppIcon v-else name="tv" size="lg" style="display: flex; align-items: center; justify-content: center; width: 100%; height: 100%; color: var(--muted);" />
          </div>
          <div class="series-card-info" style="padding: var(--s-3); display: flex; flex-direction: column; gap: var(--s-1); flex: 1;">
            <h4 style="margin: 0; font-size: var(--t-card); font-weight: 500; color: var(--ink); line-height: 1.3; display: -webkit-box; -webkit-line-clamp: 2; -webkit-box-orient: vertical; overflow: hidden;">{{ series.title }}</h4>
            <p v-if="series.summary" class="series-summary" style="margin: 0; font-size: var(--t-small); color: var(--muted); line-height: 1.5; display: -webkit-box; -webkit-line-clamp: 3; -webkit-box-orient: vertical; overflow: hidden;">{{ series.summary }}</p>
          </div>
        </div>

        <!-- Loading skeletons for next page -->
        <template v-if="isLoadingMore">
          <div v-for="n in 6" :key="'more-' + n" class="series-card skeleton" style="--p-h: 320px;">
            <div class="series-card-poster skeleton"></div>
            <div class="series-card-info">
              <div class="skeleton-line"></div>
              <div class="skeleton-line short"></div>
            </div>
          </div>
        </template>

        <!-- Sentinel for IntersectionObserver -->
        <div ref="sentinelRef" class="sentinel" aria-hidden="true" style="height: 20px;"></div>
      </div>

      <!-- End of catalog indicator -->
      <div v-if="!hasMore && seriesList.length > 0" class="end-of-catalog" style="text-align: center; padding: var(--s-6); color: var(--muted);">
        <span>{{ t("series.noSeries") }}</span>
      </div>
    </div>

    <!-- Empty State -->
    <div v-else-if="!seriesLoading && !seriesError" class="empty panel" style="--p-h: 320px; display: flex; flex-direction: column; align-items: center; justify-content: center; text-align: center; padding: var(--s-6); gap: var(--s-4);">
      <AppIcon name="tv" size="xl" style="color: var(--muted);" />
      <p class="t-body muted" style="margin: 0;">{{ t("series.noSeries") }}</p>
    </div>
  </div>
</template>

<style scoped>
.series-view {
  width: 100%;
}

.series-toolbar {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: var(--s-4);
  margin-bottom: var(--s-4);
  flex-wrap: wrap;
}

.search-box {
  position: relative;
  flex: 1;
  min-width: 280px;
}

.count {
  white-space: nowrap;
}

.series-grid {
  display: grid;
  grid-template-columns: 1fr;
  gap: var(--s-4);
}

@media (min-width: 480px) {
  .series-grid {
    grid-template-columns: repeat(2, 1fr);
  }
}

@media (min-width: 768px) {
  .series-grid {
    grid-template-columns: repeat(3, 1fr);
  }
}

@media (min-width: 1200px) {
  .series-grid {
    grid-template-columns: repeat(4, 1fr);
  }
}

.series-card {
  display: flex;
  flex-direction: column;
  background: var(--card);
  border: 1px solid var(--border);
  border-radius: var(--r-lg);
  overflow: hidden;
  cursor: pointer;
  transition: border-color var(--m-fast) var(--m-ease), box-shadow var(--m-fast) var(--m-ease);
}

@media (hover: hover) and (pointer: fine) {
  .series-card:hover {
    border-color: color-mix(in oklab, var(--action) 50%, transparent);
    box-shadow: var(--shadow-md);
  }
}

.series-card:focus-visible {
  outline: 2px solid var(--action);
  outline-offset: 2px;
}

.series-card.skeleton {
  pointer-events: none;
}

.series-card-poster {
  position: relative;
  aspect-ratio: 2/3;
  background: var(--surface);
  overflow: hidden;
}

.series-card-poster.skeleton {
  background: linear-gradient(90deg, var(--surface) 25%, var(--border) 50%, var(--surface) 75%);
  background-size: 200% 100%;
  animation: shimmer 1.5s infinite;
}

@keyframes shimmer {
  0% { background-position: 200% 0; }
  100% { background-position: -200% 0; }
}

.series-card-info {
  padding: var(--s-3);
  display: flex;
  flex-direction: column;
  gap: var(--s-1);
  flex: 1;
}

.series-summary {
  margin: 0;
  font-size: var(--t-small);
  color: var(--muted);
  line-height: 1.5;
  display: -webkit-box;
  -webkit-line-clamp: 3;
  -webkit-box-orient: vertical;
  overflow: hidden;
}

.series-grid-wrapper {
  display: flex;
  flex-direction: column;
  gap: var(--s-6);
}

.sentinel {
  height: 20px;
}

.end-of-catalog {
  text-align: center;
  padding: var(--s-6);
  color: var(--muted);
}

.empty {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  text-align: center;
  padding: var(--s-6);
  gap: var(--s-4);
}

.alert {
  display: flex;
  align-items: center;
  gap: var(--s-3);
  padding: var(--s-3) var(--s-4);
  border-radius: var(--r-md);
  font-size: var(--t-body);
  line-height: 1.5;
  flex-wrap: wrap;
}

.alert--error {
  background: color-mix(in oklab, var(--danger, #e03e3e) 10%, transparent);
  border: 1px solid color-mix(in oklab, var(--danger, #e03e3e) 25%, transparent);
  color: var(--ink);
}

.btn {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  gap: var(--s-2);
  padding: var(--s-2) var(--s-5);
  font: var(--font-text);
  font-size: var(--t-sm);
  font-weight: 500;
  border-radius: var(--r-btn);
  border: 1px solid transparent;
  cursor: pointer;
  transition: transform var(--m-fast) var(--m-ease), background var(--m-fast) var(--m-ease), border-color var(--m-fast) var(--m-ease), box-shadow var(--m-fast) var(--m-ease);
}

.btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.btn--secondary {
  background: var(--surface);
  color: var(--ink);
  border-color: var(--border);
}

@media (hover: hover) and (pointer: fine) {
  .btn--secondary:hover:not(:disabled) {
    background: var(--border);
    transform: translateY(-2px);
    box-shadow: var(--shadow-sm);
  }
  .btn--secondary:active:not(:disabled) {
    transform: scale(0.98);
  }
}

.btn--secondary:focus-visible {
  outline: 2px solid var(--action);
  outline-offset: 2px;
}

.btn-sm {
  padding: var(--s-1) var(--s-3);
  font-size: var(--t-small);
}

.skeleton-line {
  height: 16px;
  background: linear-gradient(90deg, var(--surface) 25%, var(--border) 50%, var(--surface) 75%);
  background-size: 200% 100%;
  animation: shimmer 1.5s infinite;
  border-radius: var(--r-sm);
}

.skeleton-line.short {
  width: 60%;
}

@media (prefers-reduced-motion: reduce) {
  .skeleton-line,
  .series-card-poster.skeleton,
  .skeleton-item {
    animation: none !important;
    background: var(--surface);
  }
}
</style>