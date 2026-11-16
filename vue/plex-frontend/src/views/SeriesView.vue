<script setup>
import { ref, computed } from "vue";
import { useRoute, useRouter } from "vue-router";
import { useI18n } from "vue-i18n";

import { useSeriesCatalog } from "../composables/useSeriesCatalog";
import { useSeriesByGenre } from "../composables/useSeriesByGenre";

const { t } = useI18n();
const route = useRoute();
const router = useRouter();

const libraryKey = computed(() => route.params.key);
const groupByGenre = ref(false);

function openMarkers(series) {
  router.push({ name: "series-markers", params: { seriesKey: series.ratingKey } });
}

const catalog = useSeriesCatalog({ libraryKey: libraryKey.value });
const byGenre = useSeriesByGenre({ libraryKey: libraryKey.value });

const libraryTitle = computed(() => {
  return libraryKey.value ? `Biblioteca ${libraryKey.value}` : "Biblioteca";
});

const totalSeriesByGenre = computed(() => {
  return byGenre.genreGroups.value.reduce((sum, group) => sum + group.series.length, 0);
});


</script>

<template>
  <div class="series-view">
    <!-- HERO: photograph-led, 1 screen, data-enter -->
    <section class="hero hero--split" data-enter aria-labelledby="series-hero-h">
      <img class="layer" src="/assets/layout/img/photo-465.jpg" alt="" aria-hidden="true" />
      <div class="wrap grid hero-text hero-split">
        <div class="hero-copy span-6">
          <h1 id="series-hero-h">{{ libraryTitle }}</h1>
          <p class="t-lead">{{ catalog.seriesTotalSize }} série{{ catalog.seriesTotalSize !== 1 ? 's' : '' }} encontrada{{ catalog.seriesTotalSize !== 1 ? 's' : '' }}. Clique para gerenciar marcadores de intro/créditos.</p>
          <div class="btn-row">
            <button class="btn btn--secondary" :disabled="catalog.seriesLoading" data-testid="btn-refresh-series" @click="catalog.refresh">
              <img src="/assets/icons/ui-refresh-cw.svg" data-icon="ui-refresh-cw" class="icon" width="20" height="20" aria-hidden="true" />
              Atualizar
            </button>
          </div>
        </div>
        <div class="hero-visual span-6 start-7" aria-hidden="true">
          <div class="panel frame" data-fx="mesh">
            <div class="stage at-corner" data-ui="series-mosaic" data-seed="series"></div>
          </div>
        </div>
      </div>
    </section>

    <!-- SECTION: Catálogo de Séries -->
    <section class="section" aria-labelledby="series-h" style="--h: 2">
      <div class="wrap grid">
        <div style="grid-column: 1 / -1; display: flex; align-items: center; justify-content: space-between; flex-wrap: wrap; gap: var(--s-3); margin-bottom: var(--s-4);">
          <h2 id="series-h" class="t-h2">Catálogo de Séries</h2>
          <div style="display: flex; align-items: center; gap: var(--s-4);">
            <span v-if="!catalog.seriesLoading && !groupByGenre" class="count t-small muted">{{ catalog.seriesTotalSize }} série{{ catalog.seriesTotalSize !== 1 ? 's' : '' }}</span>
            <span v-if="!byGenre.isLoading && groupByGenre" class="count t-small muted">{{ totalSeriesByGenre }} série{{ totalSeriesByGenre !== 1 ? 's' : '' }}</span>
            <div class="toggle-group">
              <label class="m-switch" role="switch" :aria-checked="groupByGenre" :aria-label="t('series.groupByGenre')" @click="groupByGenre = !groupByGenre">
                <span class="visually-hidden">{{ t("series.groupByGenre") }}</span>
              </label>
              <span class="t-small">{{ t("series.groupByGenre") }}</span>
            </div>
          </div>
        </div>

        <!-- Search (only in flat list mode) -->
        <div v-if="!groupByGenre" class="search-box" style="grid-column: 1 / -1; margin-bottom: var(--s-6);">
          <img src="/assets/icons/ui-search.svg" data-icon="ui-search" class="icon" width="20" height="20" aria-hidden="true" />
          <input
            v-model="catalog.seriesQuery"
            class="input"
            type="search"
            placeholder="Buscar série..."
            aria-label="Buscar série"
          />
        </div>

        <!-- Error State -->
        <div v-if="!groupByGenre && catalog.seriesError" class="alert" role="alert" style="grid-column: 1 / -1;">
          <img src="/assets/icons/ui-triangle-alert.svg" data-icon="ui-triangle-alert" class="icon" width="20" height="20" aria-hidden="true" />
          <span>{{ catalog.seriesError }}</span>
          <button class="btn btn--secondary btn-sm" @click="catalog.refresh">
            <img src="/assets/icons/ui-refresh-cw.svg" data-icon="ui-refresh-cw" class="icon" width="16" height="16" aria-hidden="true" />
            Tentar novamente
          </button>
        </div>
        <div v-if="groupByGenre && byGenre.error" class="alert" role="alert" style="grid-column: 1 / -1;">
          <img src="/assets/icons/ui-triangle-alert.svg" data-icon="ui-triangle-alert" class="icon" width="20" height="20" aria-hidden="true" />
          <span>{{ byGenre.error }}</span>
          <button class="btn btn--secondary btn-sm" @click="byGenre.refresh">
            <img src="/assets/icons/ui-refresh-cw.svg" data-icon="ui-refresh-cw" class="icon" width="16" height="16" aria-hidden="true" />
            Tentar novamente
          </button>
        </div>

        <!-- Flat List Mode -->
        <div v-else-if="!groupByGenre" class="series-list-wrapper" style="grid-column: 1 / -1;">

          <!-- Loading Skeletons -->
          <div v-if="catalog.seriesLoading && catalog.seriesList.length === 0" class="series-list" role="list" aria-label="Carregando séries">
            <div v-for="n in 12" :key="n" class="series-item skeleton" role="listitem">
              <div class="series-item__panel skeleton" style="--p-h: 120px;"></div>
            </div>
          </div>

          <!-- Series List -->
          <div v-else-if="catalog.seriesList.length > 0" class="series-list" role="list" aria-label="Séries">
            <article
              v-for="series in catalog.seriesList"
              :key="series.ratingKey"
              class="series-item"
              role="listitem"
              tabindex="0"
              @click="openMarkers(series)"
              @keydown.enter="openMarkers(series)"
              @keydown.space.prevent="openMarkers(series)"
            >
              <div class="series-item__panel" style="--p-h: 120px;">
                <div v-if="series.thumb" class="stage at-corner" style="background-image: url('{{ series.thumb }}'); background-size: cover; background-position: center;"></div>
                <div v-else class="stage at-corner" data-ui="series-placeholder"></div>
              </div>
              <div class="series-item__content">
                <h3 class="series-item__title">{{ series.title }}</h3>
                <p v-if="series.summary" class="series-item__summary">{{ series.summary }}</p>
                <div class="series-item__meta">
                  <span v-if="series.genre" class="badge badge--genre">{{ series.genre }}</span>
                  <span v-if="series.year" class="t-small muted">{{ series.year }}</span>
                </div>
              </div>
              <a class="series-item__link" @click.stop="openMarkers(series)">
                <span>{{ t("series.manageMarkers") }}</span>
                <img src="/assets/icons/ui-chevron-right.svg" data-icon="ui-chevron-right" class="icon" width="14" height="14" aria-hidden="true" />
              </a>
            </article>

            <!-- Loading skeletons for next page -->
            <template v-if="catalog.isLoadingMore">
              <div v-for="n in 6" :key="'more-' + n" class="series-item skeleton" role="listitem">
                <div class="series-item__panel skeleton" style="--p-h: 120px;"></div>
              </div>
            </template>

            <!-- Sentinel for IntersectionObserver -->
            <div ref="catalog.sentinelRef" class="sentinel" aria-hidden="true"></div>
          </div>

          <!-- End of catalog indicator -->
          <div v-if="!catalog.hasMore && catalog.seriesList.length > 0" class="end-of-catalog" style="text-align: center; padding: var(--s-6); color: var(--muted);">
            <span>Fim do catálogo</span>
          </div>

          <!-- Empty State -->
          <div v-else-if="!catalog.seriesLoading && !catalog.seriesError" class="empty-state" style="text-align: center; padding: var(--s-10);">
            <img src="/assets/icons/ui-tv.svg" data-icon="ui-tv" class="icon icon-lg" width="64" height="64" aria-hidden="true" />
            <p>Nenhuma série encontrada.</p>
            <p class="t-small muted">Tente ajustar a busca ou verifique a biblioteca no Plex.</p>
          </div>
        </div>

        <!-- Grouped by Genre Mode -->
        <div v-else class="genres-list-wrapper" style="grid-column: 1 / -1;">
          <div v-if="byGenre.isLoading && byGenre.genreGroups.length === 0" class="genres-loading" role="list" aria-label="Carregando séries">
            <div v-for="n in 4" :key="n" class="genre-section-skeleton">
              <div class="genre-header-skeleton" />
              <div class="genre-items-skeleton">
                <div v-for="i in 4" :key="i" class="series-item skeleton">
                  <div class="series-item__panel skeleton" style="--p-h: 120px;"></div>
                </div>
              </div>
            </div>
          </div>

          <div v-else-if="byGenre.genreGroups.length > 0" class="genres-list" role="list" aria-label="Séries por gênero">
            <section
              v-for="group in byGenre.genreGroups"
              :key="group.genre"
              class="genre-section"
              role="listitem"
            >
              <header class="genre-header">
                <h3 class="genre-title">{{ group.genre }}</h3>
                <span class="genre-count t-small muted">{{ group.series.length }} série{{ group.series.length !== 1 ? 's' : '' }}</span>
              </header>
              <div class="genre-items" role="list" :aria-label="`Séries de ${group.genre}`">
                <article
                  v-for="series in group.series"
                  :key="series.ratingKey"
                  class="series-item"
                  role="listitem"
                  tabindex="0"
                  @click="openMarkers(series)"
                  @keydown.enter="openMarkers(series)"
                  @keydown.space.prevent="openMarkers(series)"
                >
                  <div class="series-item__panel" style="--p-h: 120px;">
                    <div v-if="series.thumb" class="stage at-corner" style="background-image: url('{{ series.thumb }}'); background-size: cover; background-position: center;"></div>
                    <div v-else class="stage at-corner" data-ui="series-placeholder"></div>
                  </div>
                  <div class="series-item__content">
                    <h4 class="series-item__title">{{ series.title }}</h4>
                    <p v-if="series.summary" class="series-item__summary">{{ series.summary }}</p>
                    <span v-if="series.year" class="t-small muted">{{ series.year }}</span>
                  </div>
                  <a class="series-item__link" @click.stop="openMarkers(series)">
                    <span>{{ t("series.manageMarkers") }}</span>
                    <img src="/assets/icons/ui-chevron-right.svg" data-icon="ui-chevron-right" class="icon" width="14" height="14" aria-hidden="true" />
                  </a>
                </article>
              </div>
            </section>

            <!-- Load more sentinel -->
            <div v-if="byGenre.hasMore" ref="byGenre.sentinelRef" class="sentinel" aria-hidden="true"></div>
          </div>

          <div v-else-if="!byGenre.isLoading && !byGenre.error" class="empty-state" style="text-align: center; padding: var(--s-10);">
            <img src="/assets/icons/ui-tv.svg" data-icon="ui-tv" class="icon icon-lg" width="64" height="64" aria-hidden="true" />
            <p>Nenhuma série encontrada.</p>
            <p class="t-small muted">Verifique a biblioteca no Plex.</p>
          </div>
        </div>
      </div>
    </section>
  </div>
