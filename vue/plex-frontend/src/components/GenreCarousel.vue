<script setup>
import { useRouter } from "vue-router";
import { useI18n } from "vue-i18n";
import AppIcon from "./AppIcon.vue";

const { t } = useI18n();

defineProps({
  genreGroups: {
    type: Array,
    required: true,
  },
  isLoading: {
    type: Boolean,
    default: false,
  },
  error: {
    type: String,
    default: "",
  },
});

const router = useRouter();

function openMarkers(series) {
  router.push({ name: "series-markers", params: { seriesKey: series.ratingKey } });
}

function getTrack(button) {
  const section = button.closest(".genre-section");
  return section?.querySelector(".carousel-track");
}

function scrollLeft(e) {
  const track = getTrack(e.currentTarget);
  if (track) {
    track.scrollBy({ left: -track.clientWidth * 0.8, behavior: "smooth" });
  }
}

function scrollRight(e) {
  const track = getTrack(e.currentTarget);
  if (track) {
    track.scrollBy({ left: track.clientWidth * 0.8, behavior: "smooth" });
  }
}

function onWheel(e) {
  if (e.deltaY !== 0) {
    const track = e.currentTarget;
    track.scrollLeft += e.deltaY;
    e.preventDefault();
  }
}
</script>

<template>
  <div class="genre-carousel-view">
    <div v-if="error" class="alert" role="alert">
      <span>{{ error }}</span>
    </div>

    <div v-else-if="isLoading" class="genre-carousel-skeleton">
      <div v-for="n in 3" :key="n" class="genre-section-skeleton">
        <div class="genre-header-skeleton">
          <div class="skeleton-line"></div>
        </div>
        <div class="carousel-track-skeleton">
          <div v-for="i in 4" :key="i" class="carousel-card-skeleton">
            <div class="carousel-card-poster skeleton"></div>
            <div class="carousel-card-info">
              <div class="skeleton-line"></div>
            </div>
          </div>
        </div>
      </div>
    </div>

    <div v-else-if="genreGroups.length > 0" class="genre-carousel-container">
      <section
        v-for="group in genreGroups"
        :key="group.genre"
        class="genre-section"
      >
        <header class="genre-header">
          <h2 class="genre-title">{{ group.genre }}</h2>
          <span class="genre-count">{{ t("series.seriesCount", { count: group.series.length }) }}</span>
        </header>

        <div class="carousel-wrapper">
          <button
            v-if="group.series.length > 2"
            class="scroll-btn scroll-btn--left carousel-btn-left"
            type="button"
            :aria-label="t('aria.scrollLeft')"
            @click="scrollLeft($event)"
          >
            <AppIcon name="chevron-left" size="md" />
          </button>

          <div class="carousel-track" @wheel="onWheel($event)">
            <div
              v-for="series in group.series"
              :key="series.ratingKey"
              class="carousel-card"
              @click="openMarkers(series)"
            >
              <div class="carousel-card-poster">
                <img
                  v-if="series.thumb"
                  :src="series.thumb"
                  :alt="series.title"
                  loading="lazy"
                />
                <span v-else>📺</span>
              </div>
              <div class="carousel-card-info">
                <h4>{{ series.title }}</h4>
                <p v-if="series.summary" class="carousel-summary">{{ series.summary }}</p>
              </div>
            </div>
          </div>

          <button
            v-if="group.series.length > 2"
            class="scroll-btn scroll-btn--right carousel-btn-right"
            type="button"
            :aria-label="t('aria.scrollRight')"
            @click="scrollRight(group.genre, $event)"
          >
            <AppIcon name="chevron-right" size="md" />
          </button>
        </div>
      </section>
    </div>

    <div v-else-if="!isLoading && !error" class="empty">
      <p>{{ t("series.noSeriesInLibrary") }}</p>
    </div>
  </div>
</template>

<style scoped>
.genre-carousel-view {
  padding: 1.5rem;
  max-width: 100%;
}

.genre-carousel-container {
  display: flex;
  flex-direction: column;
  gap: 2rem;
}

.genre-section {
  flex-shrink: 0;
}

.genre-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 0.75rem;
  padding: 0 0.5rem;
}

.genre-title {
  font-size: 1.25rem;
  font-weight: 700;
  margin: 0;
  color: var(--text);
}

.genre-count {
  font-size: 0.75rem;
  color: var(--text-tertiary);
  background: var(--bg-sunken);
  padding: 0.125rem 0.5rem;
  border-radius: 999px;
}

.carousel-wrapper {
  display: flex;
  align-items: center;
  gap: 0;
  position: relative;
}

