<script setup lang="ts">
/* global HTMLVideoElement, KeyboardEvent */
import { ref, computed, onMounted, onUnmounted, watch, nextTick } from "vue";
import AutoPlayCountdown from "./AutoPlayCountdown.vue";
import EpisodeNavigation from "./EpisodeNavigation.vue";
import { useSeriesStore } from "@/stores/series";

interface Props {
  episode: {
    rating_key: string;
    title: string;
    season: number;
    episode: number;
    local_path: string;
    thumb: string | null;
    duration_seconds: number;
  } | null;
  autoPlayEnabled: boolean;
}

const props = defineProps<Props>();
const emit = defineEmits<{ ended: []; autoPlayCancelled: []; play: [episode: typeof props.episode] }>();

const store = useSeriesStore();

const videoRef = ref<HTMLVideoElement | null>(null);
const showControls = ref(true);
const showCountdown = ref(false);
const countdownCancelled = ref(false);
const isPlaying = ref(false);
const isMuted = ref(false);
const currentTime = ref(0);
const duration = ref(0);
const isFullscreen = ref(false);
const nextEpisode = computed(() => store.nextEpisode);
const previousEpisode = computed(() => store.previousEpisode);

function handleVideoEnded() {
  isPlaying.value = false;
  if (props.autoPlayEnabled && nextEpisode.value) {
    showCountdown.value = true;
    countdownCancelled.value = false;
  } else {
    emit("ended");
  }
}

function handleAutoPlayCancel() {
  showCountdown.value = false;
  countdownCancelled.value = true;
  emit("autoPlayCancelled");
}

function play() {
  videoRef.value?.play().catch(() => {});
  isPlaying.value = true;
}

function pause() {
  videoRef.value?.pause();
  isPlaying.value = false;
}

function togglePlay() {
  if (isPlaying.value) pause();
  else play();
}

function toggleMute() {
  if (videoRef.value) {
    videoRef.value.muted = !videoRef.value.muted;
    isMuted.value = videoRef.value.muted;
  }
}

function toggleFullscreen() {
  const video = videoRef.value;
  if (!video) return;

  if (!document.fullscreenElement) {
    video.requestFullscreen().catch(() => {});
  } else {
    document.exitFullscreen().catch(() => {});
  }
}

function handleTimeUpdate() {
  if (videoRef.value) {
    currentTime.value = videoRef.value.currentTime;
  }
}

function handleLoadedMetadata() {
  if (videoRef.value) {
    duration.value = videoRef.value.duration;
  }
}

function handleKeyDown(e: KeyboardEvent) {
  // Don't handle keys if user is typing in an input
  if (e.target instanceof HTMLInputElement || e.target instanceof HTMLTextAreaElement) return;

  switch (e.key) {
    case " ":
    case "k":
      e.preventDefault();
      togglePlay();
      break;
    case "ArrowLeft":
      e.preventDefault();
      if (previousEpisode.value) emit("play", previousEpisode.value);
      break;
    case "ArrowRight":
      e.preventDefault();
      if (nextEpisode.value) emit("play", nextEpisode.value);
      break;
    case "m":
      e.preventDefault();
      toggleMute();
      break;
    case "f":
      e.preventDefault();
      toggleFullscreen();
      break;
    case "Escape":
      if (isFullscreen.value) toggleFullscreen();
      break;
  }
}

function formatTime(seconds: number): string {
  if (isNaN(seconds)) return "0:00";
  const h = Math.floor(seconds / 3600);
  const m = Math.floor((seconds % 3600) / 60);
  const s = Math.floor(seconds % 60);
  return h > 0 ? `${h}:${m.toString().padStart(2, "0")}:${s.toString().padStart(2, "0")}` : `${m}:${s.toString().padStart(2, "0")}`;
}

onMounted(() => {
  document.addEventListener("keydown", handleKeyDown);
  document.addEventListener("fullscreenchange", () => {
    isFullscreen.value = !!document.fullscreenElement;
  });
});

onUnmounted(() => {
  document.removeEventListener("keydown", handleKeyDown);
});

watch(
  () => props.episode,
  () => {
    nextTick(() => {
      if (videoRef.value) {
        videoRef.value.play().catch(() => {});
        isPlaying.value = true;
      }
    });
  },
  { immediate: true }
);
</script>

