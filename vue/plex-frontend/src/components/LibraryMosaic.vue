<script setup>
import { computed } from "vue";
import { useLibraryMosaic } from "../composables/useLibraryMosaic";
import AppIcon from "@/components/AppIcon.vue";

const props = defineProps({
  libraryKey: { type: String, required: true },
  series: { type: Array, default: () => [] },
  fallbackIcon: { type: String, default: "tv" },
});

const { posters, isLoading, error, startHover, stopHover } = useLibraryMosaic(props.libraryKey);

const hasPosters = computed(() => posters.value.length > 0);
const mosaicStyle = computed(() => ({
  "--mosaic-columns": 3,
  "--mosaic-rows": 2,
}));
const ariaLabel = computed(() => `Mosaic de posters da biblioteca ${props.libraryKey}`);

function handleMouseEnter() {
  startHover();
}

function handleMouseLeave() {
  stopHover();
}

function handleFocus() {
  startHover();
}

function handleBlur() {
  stopHover();
}
</script>

<template>
  <div
    class="library-mosaic"
    :style="mosaicStyle"
    tabindex="0"
    role="img"
    :aria-label="ariaLabel"
    @mouseenter="handleMouseEnter"
    @mouseleave="handleMouseLeave"
    @focusin="handleFocus"
    @focusout="handleBlur"
  >
    <div v-if="isLoading && !hasPosters" class="mosaic-skeleton" aria-hidden="true">
      <div v-for="i in 6" :key="i" class="skeleton-item" />
    </div>

    <div v-else-if="hasPosters" class="mosaic-grid" aria-hidden="true">
      <div
        v-for="poster in posters"
        :key="poster.ratingKey"
        class="mosaic-poster"
        :style="{ backgroundImage: `url(${poster.thumb})` }"
        :aria-label="poster.title"
      >
        <span class="sr-only">{{ poster.title }}</span>
      </div>
    </div>

    <div v-else class="mosaic-fallback" aria-hidden="true">
      <AppIcon :name="fallbackIcon" size="xl" />
    </div>

    <div v-if="error && !hasPosters" class="mosaic-error" aria-hidden="true">
      <AppIcon name="circle-alert" size="lg" />
    </div>
  </div>
</template>

<style scoped>
.library-mosaic {
  position: relative;
  width: 100%;
  aspect-ratio: 16 / 9;
  border-radius: var(--r-lg);
  overflow: hidden;
  background: var(--surface);
  border: 1px solid var(--border);
  contain: layout style paint;
}

.mosaic-grid {
  position: absolute;
  inset: 0;
  display: grid;
  grid-template-columns: repeat(var(--mosaic-columns, 3), 1fr);
  grid-template-rows: repeat(var(--mosaic-rows, 2), 1fr);
  gap: 2px;
}

.mosaic-poster {
  position: relative;
  width: 100%;
  height: 100%;
  background-size: cover;
  background-position: center;
  background-repeat: no-repeat;
  border-radius: 2px;
  transition:
    transform var(--dur) var(--ease),
    opacity var(--dur) var(--ease),
    filter var(--dur) var(--ease);
  will-change: transform, opacity;
}

.mosaic-poster.shuffling {
  animation: mosaicShuffle 0.4s var(--ease);
}

@keyframes mosaicShuffle {
  0% {
    transform: scale(1);
    opacity: 1;
  }
  50% {
    transform: scale(0.95) rotate(var(--rotate, 0deg));
    opacity: 0.7;
  }
  100% {
    transform: scale(1);
    opacity: 1;
  }
}

.library-mosaic:hover .mosaic-poster {
  filter: brightness(1.05) saturate(1.1);
}

.mosaic-skeleton {
  position: absolute;
  inset: 0;
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  grid-template-rows: repeat(2, 1fr);
  gap: 2px;
  padding: 2px;
}

.skeleton-item {
  background: linear-gradient(
    90deg,
    var(--surface) 25%,
    var(--border) 50%,
    var(--surface) 75%
  );
  background-size: 200% 100%;
  animation: shimmer 1.5s infinite;
  border-radius: 2px;
}

@keyframes shimmer {
  0% { background-position: 200% 0; }
  100% { background-position: -200% 0; }
}

.mosaic-fallback {
  position: absolute;
  inset: 0;
  display: flex;
  align-items: center;
  justify-content: center;
  color: var(--muted);
  opacity: 0.6;
  transition: opacity var(--dur) var(--ease);
}

.library-mosaic:hover .mosaic-fallback {
  opacity: 0.8;
}

.mosaic-error {
  position: absolute;
  inset: 0;
  display: flex;
  align-items: center;
  justify-content: center;
  color: var(--danger, #e03e3e);
  opacity: 0.5;
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

@media (prefers-reduced-motion: reduce) {
  .mosaic-poster {
    transition: none !important;
    animation: none !important;
  }
  .skeleton-item {
    animation: none !important;
    background: var(--surface);
  }
}

.library-mosaic:focus-visible {
  outline: 2px solid var(--action);
  outline-offset: 2px;
  z-index: 1;
}
</style>