.carousel-track {
  display: flex;
  gap: 0.75rem;
  overflow-x: auto;
  padding: 0.25rem 0.5rem;
  scrollbar-width: thin;
  scrollbar-color: var(--border) transparent;
  scroll-snap-type: x mandatory;
  flex: 1;
  min-width: 0;
}

.carousel-track::-webkit-scrollbar {
  height: 8px;
}

.carousel-track::-webkit-scrollbar-track {
  background: transparent;
}

.carousel-track::-webkit-scrollbar-thumb {
  background: var(--border);
  border-radius: 4px;
}

.carousel-card {
  flex: 0 0 160px;
  scroll-snap-align: start;
  transition: transform 0.2s ease, box-shadow 0.2s ease, z-index 0.2s;
  z-index: 1;
  border-radius: 8px;
  overflow: hidden;
}

.carousel-card:hover {
  transform: scale(1.08) translateY(-4px);
  box-shadow: var(--shadow-2);
  z-index: 10;
}

.carousel-card-poster {
  aspect-ratio: 2/3;
  border-radius: 8px;
  background: var(--bg-sunken);
  margin-bottom: 0.5rem;
  overflow: hidden;
  position: relative;
}

.carousel-card-poster img {
  width: 100%;
  height: 100%;
  object-fit: cover;
  transition: transform 0.2s ease;
}

.carousel-card:hover .carousel-card-poster img {
  transform: scale(1.05);
}

.carousel-card-poster span {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 100%;
  height: 100%;
  color: var(--text-tertiary);
  font-size: 2rem;
}

.carousel-card-info {
  padding: 0 0.25rem;
}

.carousel-card-info h4 {
  font-size: 0.8125rem;
  font-weight: 600;
  margin: 0 0 0.25rem;
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
}

.carousel-summary {
  font-size: 0.6875rem;
  color: var(--text-secondary);
  margin: 0;
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
}

.scroll-btn {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 40px;
  height: 100%;
  min-height: 240px;
  background: linear-gradient(
    90deg,
    var(--bg-elevated) 0%,
    transparent 100%
  );
  border: none;
  cursor: pointer;
  color: var(--text-secondary);
  transition: background 0.2s, color 0.2s;
  z-index: 5;
  flex-shrink: 0;
}

.scroll-btn:hover {
  background: linear-gradient(
    90deg,
    color-mix(in srgb, var(--primary) 15%, var(--bg-elevated)) 0%,
    transparent 100%
  );
  color: var(--primary);
}

.scroll-btn:focus-visible {
  outline: 2px solid var(--primary);
  outline-offset: -2px;
}

.scroll-btn--left {
  border-radius: 8px 0 0 8px;
  background: linear-gradient(
    to right,
    var(--bg-elevated) 0%,
    transparent 100%
  );
}

.scroll-btn--right {
  border-radius: 0 8px 8px 0;
  background: linear-gradient(
    to left,
    var(--bg-elevated) 0%,
    transparent 100%
  );
}

.genre-carousel-skeleton {
  display: flex;
  flex-direction: column;
  gap: 2rem;
}

.genre-section-skeleton {
  flex-shrink: 0;
}

.genre-header-skeleton {
  margin-bottom: 0.75rem;
  padding: 0 0.5rem;
}

.carousel-wrapper-skeleton {
  display: flex;
  gap: 0.75rem;
  padding: 0 0.5rem;
}

.carousel-track-skeleton {
  display: flex;
  gap: 0.75rem;
  flex: 1;
}

.carousel-card-skeleton {
  flex: 0 0 160px;
}

.carousel-card-poster {
  aspect-ratio: 2/3;
  border-radius: 8px;
  background: var(--bg-sunken);
  margin-bottom: 0.5rem;
}

.skeleton-line {
  height: 1rem;
  background: linear-gradient(90deg, var(--bg-sunken) 25%, var(--border) 50%, var(--bg-sunken) 75%);
  background-size: 200% 100%;
  animation: shimmer 1.5s infinite;
  border-radius: 4px;
  margin-bottom: 0.5rem;
}

.skeleton {
  animation: pulse 1.5s ease-in-out infinite;
}

@keyframes shimmer {
  0% { background-position: -200% 0; }
  100% { background-position: 200% 0; }
}

@keyframes pulse {
  0%, 100% { opacity: 1; }
  50% { opacity: 0.5; }
}

.empty {
  color: var(--text-tertiary);
  font-style: italic;
  text-align: center;
  padding: 3rem;
}

.alert {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  padding: 0.75rem 1rem;
  border-radius: 8px;
  background: color-mix(in srgb, var(--danger) 10%, transparent);
  color: var(--danger);
  margin-bottom: 1rem;
}
</style>