<template>
  <div class="video-player" @mousemove="showControls = true" @mouseleave="showControls = false">
    <video
      ref="videoRef"
      class="video-player__video"
      :src="episode?.local_path"
      :poster="episode?.thumb"
      @ended="handleVideoEnded"
      @timeupdate="handleTimeUpdate"
      @loadedmetadata="handleLoadedMetadata"
      @play="() => (isPlaying = true)"
      @pause="() => (isPlaying = false)"
      playsinline
      crossorigin="anonymous"
    ></video>

    <div v-if="showCountdown && !countdownCancelled">
      <AutoPlayCountdown
        :seconds="5"
        :next-episode-title="nextEpisode?.title || ''"
        @cancel="handleAutoPlayCancel"
      />
    </div>

    <div
      v-if="showControls || isPlaying"
      class="video-player__controls"
      :class="{ 'video-player__controls--visible': showControls || isPlaying }"
    >
      <div class="video-player__progress" role="slider" aria-label="Progresso do vídeo" tabindex="0">
        <div
          class="video-player__progress-bar"
          :style="{ width: duration > 0 ? (currentTime / duration) * 100 + '%' : '0%' }"
        ></div>
        <div class="video-player__progress-handle" :style="{ left: duration > 0 ? (currentTime / duration) * 100 + '%' : '0%' }"></div>
      </div>

      <div class="video-player__controls-row">
        <div class="video-player__left">
          <EpisodeNavigation
            :has-previous="!!previousEpisode"
            :has-next="!!nextEpisode"
            :previous-title="previousEpisode?.title"
            :next-title="nextEpisode?.title"
            @previous="$emit('play', previousEpisode)"
            @next="$emit('play', nextEpisode)"
          />
        </div>

        <div class="video-player__center">
          <button
            class="video-player__btn"
            @click="togglePlay"
            :aria-label="isPlaying ? 'Pausar' : 'Reproduzir'"
            :title="isPlaying ? 'Pausar (Espaço)' : 'Reproduzir (Espaço)'"
          >
            <AppIcon v-if="isPlaying" name="pause" size="md" />
            <AppIcon v-else name="play" size="md" />
          </button>
        </div>

        <div class="video-player__right">
          <button
            class="video-player__btn"
            @click="toggleMute"
            :aria-label="isMuted ? 'Ativar som' : 'Mutar'"
            :title="isMuted ? 'Ativar som (M)' : 'Mutar (M)'"
          >
            <AppIcon v-if="isMuted" name="volume-x" size="md" />
            <AppIcon v-else name="volume-2" size="md" />
          </button>

          <span class="video-player__time">{{ formatTime(currentTime) }} / {{ formatTime(duration) }}</span>

          <button
            class="video-player__btn"
            @click="toggleFullscreen"
            :aria-label="isFullscreen ? 'Sair de tela cheia' : 'Tela cheia'"
            :title="isFullscreen ? 'Sair de tela cheia (F)' : 'Tela cheia (F)'"
          >
            <AppIcon v-if="isFullscreen" name="minimize-2" size="md" />
            <AppIcon v-else name="maximize-2" size="md" />
          </button>
        </div>
      </div>
    </div>

    <div v-if="!episode" class="video-player__empty">
      <AppIcon name="tv" size="xl" />
      <p>Selecione um episódio para assistir</p>
    </div>
  </div>
</template>

<style scoped>
.video-player {
  position: relative;
  width: 100%;
  aspect-ratio: 16 / 9;
  background: #000;
  border-radius: var(--radius-lg);
  overflow: hidden;
}

.video-player__video {
  width: 100%;
  height: 100%;
  object-fit: contain;
}

.video-player__controls {
  position: absolute;
  bottom: 0;
  left: 0;
  right: 0;
  padding: 1rem;
  background: linear-gradient(transparent, rgba(0, 0, 0, 0.85));
  opacity: 0;
  transition: opacity var(--speed) var(--ease);
  pointer-events: none;
}

.video-player__controls--visible {
  opacity: 1;
  pointer-events: auto;
}

.video-player__progress {
  position: relative;
  height: 4px;
  background: rgba(255, 255, 255, 0.3);
  border-radius: 999px;
  cursor: pointer;
  margin-bottom: 0.75rem;
}

.video-player__progress-bar {
  height: 100%;
  background: var(--accent);
  border-radius: 999px;
  transition: width 0.1s linear;
}

.video-player__progress-handle {
  position: absolute;
  top: 50%;
  width: 12px;
  height: 12px;
  background: var(--accent);
  border-radius: 50%;
  transform: translate(-50%, -50%);
  opacity: 0;
  transition: opacity var(--speed) var(--ease);
}

.video-player__progress:hover .video-player__progress-handle,
.video-player__progress:focus-within .video-player__progress-handle {
  opacity: 1;
}

.video-player__controls-row {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 1rem;
}

.video-player__left,
.video-player__center,
.video-player__right {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.video-player__left {
  flex: 1;
}

.video-player__center {
  flex: 0 0 auto;
}

.video-player__right {
  flex: 1;
  justify-content: flex-end;
}

.video-player__btn {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  width: 40px;
  height: 40px;
  border: none;
  border-radius: 50%;
  background: rgba(255, 255, 255, 0.15);
  color: #fff;
  cursor: pointer;
  transition: background var(--speed) var(--ease), transform var(--speed) var(--ease);
}

.video-player__btn:hover {
  background: rgba(255, 255, 255, 0.3);
  transform: scale(1.05);
}

.video-player__btn:focus-visible {
  outline: 2px solid var(--accent);
  outline-offset: 2px;
}

.video-player__time {
  font-size: 0.8rem;
  font-family: var(--mono);
  color: rgba(255, 255, 255, 0.9);
  min-width: 100px;
  text-align: right;
}

.video-player__empty {
  position: absolute;
  inset: 0;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 1rem;
  color: rgba(255, 255, 255, 0.6);
  text-align: center;
}

.video-player__empty :deep(.icon) {
  opacity: 0.4;
}
</style>