</template>

<style scoped>
.series-view {
  /* Page wrapper */
}

.hero {
  position: relative;
  min-height: calc(100vh - var(--nav-h, 64px));
  display: flex;
  align-items: center;
}

.hero-text {
  display: grid;
  grid-template-columns: repeat(12, minmax(0, 1fr));
  column-gap: var(--gutter);
  align-items: center;
  height: 100%;
}

.hero-copy {
  display: flex;
  flex-direction: column;
  justify-content: center;
}

.hero-copy h1 {
  font: var(--font-display);
  font-size: var(--t-h1);
  font-weight: var(--display-weight);
  line-height: 1.1;
  letter-spacing: var(--display-tracking);
  margin: 0 0 var(--s-4);
  color: var(--ink);
}

.hero-copy .t-lead {
  font-size: var(--t-lead);
  line-height: 1.6;
  color: var(--muted);
  margin: 0 0 var(--s-6);
  max-width: 60ch;
}

.btn-row {
  display: flex;
  gap: var(--s-3);
  flex-wrap: wrap;
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
  transition: background var(--dur) var(--ease), border-color var(--dur) var(--ease), color var(--dur) var(--ease), transform var(--dur) var(--ease), box-shadow var(--dur) var(--ease);
}
.btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}
.btn:focus-visible {
  outline: 2px solid var(--action);
  outline-offset: 2px;
}

