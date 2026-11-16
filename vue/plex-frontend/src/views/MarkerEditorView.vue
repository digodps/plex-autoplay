<script setup>
import { ref, computed, onMounted } from "vue";
import { useRoute, useRouter } from "vue-router";
import { useI18n } from "vue-i18n";
import { useMarkerStore } from "@/stores/markers";
import { apiFetch } from "@/api";
import MarkerEditorRow from "@/components/MarkerEditorRow.vue";

const { t } = useI18n();
const route = useRoute();
const router = useRouter();
const store = useMarkerStore();
const reprocessing = ref(false);

const seriesKey = computed(() => route.params.seriesKey as string);

async function handleReprocess() {
  if (!seriesKey.value) return;
  reprocessing.value = true;
  try {
    const res = await apiFetch("/api/markers/reprocess", {
      method: "POST",
      body: JSON.stringify({ seriesKey: seriesKey.value }),
    });
    if (!res.ok) throw new Error(t("markers.reprocessFailed"));
    await res.json();
    await store.fetchSeries(seriesKey.value);
  } catch (e) {
    store.error.value = e.message || t("markers.reprocessFailed");
  } finally {
    reprocessing.value = false;
  }
}

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



onMounted(() => {
  store.fetchSeries(seriesKey.value);
});
</script>

<template>
  <div class="marker-editor-view">
    <!-- HERO: product, 1 screen, data-enter -->
    <section class="hero hero--product" data-enter aria-labelledby="markers-hero-h">
      <div class="wrap grid hero-text">
        <div class="hero-copy span-6">
          <h1 id="markers-hero-h">{{ store.currentSeries?.title || "Carregando..." }}</h1>
          <p class="t-lead">Editor de marcadores de intro e créditos por temporada e episódio.</p>
          <div class="btn-row">
            <button class="btn btn--secondary" data-testid="markers-btn-back" @click="router.back()">
              <img src="/assets/icons/ui-chevron-right.svg" data-icon="ui-chevron-right" class="icon" width="20" height="20" aria-hidden="true" style="transform: scaleX(-1);" />
              Voltar às Séries
            </button>
            <button
              class="btn btn--secondary"
              :disabled="reprocessing"
              data-testid="markers-btn-reprocess"
              @click="handleReprocess"
            >
              <img src="/assets/icons/ui-refresh-cw.svg" data-icon="ui-refresh-cw" class="icon" width="20" height="20" aria-hidden="true" />
              {{ reprocessing ? "Reprocessando..." : "Reprocessar Todos" }}
            </button>
          </div>
        </div>
        <div class="hero-visual span-6 start-7" aria-hidden="true">
          <div class="panel frame panel--device" data-fx="marker-editor">
            <div class="panel__bar">
              <span class="muted">{{ store.currentSeries?.title || "Editor de Marcadores" }}</span>
              <span class="sp"></span>
              <span v-if="store.loading" class="panel__status text-warning">Carregando...</span>
              <span v-else-if="!store.error" class="panel__status text-success">Pronto</span>
              <span v-else class="panel__status text-danger">Erro</span>
            </div>
            <div class="panel__screen">
              <div class="stage at-corner" data-ui="marker-timeline" data-seed="markers"></div>
            </div>
          </div>
        </div>
      </div>
    </section>

    <!-- SECTION: Editor de Marcadores (Dealt Moment: picture-develops) -->
    <section class="section g2" aria-labelledby="markers-editor-h">
      <div class="wrap grid">
        <h2 id="markers-editor-h" class="t-h2">Editor de Marcadores</h2>

        <div v-if="store.error" class="alert" role="alert" style="grid-column: 1 / -1;">
          <img src="/assets/icons/ui-triangle-alert.svg" data-icon="ui-triangle-alert" class="icon" width="20" height="20" aria-hidden="true" />
          <span>{{ store.error }}</span>
          <button class="btn btn--secondary btn-sm" @click="store.fetchSeries(seriesKey)">
            <img src="/assets/icons/ui-refresh-cw.svg" data-icon="ui-refresh-cw" class="icon" width="16" height="16" aria-hidden="true" />
            Tentar novamente
          </button>
        </div>

        <div v-else-if="store.loading" class="editor-skeleton" style="grid-column: 1 / -1;">
          <div v-for="n in 3" :key="n" class="skeleton group-line" />
          <div class="skeleton row-line" />
          <div class="skeleton row-line" />
          <div class="skeleton row-line" />
        </div>

        <div v-else class="seasons" style="grid-column: 1 / -1;">
          <div class="moment moment--picture-develops" style="margin-bottom: var(--s-8);" data-moment="picture-develops" aria-label="Visualização de marcadores se desenvolvendo no timeline">
            <div class="stage at-corner" data-ui="marker-develops" :data-series="store.currentSeries?.title"></div>
          </div>

          <section v-for="{ season, list } in seasons" :key="season" class="season" style="margin-bottom: var(--s-10);">
            <div style="display: flex; align-items: center; justify-content: space-between; margin-bottom: var(--s-4);">
              <h3 class="season-title">Temporada {{ season }}</h3>
              <span class="t-small muted">{{ list.length }} episódio{{ list.length !== 1 ? 's' : '' }}</span>
            </div>

            <div class="episodes-list">
              <article v-for="ep in list" :key="ep.rating_key" class="episode">
                <div class="episode-head">
                  <span class="episode-number">T{{ ep.season }} · E{{ ep.episode }}</span>
                  <h4 class="episode-title">{{ ep.title }}</h4>
                </div>

                <div class="marker-rows">
                  <div v-for="kind in ['intro', 'credits']" :key="kind" class="marker-editor-row">
                    <MarkerEditorRow
                      :rating-key="ep.rating_key"
                      :kind="kind"
                      :episode="ep"
                    />
                  </div>
                </div>
              </article>
            </div>
          </section>

          <div v-if="seasons.length === 0" class="empty-state" style="grid-column: 1 / -1; text-align: center; padding: var(--s-10);">
            <img src="/assets/icons/ui-tv.svg" data-icon="ui-tv" class="icon icon-lg" width="64" height="64" aria-hidden="true" />
            <p>Nenhum episódio nesta série.</p>
          </div>
        </div>
      </div>
    </section>
  </div>
