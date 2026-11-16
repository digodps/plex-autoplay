<script setup lang="ts">
import { computed } from "vue";
import AppIcon from "@/components/AppIcon.vue";

interface Props {
  episode: {
    rating_key: string;
    title: string;
    season: number;
    episode: number;
    thumb: string | null;
    watched: boolean;
    watch_progress: number;
    duration_seconds: number;
  };
  isCurrent: boolean;
}

const props = defineProps<Props>();
const emit = defineEmits<{ play: [episode: typeof props.episode] }>();

const progressPercent = computed(() => props.episode.watch_progress || 0);
const isWatched = computed(() => props.episode.watched);
const isPartial = computed(() => !isWatched.value && progressPercent.value > 0);
const isInProgress = computed(() => props.isCurrent && !isWatched.value);

const statusIcon = computed(() => {
  if (isInProgress.value) return "play-circle";
  if (isWatched.value) return "check";
  if (isPartial.value) return "circle-dotted";
  return "play";
});

function formatDuration(seconds: number): string {
  const h = Math.floor(seconds / 3600);
  const m = Math.floor((seconds % 3600) / 60);
  return h > 0 ? `${h}h ${m}m` : `${m}m`;
}

function handleClick() {
  emit("play", props.episode);
}
</script>

<template>
  <article
    class="episode-item"
    :class="{
      'episode-item--current': isCurrent,
      'episode-item--watched': isWatched,
      'episode-item--in-progress': isInProgress,
    }"
    @click="handleClick"
    role="button"
    tabindex="0"
    @keydown.enter="handleClick"
    @keydown.space.prevent="handleClick"
  >
    <div class="episode-item__thumb">
      <img v-if="episode.thumb" :src="episode.thumb" :alt="episode.title" loading="lazy" />
      <AppIcon v-else name="tv" size="lg" />
      <div v-if="isInProgress" class="episode-item__pulse" aria-hidden="true"></div>
    </div>

    <div class="episode-item__info">
      <div class="episode-item__header">
        <span class="episode-item__ep-badge">
          T{{ episode.season }} · E{{ episode.episode }}
        </span>
        <h4 class="episode-item__title">{{ episode.title }}</h4>
      </div>

      <div class="episode-item__meta">
        <span class="episode-item__duration">{{ formatDuration(episode.duration_seconds) }}</span>
        <div class="episode-item__status" :title="statusTitle">
          <AppIcon :name="statusIcon" size="sm" />
          <span v-if="isWatched" class="sr-only">Assistido</span>
          <span v-else-if="isPartial" class="sr-only">{{ progressPercent }}% assistido</span>
          <span v-else-if="isInProgress" class="sr-only">Reproduzindo agora</span>
          <span v-else class="sr-only">Não assistido</span>
        </div>
      </div>

      <div v-if="isPartial" class="episode-item__progress" role="progressbar" :aria-valuenow="progressPercent" aria-valuemin="0" aria-valuemax="100" :aria-label="`Progresso: ${progressPercent}%`">
        <div class="episode-item__progress-bar" :style="{ width: progressPercent + '%' }"></div>
      </div>
    </div>
  </article>
</template>

<style scoped>
.episode-item {
  display: flex;
  gap: 1rem;
  padding: 0.75rem;
  border-radius: var(--radius);
  background: var(--surface);
  border: 1px solid var(--border);
  cursor: pointer;
  transition: background var(--speed) var(--ease), border-color var(--speed) var(--ease), box-shadow var(--speed) var(--ease);
}

.episode-item:hover {
  background: var(--bg-sunken);
  border-color: var(--border-strong);
}

.episode-item:focus-visible {
  outline: 2px solid var(--accent);
  outline-offset: 2px;
}

.episode-item--current {
  border-color: var(--accent);
  box-shadow: 0 0 0 1px var(--accent);
}

.episode-item--watched {
  opacity: 0.6;
}

.episode-item--watched .episode-item__title {
  color: var(--text-secondary);
}

.episode-item--in-progress {
  border-color: var(--accent);
}

.episode-item__thumb {
  position: relative;
  flex: 0 0 160px;
  aspect-ratio: 16 / 9;
  border-radius: var(--radius-sm);
  overflow: hidden;
  background: var(--bg-sunken);
}

.episode-item__thumb img {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.episode-item__pulse {
  position: absolute;
  inset: 0;
  background: linear-gradient(90deg, transparent, color-mix(in srgb, var(--accent) 30%, transparent), transparent);
  background-size: 200% 100%;
  animation: pulse-shimmer 1.5s ease-in-out infinite;
  pointer-events: none;
}

@keyframes pulse-shimmer {
  0% { background-position: 200% 0; }
  100% { background-position: -200% 0; }
}

.episode-item__info {
  flex: 1;
  min-width: 0;
  display: flex;
  flex-direction: column;
  gap: 0.35rem;
}

.episode-item__header {
  display: flex;
  align-items: flex-start;
  gap: 0.75rem;
}

.episode-item__ep-badge {
  flex: 0 0 auto;
  font-size: 0.7rem;
  font-weight: 700;
  color: var(--accent);
  background: color-mix(in srgb, var(--accent) 12%, transparent);
  padding: 0.15rem 0.4rem;
  border-radius: 999px;
  white-space: nowrap;
}

.episode-item__title {
  font-size: 0.95rem;
  font-weight: 600;
  color: var(--text);
  line-height: 1.3;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.episode-item__meta {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 0.5rem;
}

.episode-item__duration {
  font-size: 0.75rem;
  color: var(--text-tertiary);
  font-family: var(--mono);
}

.episode-item__status {
  display: flex;
  align-items: center;
  gap: 0.35rem;
  font-size: 0.75rem;
  color: var(--text-secondary);
}

.episode-item__status :deep(.icon) {
  color: var(--accent);
}

.episode-item__progress {
  height: 4px;
  background: var(--bg-sunken);
  border-radius: 999px;
  overflow: hidden;
  margin-top: 0.25rem;
}

.episode-item__progress-bar {
  height: 100%;
  background: var(--accent);
  border-radius: 999px;
  transition: width 0.3s var(--ease);
}

@media (max-width: 480px) {
  .episode-item {
    flex-direction: column;
    gap: 0.5rem;
  }

  .episode-item__thumb {
    flex: none;
    width: 100%;
  }

  .episode-item__header {
    flex-direction: column;
    gap: 0.25rem;
  }
}
</style>
