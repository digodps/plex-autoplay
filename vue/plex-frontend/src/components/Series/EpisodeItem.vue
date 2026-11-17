<script setup lang="ts">
import { computed } from "vue";
import AppIcon from "@/components/AppIcon.vue";

const props = defineProps({
  episode: {
    type: Object,
    required: true,
  },
  isCurrent: { type: Boolean, default: false },
});

const emit = defineEmits(["play"]);

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

function formatDuration(seconds) {
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
    role="button"
    tabindex="0"
    @click="handleClick"
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
  gap: var(--s-3);
  padding: var(--s-3);
  border-radius: var(--r-md);
  background: var(--surface);
  border: 1px solid var(--border);
  cursor: pointer;
  transition: background var(--dur) var(--ease), border-color var(--dur) var(--ease), box-shadow var(--dur) var(--ease);
}

@media (hover: hover) and (pointer: fine) {
  .episode-item:hover {
    background: var(--border);
    border-color: var(--line-strong);
  }
}

.episode-item:focus-visible {
  outline: 2px solid var(--action);
  outline-offset: 2px;
}

.episode-item--current {
  border-color: var(--action);
  box-shadow: 0 0 0 1px var(--action);
}

.episode-item--watched {
  opacity: 0.6;
}

.episode-item--watched .episode-item__title {
  color: var(--muted);
}

.episode-item--in-progress {
  border-color: var(--action);
}

.episode-item__thumb {
  position: relative;
  flex: 0 0 160px;
  aspect-ratio: 16 / 9;
  border-radius: var(--r-sm);
  overflow: hidden;
  background: var(--border);
}

.episode-item__thumb img {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.episode-item__pulse {
  position: absolute;
  inset: 0;
  background: linear-gradient(90deg, transparent, color-mix(in srgb, var(--action) 30%, transparent), transparent);
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
  gap: var(--s-1);
}

.episode-item__header {
  display: flex;
  align-items: flex-start;
  gap: var(--s-3);
}

.episode-item__ep-badge {
  flex: 0 0 auto;
  font-size: var(--t-label);
  font-weight: 700;
  color: var(--action);
  background: color-mix(in srgb, var(--action) 12%, transparent);
  padding: var(--s-1) var(--s-2);
  border-radius: 999px;
  white-space: nowrap;
}

.episode-item__title {
  font-size: var(--t-body);
  font-weight: 600;
  color: var(--ink);
  line-height: 1.3;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.episode-item__meta {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: var(--s-2);
}

.episode-item__duration {
  font-size: var(--t-small);
  color: var(--muted);
  font-family: var(--font-mono);
}

.episode-item__status {
  display: flex;
  align-items: center;
  gap: var(--s-1);
  font-size: var(--t-small);
  color: var(--muted);
}

.episode-item__status :deep(.icon) {
  color: var(--action);
}

.episode-item__progress {
  height: 4px;
  background: var(--border);
  border-radius: 999px;
  overflow: hidden;
  margin-top: var(--s-1);
}

.episode-item__progress-bar {
  height: 100%;
  background: var(--action);
  border-radius: 999px;
  transition: width 0.3s var(--ease);
}

@media (max-width: 480px) {
  .episode-item {
    flex-direction: column;
    gap: var(--s-2);
  }

  .episode-item__thumb {
    flex: none;
    width: 100%;
  }

  .episode-item__header {
    flex-direction: column;
    gap: var(--s-1);
  }
}

.sr-only {
  position: absolute;
  width: 1px;
  height: 1px;
  padding: 0;
  margin: -1px;
  overflow: hidden;
  clip: rect(0, 0, 0, 0);
  white-space: nowrap;
  border: 0;
}
</style>