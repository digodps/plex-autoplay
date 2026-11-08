<script setup>
import { ref, onMounted } from "vue";
import { useRouter } from "vue-router";
import DefaultLayout from "@/components/layouts/DefaultLayout.vue";
import AppIcon from "@/components/AppIcon.vue";
import { apiFetch } from "@/api";

const router = useRouter();
const libraries = ref([]);
const error = ref("");
const loading = ref(false);

async function loadLibraries() {
  loading.value = true;
  error.value = "";
  try {
    const response = await apiFetch("/api/plex/libraries");
    if (response.ok) libraries.value = await response.json();
    else error.value = "Erro ao carregar bibliotecas";
  } catch {
    error.value = "Erro de conexão";
  } finally {
    loading.value = false;
  }
}

onMounted(loadLibraries);

function navigateToSeries(libraryKey, libraryTitle) {
  router.push({
    name: "series",
    params: { key: libraryKey },
    query: { libraryTitle },
  });
}
</script>

<template>
  <DefaultLayout>
    <div class="mb-6">
      <h1 class="text-title-md font-bold text-black dark:text-white">Bibliotecas</h1>
      <p class="mt-1 text-sm font-medium text-body dark:text-bodydark">
        Bibliotecas de séries detectadas no Plex.
      </p>
    </div>

    <div
      v-if="error"
      class="mb-6 flex items-center gap-3 rounded-sm border border-meta-1/30 bg-meta-1/10 px-5 py-4 text-sm font-medium text-meta-1"
      role="alert"
    >
      <AppIcon name="warning" size="sm" />
      {{ error }}
    </div>

    <div v-if="loading" class="grid grid-cols-1 gap-5 sm:grid-cols-2 lg:grid-cols-3 2xl:gap-7.5">
      <div
        v-for="n in 6"
        :key="n"
        class="h-40 animate-pulse rounded-sm border border-stroke bg-white dark:border-strokedark dark:bg-boxdark"
      ></div>
    </div>

    <div
      v-else-if="libraries.length"
      class="grid grid-cols-1 gap-5 sm:grid-cols-2 lg:grid-cols-3 2xl:gap-7.5"
    >
      <button
        v-for="lib in libraries"
        :key="lib.key"
        type="button"
        class="group flex flex-col rounded-sm border border-stroke bg-white p-6 text-left shadow-default transition hover:border-primary hover:shadow-2 dark:border-strokedark dark:bg-boxdark"
        @click="navigateToSeries(lib.key)"
      >
        <span
          class="mb-4 flex h-12 w-12 items-center justify-center rounded-full bg-meta-2 text-primary transition group-hover:bg-primary group-hover:text-white dark:bg-meta-4"
        >
          <AppIcon name="tv" size="lg" />
        </span>
        <h3 class="text-title-sm font-semibold text-black dark:text-white">{{ lib.title }}</h3>
        <span class="mt-3 inline-flex items-center gap-1 text-sm font-medium text-primary">
          Ver séries
          <AppIcon name="chevron-right" size="xs" />
        </span>
      </button>
    </div>

    <div
      v-else
      class="rounded-sm border border-stroke bg-white px-6 py-11 text-center dark:border-strokedark dark:bg-boxdark"
    >
      <AppIcon name="tv" size="lg" class="mx-auto text-body dark:text-bodydark" />
      <p class="mt-3 text-sm font-medium text-body dark:text-bodydark">
        Nenhuma biblioteca de séries encontrada.
      </p>
    </div>
  </DefaultLayout>
</template>