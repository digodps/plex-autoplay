<script setup lang="ts">
import { ref, computed, onMounted, watch, nextTick } from "vue";
import { useRoute, useRouter } from "vue-router";
import Breadcrumb from "@/components/Breadcrumb.vue";
import AppIcon from "@/components/AppIcon.vue";
import VideoPlayer from "@/components/Series/VideoPlayer.vue";
import EpisodeList from "@/components/Series/EpisodeList.vue";
import { useSeriesStore } from "@/stores/series";
import { useBreakpoint, type ViewMode } from "@/composables/useBreakpoint";

const route = useRoute();
const router = useRouter();

const store = useSeriesStore();
const { viewMode, isDesktop, isTablet, isMobile } = useBreakpoint();

const seriesKey = computed(() => route.params.seriesKey as string);
const showEpisodeDrawer = ref(false);
const showEpisodeSheet = ref(false);

const breadcrumbItems = computed(() => [
  { label: "Bibliotecas", to: { name: "libraries" } },
  { label: "Série", current: true },
]);

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
    <!-- Breadcrumb -->
    <Breadcrumb :items="breadcrumbItems" class="series-page__breadcrumb" />

    <!-- Desktop Layout -->
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
          <h2 class="series-page__episode-list-title">Episódios</h2>
          <div class="series-page__episode-list-actions">
            <button
              class="btn btn-ghost btn-sm"
              @click="store.toggleAutoPlayPreference(!store.autoPlayEnabled)"
              :aria-pressed="store.autoPlayEnabled"
            >
              <AppIcon :name="store.autoPlayEnabled ? 'play-circle' : 'pause-circle'" size="sm" />
              <span>{{ store.autoPlayEnabled ? "Auto-play ativado" : "Auto-play desativado" }}</span>
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
          <h2 class="series-page__episode-list-title">Episódios</h2>
          <div class="series-page__tablet-actions">
            <button
              class="btn btn-ghost btn-sm"
              @click="store.toggleAutoPlayPreference(!store.autoPlayEnabled)"
              :aria-pressed="store.autoPlayEnabled"
            >
              <AppIcon :name="store.autoPlayEnabled ? 'play-circle' : 'pause-circle'" size="sm" />
            </button>
            <button
              class="btn btn-ghost btn-sm"
              @click="toggleDrawer"
              :aria-expanded="showEpisodeDrawer"
              aria-label="Lista de episódios"
            >
              <AppIcon name="list" size="sm" />
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
            <button class="btn btn-ghost btn-sm" @click="closeDrawer" aria-label="Fechar lista">
              <AppIcon name="x" size="sm" />
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
        <h2 class="series-page__episode-list-title">Episódios</h2>
        <div class="series-page__mobile-actions">
          <button
            class="btn btn-ghost btn-sm"
            @click="store.toggleAutoPlayPreference(!store.autoPlayEnabled)"
            :aria-pressed="store.autoPlayEnabled"
          >
            <AppIcon :name="store.autoPlayEnabled ? 'play-circle' : 'pause-circle'" size="sm" />
          </button>
          <button
            class="btn btn-ghost btn-sm"
            @click="toggleSheet"
            :aria-expanded="showEpisodeSheet"
            aria-label="Lista de episódios"
          >
            <AppIcon name="list" size="sm" />
          </button>
        </div>
      </div>

      <!-- Episode Bottom Sheet -->
      <Transition name="sheet">
        <div v-if="showEpisodeSheet" class="series-page__sheet-overlay" @click="closeSheet"></div>
      </Transition>

      <Transition name="sheet">
        <aside v-if="showEpisodeSheet" class="series-page__sheet" role="dialog" aria-label="Episódios" aria-modal="true">
          <div class="series-page__sheet-handle" @click="closeSheet" aria-hidden="true"></div>
          <div class="series-page__sheet-header">
            <h3>Episódios</h3>
            <button class="btn btn-ghost btn-sm" @click="closeSheet" aria-label="Fechar lista">
              <AppIcon name="x" size="sm" />
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
    <div v-if="store.loading && !store.currentEpisode" class="series-page__loading">
      <div class="series-page__loading-player skeleton" />
      <div class="series-page__loading-list">
        <div v-for="n in 6" :key="n" class="episode-item skeleton" />
      </div>
    </div>

    <!-- Error State -->
    <div v-else-if="store.error && !store.currentEpisode" class="series-page__error" role="alert">
      <AppIcon name="warning" size="lg" />
      <p>{{ store.error }}</p>
      <button class="btn" @click="loadSeries">Tentar novamente</button>
    </div>
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

