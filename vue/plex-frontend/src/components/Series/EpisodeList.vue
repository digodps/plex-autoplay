<script setup lang="ts">
/* global HTMLElement */
import { computed, ref, watch, nextTick } from "vue";
import EpisodeItem from "./EpisodeItem.vue";
import AppIcon from "@/components/AppIcon.vue";

const props = defineProps({
  episodes: { type: Array, default: () => [] },
  currentEpisode: { type: Object, default: null },
});

const emit = defineEmits<{ play: [episode: Record<string, unknown>] }>();

const listContainer = ref<HTMLElement | null>(null);

const hasEpisodes = computed(() => props.episodes.length > 0);

function handlePlay(episode) {
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
  <div ref="listContainer" class="episode-list" role="list" aria-label="Episódios da série">
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
      <p class="episode-list__empty-text">{{ $t("series.noEpisodes") }}</p>
    </div>
  </div>
</template>

<style scoped>
.episode-list {
  display: flex;
  flex-direction: column;
  gap: var(--s-2);
  overflow-y: auto;
  max-height: 100%;
  padding: var(--s-2);
}

.episode-list__items {
  display: flex;
  flex-direction: column;
  gap: var(--s-2);
}

.episode-list__empty {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: var(--s-8) var(--s-4);
  color: var(--muted);
  text-align: center;
}

.episode-list__empty-icon {
  margin-bottom: var(--s-3);
  opacity: 0.5;
}

.episode-list__empty-text {
  margin: 0;
  font-size: var(--t-small);
}
</style>