<script setup lang="ts">
import { ref, computed, onMounted, watch } from "vue";
import { useRoute } from "vue-router";
import VideoPlayer from "@/components/Series/VideoPlayer.vue";
import EpisodeList from "@/components/Series/EpisodeList.vue";
import { useSeriesStore } from "@/stores/series";
import { useBreakpoint } from "@/composables/useBreakpoint";
const route = useRoute();

const store = useSeriesStore();
const { isDesktop, isTablet, isMobile } = useBreakpoint();

const seriesKey = computed(() => route.params.seriesKey as string);
const showEpisodeDrawer = ref(false);
const showEpisodeSheet = ref(false);



async function loadSeries() {
  if (!seriesKey.value) return;
  await Promise.all([
    store.fetchEpisodes(seriesKey.value),
    store.fetchAutoPlayPreference(),
  ]);
}

function handlePlayEpisode(episode: typeof store.episodes[0]) {
  store.setCurrentEpisode(episode);
  if (isMobile.value) {
    showEpisodeSheet.value = false;
  } else if (isTablet.value) {
    showEpisodeDrawer.value = false;
  }
}

function closeDrawer() {
  showEpisodeDrawer.value = false;
}

function closeSheet() {
  showEpisodeSheet.value = false;
}

function toggleDrawer() {
  showEpisodeDrawer.value = !showEpisodeDrawer.value;
}

function toggleSheet() {
  showEpisodeSheet.value = !showEpisodeSheet.value;
}

function handleAutoPlayCancelled() {
  // Auto-play was cancelled, do nothing special
}

function handleEpisodeEnded() {
  // Episode ended naturally, auto-play logic handled in VideoPlayer
}

onMounted(() => {
  loadSeries();
});

watch(
  () => route.params.seriesKey,
  () => {
    store.clearEpisodes();
    loadSeries();
  }
);
</script>

