<script setup>
import { computed, onMounted } from "vue";
import { useRoute, useRouter } from "vue-router";
import DefaultLayout from "@/components/layouts/DefaultLayout.vue";
import AppIcon from "@/components/AppIcon.vue";
import MarkerRow from "@/components/MarkerRow.vue";
import { useMarkerStore } from "@/stores/markers";

const route = useRoute();
const router = useRouter();
const store = useMarkerStore();
const seriesKey = computed(() => route.params.seriesKey);

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
  <DefaultLayout>
    <div class="mb-6 flex flex-wrap items-start gap-4">
      <button
        class="inline-flex items-center gap-1 rounded-lg border border-stroke bg-white px-3 py-2 text-sm font-medium text-body transition hover:border-primary hover:text-primary dark:border-strokedark dark:bg-boxdark dark:text-bodydark"
        @click="router.back()"
      >
        <AppIcon name="chevron-left" size="sm" />
        Voltar
      </button>
      <div>
        <h1 class="text-title-md font-bold text-black dark:text-white">Marcadores da série</h1>
        <p class="mt-1 text-sm font-medium text-body dark:text-bodydark">
          Selecione um campo de intro ou créditos para editar ou adicionar.
        </p>
      </div>
    </div>

    <div
      v-if="store.error"
      class="mb-6 flex items-center gap-3 rounded-sm border border-meta-1/30 bg-meta-1/10 px-5 py-4 text-sm font-medium text-meta-1"
      role="alert"
    >
      <AppIcon name="warning" size="sm" />
      <span class="flex-1">{{ store.error }}</span>
      <button class="rounded p-1 transition hover:bg-meta-1/10" @click="store.clearError()">
        <AppIcon name="close" size="sm" />
      </button>
    </div>

    <div v-if="store.loading" class="space-y-4">
      <div v-for="n in 3" :key="n" class="h-32 animate-pulse rounded-sm border border-stroke bg-white dark:border-strokedark dark:bg-boxdark"></div>
    </div>

    <div v-else class="space-y-6">
      <section
        v-for="{ season, list } in seasons"
        :key="season"
        class="rounded-sm border border-stroke bg-white shadow-default dark:border-strokedark dark:bg-boxdark"
      >
        <h2 class="border-b border-stroke px-6 py-4 text-title-sm font-semibold text-black dark:border-strokedark dark:text-white">
          Temporada {{ season }}
        </h2>

        <div class="divide-y divide-stroke dark:divide-strokedark">
          <article v-for="ep in list" :key="ep.rating_key" class="px-6 py-4">
            <div class="mb-3 flex flex-wrap items-center gap-2">
              <span class="inline-flex rounded-full bg-meta-2 px-2.5 py-0.5 text-xs font-semibold text-body dark:bg-meta-4 dark:text-bodydark">
                T{{ ep.season }} · E{{ ep.episode }}
              </span>
              <h3 class="text-sm font-semibold text-black dark:text-white">{{ ep.title }}</h3>
            </div>
            <div class="space-y-1">
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

      <div
        v-if="seasons.length === 0"
        class="rounded-sm border border-stroke bg-white px-6 py-11 text-center dark:border-strokedark dark:bg-boxdark"
      >
        <AppIcon name="tv" size="lg" class="mx-auto text-body dark:text-bodydark" />
        <p class="mt-3 text-sm font-medium text-body dark:text-bodydark">Nenhum episódio nesta série.</p>
      </div>
    </div>
  </DefaultLayout>
</template>