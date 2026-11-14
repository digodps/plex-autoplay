<script setup>
import { computed, onMounted, ref } from "vue";
import { useRoute, useRouter } from "vue-router";
import { useMarkerStore } from "../stores/markers";
import AppIcon from "../components/AppIcon.vue";
import MarkerRow from "../components/MarkerRow.vue";
import { reprocessMarkers } from "../api";

const route = useRoute();
const router = useRouter();
const store = useMarkerStore();
const reprocessing = ref(false);

const seriesKey = computed(() => route.params.seriesKey);

async function handleReprocess() {
  if (!seriesKey.value) return;
  reprocessing.value = true;
  try {
    const res = await reprocessMarkers(seriesKey.value);
    if (!res.ok) throw new Error("Falha ao reprocessar marcadores");
    await res.json();
    await store.fetchSeries(seriesKey.value);
  } catch (e) {
    store.error.value = e.message || "Falha ao reprocessar marcadores.";
  } finally {
    reprocessing.value = false;
  }
}

onMounted(() => {
  store.fetchSeries(seriesKey.value);
});

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
</script>

<template>
  <div class="editor">
    <header class="editor-header">
      <button class="btn btn-ghost back" @click="router.back()">
        <AppIcon name="chevron-left" size="sm" /> Voltar
      </button>
      <div>
        <h1>Marcadores da série</h1>
        <p class="subtitle">Selecione um campo de intro ou créditos para editar ou adicionar.</p>
      </div>
      <button
        class="btn btn-ghost reprocess"
        :disabled="reprocessing"
        data-testid="markers-reprocess"
        @click="handleReprocess"
      >
        <AppIcon name="refresh" size="sm" />
        {{ reprocessing ? "Reprocessando…" : "Reprocessar" }}
      </button>
    </header>

    <div v-if="store.error" class="alert" role="alert">
      <AppIcon name="warning" size="sm" />
      <span>{{ store.error }}</span>
      <button class="btn btn-ghost dismiss" @click="store.clearError()">
        <AppIcon name="close" size="sm" />
      </button>
    </div>

    <div v-if="store.loading" class="editor-skeleton">
      <div v-for="n in 3" :key="n" class="skeleton group-line" />
      <div class="skeleton row-line" />
      <div class="skeleton row-line" />
      <div class="skeleton row-line" />
    </div>

    <div v-else class="seasons">
      <section v-for="{ season, list } in seasons" :key="season" class="season">
        <h2 class="season-title">Temporada {{ season }}</h2>

        <div class="episodes-list">
          <article v-for="ep in list" :key="ep.rating_key" class="episode">
            <div class="episode-head">
              <span class="episode-number">T{{ ep.season }} · E{{ ep.episode }}</span>
              <h3 class="episode-title">
                {{ ep.title }}
              </h3>
            </div>

            <div class="marker-rows">
              <MarkerRow
                v-for="kind in ['intro', 'credits']"
                :key="kind"
                :rating-key="ep.rating_key"
                :kind="kind"
              />
            </div>
          </article>
        </div>
      </section>

      <div v-if="seasons.length === 0" class="empty">
        <AppIcon name="tv" size="lg" />
        <p>Nenhum episódio nesta série.</p>
      </div>
    </div>
  </div>
</template>

<style scoped src="./marker-editor.css"></style>