<template>
  <div class="series-page">
    <!-- HERO: product (app-frame), 1-2 screens, data-enter -->
    <section class="hero hero--product" data-enter aria-labelledby="series-page-hero-h" style="--h: 2">
      <div class="wrap grid hero-text">
        <div class="hero-copy span-6">
          <h1 id="series-page-hero-h">{{ store.currentSeries?.title || "Carregando..." }}</h1>
          <p v-if="store.currentSeries" class="t-lead">{{ store.currentSeries.seasonCount }} temporada{{ store.currentSeries.seasonCount !== 1 ? 's' : '' }}, {{ store.currentSeries.episodeCount }} episódio{{ store.currentSeries.episodeCount !== 1 ? 's' : '' }}. Player com auto-play e gerenciamento de marcadores.</p>
          <div class="btn-row">
            <button
              class="btn btn--secondary"
              :aria-pressed="store.autoPlayEnabled"
              data-testid="btn-toggle-autoplay"
              @click="store.toggleAutoPlayPreference(!store.autoPlayEnabled)"
            >
              <img :src="store.autoPlayEnabled ? '/assets/icons/ui-play.svg' : '/assets/icons/ui-pause.svg'" :data-icon="store.autoPlayEnabled ? 'ui-play' : 'ui-pause'" class="icon" width="20" height="20" aria-hidden="true" />
              {{ store.autoPlayEnabled ? 'Auto-play ativado' : 'Auto-play desativado' }}
            </button>
          </div>
        </div>
        <div class="hero-visual span-6 start-7" aria-hidden="true">
          <div class="panel frame panel--device" data-fx="player">
            <div class="panel__bar">
              <span class="muted">{{ store.currentSeries?.title || "Plex Autoplay" }}</span>
              <span class="sp"></span>
              <span class="panel__status">
                <i :class="store.autoPlayEnabled ? 'text-success' : 'text-muted'"></i>
                {{ store.autoPlayEnabled ? 'Auto-play ON' : 'Auto-play OFF' }}
              </span>
            </div>
            <div class="panel__screen">
              <div class="stage at-corner" data-ui="video-player" :data-playing="store.autoPlayEnabled"></div>
            </div>
          </div>
        </div>
      </div>
    </section>

    <!-- SECTION: Episódios -->
    <section class="section g2" aria-labelledby="episodes-h">
      <div class="wrap grid">
        <h2 id="episodes-h" class="t-h2">Episódios</h2>

        <!-- Desktop/Tablet Layout -->
        <div v-if="isDesktop" class="series-page__desktop">
          <div class="series-page__player-area">
            <VideoPlayer
              :episode="store.currentEpisode"
              :auto-play-enabled="store.autoPlayEnabled"
              @play="handlePlayEpisode"
              @auto-play-cancelled="handleAutoPlayCancelled"
              @ended="handleEpisodeEnded"
            />
          </div>

          <aside class="series-page__episode-list-area">
            <div class="series-page__episode-list-header">
              <h3 class="series-page__episode-list-title">Lista de Episódios</h3>
              <div class="series-page__episode-list-actions">
                <button
                  class="btn btn--secondary btn-sm"
                  :aria-pressed="store.autoPlayEnabled"
                  @click="store.toggleAutoPlayPreference(!store.autoPlayEnabled)"
                >
                  <img :src="store.autoPlayEnabled ? '/assets/icons/ui-play.svg' : '/assets/icons/ui-pause.svg'" :data-icon="store.autoPlayEnabled ? 'ui-play' : 'ui-pause'" class="icon" width="16" height="16" aria-hidden="true" />
                  <span>{{ store.autoPlayEnabled ? 'Auto-play ON' : 'Auto-play OFF' }}</span>
                </button>
              </div>
            </div>
            <EpisodeList
              :episodes="store.episodes"
              :current-episode="store.currentEpisode"
              @play="handlePlayEpisode"
            />
          </aside>
        </div>

        <!-- Tablet Layout -->
        <div v-else-if="isTablet" class="series-page__tablet">
          <div class="series-page__player-area">
            <VideoPlayer
              :episode="store.currentEpisode"
              :auto-play-enabled="store.autoPlayEnabled"
              @play="handlePlayEpisode"
              @auto-play-cancelled="handleAutoPlayCancelled"
              @ended="handleEpisodeEnded"
            />

            <div class="series-page__tablet-header">
              <h3 class="series-page__episode-list-title">Episódios</h3>
              <div class="series-page__tablet-actions">
                <button
                  class="btn btn--secondary btn-sm"
                  :aria-pressed="store.autoPlayEnabled"
                  @click="store.toggleAutoPlayPreference(!store.autoPlayEnabled)"
                >
                  <img :src="store.autoPlayEnabled ? '/assets/icons/ui-play.svg' : '/assets/icons/ui-pause.svg'" :data-icon="store.autoPlayEnabled ? 'ui-play' : 'ui-pause'" class="icon" width="16" height="16" aria-hidden="true" />
                  <span>{{ store.autoPlayEnabled ? 'Auto-play' : 'Auto-play' }}</span>
                </button>
                <button
                  class="btn btn--secondary btn-sm"
                  :aria-expanded="showEpisodeDrawer"
                  aria-label="Lista de episódios"
                  @click="toggleDrawer"
                >
                  <img src="/assets/icons/ui-chevron-right.svg" data-icon="ui-chevron-right" class="icon" width="16" height="16" aria-hidden="true" />
                  <span>Episódios</span>
                </button>
              </div>
            </div>
          </div>

          <!-- Episode Drawer -->
          <Transition name="drawer">
            <div v-if="showEpisodeDrawer" class="series-page__drawer-overlay" @click="closeDrawer"></div>
          </Transition>

          <Transition name="drawer">
            <aside v-if="showEpisodeDrawer" class="series-page__drawer" role="dialog" aria-label="Episódios" aria-modal="true">
              <div class="series-page__drawer-header">
                <h3>Episódios</h3>
                <button class="btn btn--secondary btn-sm" aria-label="Fechar lista" @click="closeDrawer">
                  <img src="/assets/icons/ui-circle-x.svg" data-icon="ui-circle-x" class="icon" width="16" height="16" aria-hidden="true" />
                  <span>Fechar</span>
                </button>
              </div>
              <EpisodeList
                :episodes="store.episodes"
                :current-episode="store.currentEpisode"
                @play="handlePlayEpisode"
              />
            </aside>
          </Transition>
        </div>

        <!-- Mobile Layout -->
        <div v-else class="series-page__mobile">
          <VideoPlayer
            :episode="store.currentEpisode"
            :auto-play-enabled="store.autoPlayEnabled"
            @play="handlePlayEpisode"
            @auto-play-cancelled="handleAutoPlayCancelled"
            @ended="handleEpisodeEnded"
          />

          <div class="series-page__mobile-header">
            <h3 class="series-page__episode-list-title">Episódios</h3>
            <div class="series-page__mobile-actions">
              <button
                class="btn btn--secondary btn-sm"
                :aria-pressed="store.autoPlayEnabled"
                @click="store.toggleAutoPlayPreference(!store.autoPlayEnabled)"
              >
                <img :src="store.autoPlayEnabled ? '/assets/icons/ui-play.svg' : '/assets/icons/ui-pause.svg'" :data-icon="store.autoPlayEnabled ? 'ui-play' : 'ui-pause'" class="icon" width="16" height="16" aria-hidden="true" />
                <span>{{ store.autoPlayEnabled ? 'Auto-play' : 'Auto-play' }}</span>
              </button>
              <button
                class="btn btn--secondary btn-sm"
                :aria-expanded="showEpisodeSheet"
                aria-label="Lista de episódios"
                @click="toggleSheet"
              >
                <img src="/assets/icons/ui-chevron-right.svg" data-icon="ui-chevron-right" class="icon" width="16" height="16" aria-hidden="true" />
                <span>Episódios</span>
              </button>
            </div>
          </div>

          <!-- Episode Bottom Sheet -->
          <Transition name="sheet">
            <div v-if="showEpisodeSheet" class="series-page__sheet-overlay" @click="closeSheet"></div>
          </Transition>

          <Transition name="sheet">
            <aside v-if="showEpisodeSheet" class="series-page__sheet" role="dialog" aria-label="Episódios" aria-modal="true">
              <div class="series-page__sheet-handle" aria-hidden="true" @click="closeSheet"></div>
              <div class="series-page__sheet-header">
                <h3>Episódios</h3>
                <button class="btn btn--secondary btn-sm" aria-label="Fechar lista" @click="closeSheet">
                  <img src="/assets/icons/ui-circle-x.svg" data-icon="ui-circle-x" class="icon" width="16" height="16" aria-hidden="true" />
                  <span>Fechar</span>
                </button>
              </div>
              <EpisodeList
                :episodes="store.episodes"
                :current-episode="store.currentEpisode"
                @play="handlePlayEpisode"
              />
            </aside>
          </Transition>
        </div>

        <!-- Loading State -->
        <div v-if="store.loading && !store.currentEpisode" class="series-page__loading" style="grid-column: 1 / -1;">
          <div class="series-page__loading-player skeleton" />
          <div class="series-page__loading-list">
            <div v-for="n in 6" :key="n" class="episode-item skeleton" />
          </div>
        </div>

        <!-- Error State -->
        <div v-else-if="store.error && !store.currentEpisode" class="series-page__error" role="alert" style="grid-column: 1 / -1;">
          <img src="/assets/icons/ui-triangle-alert.svg" data-icon="ui-triangle-alert" class="icon icon-lg" width="48" height="48" aria-hidden="true" />
          <p>{{ store.error }}</p>
          <button class="btn btn--primary" @click="loadSeries">
            <img src="/assets/icons/ui-refresh-cw.svg" data-icon="ui-refresh-cw" class="icon" width="20" height="20" aria-hidden="true" />
            Tentar novamente
          </button>
        </div>
      </div>
    </section>

    <!-- SECTION: Marcadores (Dealt Moment: picture-develops) -->
    <section class="section" aria-labelledby="markers-h" style="--h: 2">
      <div class="wrap grid">
        <h2 id="markers-h" class="t-h2">Marcadores de Intro/Créditos</h2>
        
        <div class="moment moment--picture-develops" style="grid-column: 1 / -1; margin-bottom: var(--s-6);" data-moment="picture-develops" aria-label="Visualização de marcadores se desenvolvendo">
          <div class="stage at-corner" data-ui="marker-develops" :data-series="store.currentSeries?.title"></div>
        </div>

        <div style="grid-column: 1 / -1;">
          <router-link :to="{ name: 'series-markers', params: { seriesKey: seriesKey } }" class="btn btn--primary">
            <img src="/assets/icons/ui-square-pen.svg" data-icon="ui-square-pen" class="icon" width="20" height="20" aria-hidden="true" />
            Abrir Editor de Marcadores
          </router-link>
        </div>
      </div>
    </section>
  </div>