.series-page__breadcrumb {
  padding: 1rem 1.5rem;
  flex-shrink: 0;
}

.series-page__loading {
  display: flex;
  flex: 1;
  padding: 1.5rem;
  gap: 1.5rem;
}

.series-page__loading-player {
  flex: 1;
  min-width: 0;
  aspect-ratio: 16 / 9;
  border-radius: var(--radius-lg);
}

.series-page__loading-list {
  flex: 0 0 300px;
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.series-page__error {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  flex: 1;
  padding: 2rem;
  gap: 1rem;
  text-align: center;
  color: var(--danger);
}

.series-page__error :deep(.icon) {
  opacity: 0.6;
}

/* Desktop Layout */
.series-page__desktop {
  display: grid;
  grid-template-columns: 1fr 300px;
  flex: 1;
  gap: 1.5rem;
  padding: 0 1.5rem 1.5rem;
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
  padding: 0.75rem 0;
  border-bottom: 1px solid var(--border);
  flex-shrink: 0;
}

.series-page__episode-list-title {
  margin: 0;
  font-size: 1rem;
  font-weight: 600;
  color: var(--text);
}

.series-page__episode-list-actions {
  display: flex;
  gap: 0.5rem;
}

/* Tablet Layout */
.series-page__tablet {
  display: flex;
  flex-direction: column;
  flex: 1;
  padding: 1rem;
  min-height: 0;
}

.series-page__tablet-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 0.75rem 0;
  border-bottom: 1px solid var(--border);
  margin-top: 1rem;
  flex-shrink: 0;
}

.series-page__tablet-actions {
  display: flex;
  gap: 0.5rem;
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
  padding: 1rem;
  border-bottom: 1px solid var(--border);
  flex-shrink: 0;
}

.series-page__drawer-header h3 {
  margin: 0;
  font-size: 1rem;
  font-weight: 600;
}

.drawer-enter-active,
.drawer-leave-active {
  transition: transform 0.25s var(--ease), opacity 0.25s var(--ease);
}

.drawer-enter-from,
.drawer-leave-to {
  transform: translateX(100%);
  opacity: 0;
}

.drawer-enter-from + .drawer-overlay,
.drawer-leave-to + .drawer-overlay {
  opacity: 0;
}

/* Mobile Layout */
.series-page__mobile {
  display: flex;
  flex-direction: column;
  flex: 1;
  padding: 1rem;
  min-height: 0;
}

.series-page__mobile-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 0.75rem 0;
  border-bottom: 1px solid var(--border);
  margin-top: 1rem;
  flex-shrink: 0;
}

.series-page__mobile-actions {
  display: flex;
  gap: 0.5rem;
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
  border-top-left-radius: var(--radius-lg);
  border-top-right-radius: var(--radius-lg);
  border-top: 1px solid var(--border);
  box-shadow: var(--shadow-2);
}

.series-page__sheet-handle {
  width: 40px;
  height: 4px;
  margin: 0.75rem auto 0;
  background: var(--border);
  border-radius: 999px;
  cursor: pointer;
}

.series-page__sheet-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 1rem;
  border-bottom: 1px solid var(--border);
  flex-shrink: 0;
}

.series-page__sheet-header h3 {
  margin: 0;
  font-size: 1rem;
  font-weight: 600;
}

.sheet-enter-active,
.sheet-leave-active {
  transition: transform 0.3s var(--ease), opacity 0.3s var(--ease);
}

.sheet-enter-from,
.sheet-leave-to {
  transform: translateY(100%);
  opacity: 0;
}

.sheet-enter-from + .sheet-overlay,
.sheet-leave-to + .sheet-overlay {
  opacity: 0;
}

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