.btn--primary {
  background: var(--p-action);
  color: var(--p-action-ink);
  border-color: var(--p-action);
}
@media (hover: hover) and (pointer: fine) {
  .btn--primary:hover:not(:disabled) {
    background: color-mix(in oklab, var(--p-action) 84%, var(--bg));
    transform: translateY(-1px);
  }
}
.btn--primary:active:not(:disabled) {
  transform: translateY(1px) scale(0.985);
}

.btn--secondary {
  background: var(--surface);
  color: var(--ink);
  border-color: var(--border);
}
@media (hover: hover) and (pointer: fine) {
  .btn--secondary:hover:not(:disabled) {
    background: color-mix(in oklab, var(--bg) 92%, var(--ink));
    border-color: color-mix(in oklab, var(--ink) 32%, transparent);
    transform: translateY(-1px);
  }
}
.btn--secondary:active:not(:disabled) {
  transform: translateY(1px) scale(0.985);
}

.btn-sm {
  padding: var(--s-1) var(--s-3);
  font-size: var(--t-small);
}

.icon {
  display: inline-block;
  vertical-align: middle;
  flex-shrink: 0;
}
.icon-lg { width: 64px; height: 64px; }

.hero-visual {
  display: flex;
  align-items: center;
  justify-content: center;
}