</template>

<style scoped>
.series-page {
  display: flex;
  flex-direction: column;
  height: 100%;
  min-height: 100dvh;
  background: var(--bg);
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
.icon-lg { width: 48px; height: 48px; }

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

/* Desktop Layout */
.series-page__desktop {
  display: grid;
  grid-template-columns: 1fr 300px;
  flex: 1;
  gap: var(--s-6);
  padding: 0 0 var(--s-6);
  min-height: 0;
}

.series-page__player-area {
  display: flex;
  flex-direction: column;
  min-width: 0;
}

.series-page__episode-list-area {
  display: flex;
  flex-direction: column;
  min-width: 0;
  max-height: 100%;
}

.series-page__episode-list-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: var(--s-3) 0;
  border-bottom: 1px solid var(--border);
  flex-shrink: 0;
}

.series-page__episode-list-title {
  margin: 0;
  font-size: var(--t-h3);
  font-weight: 600;
  color: var(--ink);
}

.series-page__episode-list-actions {
  display: flex;
  gap: var(--s-2);
}

/* Tablet Layout */
.series-page__tablet {
  display: flex;
  flex-direction: column;
  flex: 1;
  padding: var(--s-4);
  min-height: 0;
}

.series-page__tablet-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: var(--s-3) 0;
  border-bottom: 1px solid var(--border);
  margin-top: var(--s-4);
  flex-shrink: 0;
}