</template>

<style scoped>
.marker-editor-view {
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

.icon {
  display: inline-block;
  vertical-align: middle;
  flex-shrink: 0;
}

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

/* Skeleton */
.skeleton {
  background: linear-gradient(90deg, var(--card) 25%, var(--surface) 50%, var(--card) 75%);
  background-size: 200% 100%;
  animation: shimmer 1.5s infinite;
}
@keyframes shimmer {
  0% { background-position: 200% 0; }
  100% { background-position: -200% 0; }
}
.group-line { height: 24px; border-radius: var(--r-sm); }
.row-line { height: 48px; border-radius: var(--r-sm); margin-top: var(--s-4); }

/* Seasons */
.seasons {
  display: flex;
  flex-direction: column;
  gap: var(--s-8);
}

.season-title {
  margin: 0;
  font-family: var(--font-display);
  font-size: var(--t-h3);
  font-weight: 600;
  letter-spacing: var(--display-tracking);
  color: var(--ink);
}

.episodes-list {
  display: flex;
  flex-direction: column;
  gap: var(--s-4);
}

.episode {
  border: 1px solid var(--border);
  border-radius: var(--r-md);
  padding: var(--s-4);
  background: var(--surface);
  transition: border-color var(--dur) var(--ease), box-shadow var(--dur) var(--ease);
}
@media (hover: hover) and (pointer: fine) {
  .episode:hover {
    border-color: color-mix(in oklab, var(--ink) 16%, transparent);
    box-shadow: 0 4px 16px -8px oklch(from var(--card, #fff) 0 0 0 / calc(0.75 - l * 0.55));
  }
}

.episode-head {
  display: flex;
  align-items: baseline;
  gap: var(--s-3);
  margin-bottom: var(--s-3);
  flex-wrap: wrap;
}

.episode-number {
  font: var(--font-mono);
  font-size: var(--t-small);
  font-weight: 500;
  color: var(--muted);
  background: var(--bg);
  padding: var(--s-1) var(--s-2);
  border-radius: var(--r-sm);
}

.episode-title {
  margin: 0;
  font-family: var(--font-text);
  font-size: var(--t-card);
  font-weight: 560;
  letter-spacing: -0.012em;
  color: var(--ink);
}

.marker-rows {
  display: flex;
  flex-direction: column;
  gap: var(--s-3);
}

.marker-editor-row {
  display: grid;
  grid-template-columns: 120px 1fr;
  gap: var(--s-4);
  align-items: start;
}

.marker-editor-row .marker-label {
  font-size: var(--t-small);
  font-weight: 500;
  color: var(--muted);
  text-transform: capitalize;
}

/* Moment */
.moment--picture-develops {
  border-radius: var(--r-lg);
  overflow: hidden;
  background: var(--surface);
  border: 1px solid var(--border);
}
.moment--picture-develops .stage {
  aspect-ratio: 16 / 9;
  min-height: 200px;
}

/* Card loops animation */
@media (prefers-reduced-motion: no-preference) {
  .hero .panel,
  .section .panel {
    animation: card-loop 3.2s ease-in-out infinite;
  }
  .hero .panel { animation-delay: 0s; }
  .section .panel { animation-delay: 0.8s; }
}

@keyframes card-loop {
  0%, 100% { opacity: 1; transform: translateY(0); }
  50% { opacity: 0.9; transform: translateY(-4px); }
}

.hero .panel:hover,
.section .panel:hover {
  animation-play-state: paused;
}
</style>