.frame {
  border-radius: var(--r-lg);
  overflow: hidden;
  background: var(--surface);
  border: 1px solid var(--border);
}

.section {
  padding: var(--padt) 0 calc(var(--padb) + var(--bar));
}

.t-h2 {
  font: var(--font-display);
  font-size: var(--t-h2);
  font-weight: 600;
  letter-spacing: var(--display-tracking);
  margin: 0 0 var(--s-6);
  color: var(--ink);
}

/* Search box */
.search-box {
  position: relative;
  max-width: 400px;
}
.search-box .icon {
  position: absolute;
  left: var(--s-3);
  top: 50%;
  transform: translateY(-50%);
  opacity: 0.5;
  pointer-events: none;
}
.search-box .input {
  width: 100%;
  padding: var(--s-2) var(--s-3) var(--s-2) var(--s-10);
  font: var(--font-text);
  font-size: var(--t-body);
  color: var(--ink);
  background: var(--surface);
  border: 1px solid var(--border);
  border-radius: var(--r-md);
  transition: border-color var(--dur) var(--ease), box-shadow var(--dur) var(--ease);
}
.search-box .input:focus {
  outline: none;
  border-color: var(--action);
  box-shadow: 0 0 0 1px var(--action), 0 0 0 4px color-mix(in oklab, var(--action) 20%, transparent);
}