.series-page__tablet-actions {
  display: flex;
  gap: var(--s-2);
}

.series-page__drawer-overlay {
  position: fixed;
  inset: 0;
  z-index: 40;
  background: rgba(0, 0, 0, 0.4);
  backdrop-filter: blur(2px);
}

.series-page__drawer {
  position: fixed;
  top: 0;
  right: 0;
  bottom: 0;
  width: 320px;
  max-width: 85vw;
  z-index: 50;
  display: flex;
  flex-direction: column;
  background: var(--surface);
  border-left: 1px solid var(--border);
  box-shadow: var(--shadow-2);
}

.series-page__drawer-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: var(--s-4);
  border-bottom: 1px solid var(--border);
  flex-shrink: 0;
}

.series-page__drawer-header h3 {
  margin: 0;
  font-size: var(--t-h3);
  font-weight: 600;
}

.drawer-enter-active,
.drawer-leave-active {
  transition: transform var(--m-panel) var(--m-ease), opacity var(--m-panel) var(--m-ease);
}

.drawer-enter-from,
.drawer-leave-to {
  transform: translateX(100%);
  opacity: 0;
}

/* Mobile Layout */
.series-page__mobile {
  display: flex;
  flex-direction: column;
  flex: 1;
  padding: var(--s-4);
  min-height: 0;
}

.series-page__mobile-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: var(--s-3) 0;
  border-bottom: 1px solid var(--border);
  margin-top: var(--s-4);
  flex-shrink: 0;
}

.series-page__mobile-actions {
  display: flex;
  gap: var(--s-2);
}

.series-page__sheet-overlay {
  position: fixed;
  inset: 0;
  z-index: 40;
  background: rgba(0, 0, 0, 0.4);
  backdrop-filter: blur(2px);
}

.series-page__sheet {
  position: fixed;
  bottom: 0;
  left: 0;
  right: 0;
  height: 60%;
  max-height: 70vh;
  z-index: 50;
  display: flex;
  flex-direction: column;
  background: var(--surface);
  border-top-left-radius: var(--r-lg);
  border-top-right-radius: var(--r-lg);
  border-top: 1px solid var(--border);
  box-shadow: var(--shadow-2);
}

.series-page__sheet-handle {
  width: 40px;
  height: 4px;
  margin: var(--s-3) auto 0;
  background: var(--border);
  border-radius: 999px;
  cursor: pointer;
}

.series-page__sheet-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: var(--s-4);
  border-bottom: 1px solid var(--border);
  flex-shrink: 0;
}

.series-page__sheet-header h3 {
  margin: 0;
  font-size: var(--t-h3);
  font-weight: 600;
}

.sheet-enter-active,
.sheet-leave-active {
  transition: transform var(--m-panel) var(--m-ease), opacity var(--m-panel) var(--m-ease);
}

.sheet-enter-from,
.sheet-leave-to {
  transform: translateY(100%);
  opacity: 0;
}

/* Loading State */
.series-page__loading {
  display: flex;
  flex: 1;
  padding: var(--s-6);
  gap: var(--s-6);
}

.series-page__loading-player {
  flex: 1;
  min-width: 0;
  aspect-ratio: 16 / 9;
  border-radius: var(--r-lg);
}

.series-page__loading-list {
  flex: 0 0 300px;
  display: flex;
  flex-direction: column;
  gap: var(--s-3);
}

/* Error State */
.series-page__error {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  flex: 1;
  padding: var(--s-8);
  gap: var(--s-4);
  text-align: center;
  color: var(--danger);
}

.series-page__error :deep(.icon) {
  opacity: 0.6;
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

/* Media queries */
@media (max-width: 1023px) {
  .series-page__desktop {
    display: none;
  }
}

@media (min-width: 768px) {
  .series-page__tablet,
  .series-page__mobile {
    display: none;
  }
}

@media (max-width: 767px) {
  .series-page__tablet {
    display: none;
  }
}
</style>