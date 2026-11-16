<script setup lang="ts">
/* global HTMLElement */
import { computed, ref, watch, nextTick } from "vue";
import EpisodeItem from "./EpisodeItem.vue";

interface Props {
  episodes: typeof import("@/stores/series").useSeriesStore extends () => infer T ? T extends { episodes: infer E } ? E : never : never;
  currentEpisode: typeof import("@/stores/series").useSeriesStore extends () => infer T ? T extends { currentEpisode: infer E } ? E : never : never;
}

const props = defineProps<Props>();
const emit = defineEmits<{ play: [episode: Props["episodes"][0]] }>();

const listContainer = ref<HTMLElement | null>(null);

const hasEpisodes = computed(() => props.episodes.length > 0);

function handlePlay(episode: typeof props.episodes[0]) {
  emit("play", episode);
}

function scrollToCurrent() {
  if (!listContainer.value || !props.currentEpisode) return;
  const currentElement = listContainer.value.querySelector(".episode-item--current");
  if (currentElement) {
    currentElement.scrollIntoView({ behavior: "smooth", block: "center" });
  }
}

watch(
  () => props.currentEpisode?.rating_key,
  () => {
    nextTick(() => scrollToCurrent());
  }
);
</script>

<template>
  <div class="episode-list" ref="listContainer" role="list" aria-label="Episódios da série">
    <div v-if="hasEpisodes" class="episode-list__items">
      <EpisodeItem
        v-for="episode in episodes"
        :key="episode.rating_key"
        :episode="episode"
        :is-current="currentEpisode?.rating_key === episode.rating_key"
        @play="handlePlay"
      />
    </div>

    <div v-else class="episode-list__empty">
      <AppIcon name="tv" size="lg" class="episode-list__empty-icon" />
      <p class="episode-list__empty-text">Nenhum episódio encontrado</p>
    </div>
  </div>
</template>

<style scoped>
.episode-list {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
  overflow-y: auto;
  max-height: 100%;
  padding: 0.5rem;
}

.episode-list__items {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.episode-list__empty {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 3rem 1rem;
  color: var(--text-tertiary);
  text-align: center;
}

.episode-list__empty-icon {
  margin-bottom: 0.75rem;
  opacity: 0.5;
}

.episode-list__empty-text {
  margin: 0;
  font-size: 0.9rem;
}

@media (max-width: 767px) {
  .episode-list {
    padding: 0.75rem;
  }
}
</style>