/* Alert */
.alert {
  display: flex;
  align-items: center;
  gap: var(--s-3);
  padding: var(--s-4) var(--s-5);
  border-radius: var(--r-md);
  background: color-mix(in oklab, var(--danger) 8%, transparent);
  border: 1px solid color-mix(in oklab, var(--danger) 25%, transparent);
  color: var(--danger);
  margin-bottom: var(--s-6);
}

/* Empty state */
.empty-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: var(--s-10) var(--s-6);
  text-align: center;
  gap: var(--s-4);
  color: var(--muted);
}
.empty-state .icon-lg {
  opacity: 0.4;
  margin-bottom: var(--s-2);
}

/* Series grid */
.series-grid {
  margin-top: var(--s-6);
}
.series-grid .card {
  cursor: pointer;
  transition: transform var(--m-out) var(--m-ease), border-color var(--m-out) var(--m-ease), box-shadow var(--m-out) var(--m-ease);
}
@media (hover: hover) and (pointer: fine) {
  .series-grid .card:hover {
    transform: translateY(var(--m-lift));
    border-color: color-mix(in oklab, var(--ink) 16%, transparent);
    box-shadow: inset 0 1px 0 var(--c-hi, transparent), 0 1px 2px rgb(0 0 0 / .06),
                0 14px 32px -18px oklch(from var(--card, #fff) 0 0 0 / calc(0.75 - l * 0.55));
    transition-duration: 180ms;
  }
}

.series-grid .card .panel {
  position: relative;
  overflow: clip;
}
.series-grid .card .panel .stage {
  position: absolute;
  inset: 0;
  border-radius: var(--p-r) var(--p-r) 0 0;
  background-size: cover;
  background-position: center;
}
.series-grid .card .panel .stage[data-ui="series-placeholder"] {
  background: var(--card);
  display: flex;
  align-items: center;
  justify-content: center;
}
.series-grid .card .panel .stage[data-ui="series-placeholder"]::before {
  content: "";
  width: 48px;
  height: 48px;
  background: url('/assets/icons/ui-tv.svg') center/contain no-repeat;
  opacity: 0.3;
}

.card__title {
  margin: var(--s-4) 0 var(--s-2);
  font-family: var(--font-text);
  font-size: var(--t-card);
  font-weight: 560;
  letter-spacing: -0.012em;
  color: var(--ink);
  text-wrap: balance;
}

.card__text {
  margin: 0 0 var(--s-3);
  font-size: var(--t-body);
  line-height: 1.5;
  color: color-mix(in oklab, var(--ink) 84%, var(--card));
  max-width: 46ch;
  text-wrap: pretty;
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
}

.card__link {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  margin-top: var(--s-3);
  width: max-content;
  font-size: 15px;
  font-weight: 500;
  line-height: 22px;
  color: var(--ink);
  text-decoration: none;
}
.card__link svg { width: 14px; height: 14px; transition: transform 200ms cubic-bezier(.2,.7,.2,1); }
@media (hover: hover) and (pointer: fine) {
  .card__link:hover svg { transform: translateX(2px); }
}

/* Card loops animation */
@media (prefers-reduced-motion: no-preference) {
  .series-grid .panel {
    animation: card-loop 3.2s ease-in-out infinite;
  }
  .series-grid .panel:nth-child(2) { animation-delay: 0.8s; }
  .series-grid .panel:nth-child(3) { animation-delay: 1.6s; }
  .series-grid .panel:nth-child(4) { animation-delay: 2.4s; }
  .series-grid .panel:nth-child(5) { animation-delay: 0.4s; }
  .series-grid .panel:nth-child(6) { animation-delay: 1.2s; }
  .series-grid .panel:nth-child(7) { animation-delay: 2.0s; }
  .series-grid .panel:nth-child(8) { animation-delay: 2.8s; }
  .series-grid .panel:nth-child(9) { animation-delay: 0.6s; }
  .series-grid .panel:nth-child(10) { animation-delay: 1.4s; }
  .series-grid .panel:nth-child(11) { animation-delay: 2.2s; }
  .series-grid .panel:nth-child(12) { animation-delay: 3.0s; }
}

@keyframes card-loop {
  0%, 100% { opacity: 1; transform: translateY(0); }
  50% { opacity: 0.9; transform: translateY(-4px); }
}

.series-grid .panel:hover {
  animation-play-state: paused;
}

/* Skeleton */
.skeleton {
  pointer-events: none;
}
.skeleton .panel {
  background: linear-gradient(90deg, var(--card) 25%, var(--surface) 50%, var(--card) 75%);
  background-size: 200% 100%;
  animation: shimmer 1.5s infinite;
}
@keyframes shimmer {
  0% { background-position: 200% 0; }
  100% { background-position: -200% 0; }
}

/* Toggle group */
.toggle-group {
  display: inline-flex;
  align-items: center;
  gap: var(--s-2);
}

/* Series List (flat) */
.series-list {
  display: flex;
  flex-direction: column;
  gap: var(--s-3);
}

.series-item {
  display: grid;
  grid-template-columns: 160px 1fr auto;
  gap: var(--s-4);
  align-items: center;
  padding: var(--s-3) var(--s-4);
  background: var(--card);
  border: 1px solid var(--border);
  border-radius: var(--r-lg);
  cursor: pointer;
  transition: transform var(--m-out) var(--m-ease), border-color var(--m-out) var(--m-ease), box-shadow var(--m-out) var(--m-ease);
}
@media (hover: hover) and (pointer: fine) {
  .series-item:hover {
    transform: translateY(var(--m-lift));
    border-color: color-mix(in oklab, var(--ink) 16%, transparent);
    box-shadow: inset 0 1px 0 var(--c-hi, transparent), 0 1px 2px rgb(0 0 0 / .06),
                0 14px 32px -18px oklch(from var(--card, #fff) 0 0 0 / calc(0.75 - l * 0.55));
    transition-duration: 180ms;
  }
}
.series-item:focus-visible {
  outline: 2px solid var(--action);
  outline-offset: 2px;
}

.series-item__panel {
  position: relative;
  overflow: clip;
  border-radius: var(--p-r);
}
.series-item__panel .stage {
  position: absolute;
  inset: 0;
  border-radius: var(--p-r);
  background-size: cover;
  background-position: center;
}
.series-item__panel .stage[data-ui="series-placeholder"] {
  background: var(--card);
  display: flex;
  align-items: center;
  justify-content: center;
}
.series-item__panel .stage[data-ui="series-placeholder"]::before {
  content: "";
  width: 32px;
  height: 32px;
  background: url('/assets/icons/ui-tv.svg') center/contain no-repeat;
  opacity: 0.3;
}

.series-item__content {
  display: flex;
  flex-direction: column;
  gap: var(--s-1);
  min-width: 0;
}

.series-item__title {
  margin: 0;
  font-family: var(--font-text);
  font-size: var(--t-card);
  font-weight: 560;
  letter-spacing: -0.012em;
  color: var(--ink);
  text-wrap: balance;
}

.series-item__summary {
  margin: 0;
  font-size: var(--t-body);
  line-height: 1.5;
  color: color-mix(in oklab, var(--ink) 84%, var(--card));
  max-width: 46ch;
  text-wrap: pretty;
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
}

.series-item__meta {
  display: flex;
  align-items: center;
  gap: var(--s-2);
  flex-wrap: wrap;
}

.series-item__link {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  font-size: var(--t-small);
  font-weight: 500;
  color: var(--ink);
  text-decoration: none;
}
.series-item__link svg { width: 14px; height: 14px; transition: transform 200ms cubic-bezier(.2,.7,.2,1); }
@media (hover: hover) and (pointer: fine) {
  .series-item__link:hover svg { transform: translateX(2px); }
}

/* Card loops animation for list items */
@media (prefers-reduced-motion: no-preference) {
  .series-item .series-item__panel {
    animation: card-loop 3.2s ease-in-out infinite;
  }
  .series-item:nth-child(2) .series-item__panel { animation-delay: 0.8s; }
  .series-item:nth-child(3) .series-item__panel { animation-delay: 1.6s; }
  .series-item:nth-child(4) .series-item__panel { animation-delay: 2.4s; }
  .series-item:nth-child(5) .series-item__panel { animation-delay: 0.4s; }
  .series-item:nth-child(6) .series-item__panel { animation-delay: 1.2s; }
  .series-item:nth-child(7) .series-item__panel { animation-delay: 2.0s; }
  .series-item:nth-child(8) .series-item__panel { animation-delay: 2.8s; }
  .series-item:nth-child(9) .series-item__panel { animation-delay: 0.6s; }
  .series-item:nth-child(10) .series-item__panel { animation-delay: 1.4s; }
  .series-item:nth-child(11) .series-item__panel { animation-delay: 2.2s; }
  .series-item:nth-child(12) .series-item__panel { animation-delay: 3.0s; }
}

.series-item:hover .series-item__panel {
  animation-play-state: paused;
}

/* Genres List (grouped) */
.genres-list {
  display: flex;
  flex-direction: column;
  gap: var(--s-8);
}

.genre-section {
  background: var(--card);
  border: 1px solid var(--border);
  border-radius: var(--r-lg);
  overflow: hidden;
}

.genre-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: var(--s-4) var(--s-5);
  background: var(--surface);
  border-bottom: 1px solid var(--border);
}

.genre-title {
  margin: 0;
  font: var(--font-display);
  font-size: var(--t-h3);
  font-weight: 600;
  letter-spacing: var(--display-tracking);
  color: var(--ink);
}

.genre-count {
  color: var(--muted);
}

.genre-items {
  display: flex;
  flex-direction: column;
}

.genre-items .series-item {
  border: none;
  border-bottom: 1px solid var(--border);
  border-radius: 0;
  background: transparent;
}
.genre-items .series-item:last-child {
  border-bottom: none;
}
@media (hover: hover) and (pointer: fine) {
  .genre-items .series-item:hover {
    background: color-mix(in oklab, var(--surface) 50%, transparent);
    transform: none;
    box-shadow: none;
  }
}

/* Loading skeletons for genres */
.genre-section-skeleton {
  background: var(--card);
  border: 1px solid var(--border);
  border-radius: var(--r-lg);
  overflow: hidden;
}
.genre-header-skeleton {
  padding: var(--s-4) var(--s-5);
  background: var(--surface);
  border-bottom: 1px solid var(--border);
  height: 28px;
  background: linear-gradient(90deg, var(--card) 25%, var(--surface) 50%, var(--card) 75%);
  background-size: 200% 100%;
  animation: shimmer 1.5s infinite;
}
.genre-items-skeleton {
  padding: var(--s-3);
}
.genre-items-skeleton .series-item {
  border: none;
  border-bottom: 1px solid var(--border);
  border-radius: 0;
  background: transparent;
}

/* End of catalog */
.end-of-catalog {
  margin-top: var(--s-4);
}

/* Responsive */
@media (max-width: 1023px) {
  .series-item {
    grid-template-columns: 120px 1fr;
    grid-template-rows: auto auto;
    grid-template-areas:
      "panel content"
      "panel link";
  }
  .series-item__link {
    grid-area: link;
    justify-self: start;
    margin-top: var(--s-2);
  }
}

@media (max-width: 639px) {
  .series-item {
    grid-template-columns: 1fr;
    grid-template-rows: auto auto auto;
    grid-template-areas:
      "panel"
      "content"
      "link";
    gap: var(--s-3);
    padding: var(--s-3);
  }
  .series-item__panel {
    aspect-ratio: 16/9;
    max-width: 100%;
  }
  .series-item__link {
    justify-self: stretch;
    justify-content: center;
  }
  .genre-header {
    flex-direction: column;
    align-items: flex-start;
    gap: var(--s-2);
  }
}
</style>