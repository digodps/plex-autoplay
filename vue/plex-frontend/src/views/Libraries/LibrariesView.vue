<script setup>
import { ref, onMounted } from "vue";
import { useRouter } from "vue-router";
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
  <div class="min-h-screen bg-[#111827] text-white">
    <div class="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8 py-6">
      <div class="flex items-center justify-between gap-3">
        <div>
          <h1 class="text-2xl font-bold text-white">Bibliotecas</h1>
          <p class="mt-1 text-sm text-gray-400">
            Bibliotecas de séries detectadas no Plex.
          </p>
        </div>
        <div class="flex items-center gap-2">
          <button
            class="rounded-lg border border-stroke px-4 py-2 text-sm font-medium text-gray-700 transition hover:border-accent hover:text-accent dark:border-strokedark dark:text-bodydark"
            @click="loadLibraries"
          >
            Atualizar
          </button>
        </div>
      </div>

      <div class="mt-8">
        <div v-if="error" class="rounded-lg border border-red-900 bg-red-900/20 p-4 text-sm text-red-300 mb-6" role="alert">
          {{ error }}
        </div>

        <div v-if="loading" class="grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-3 animate-pulse">
          <div class="h-44 rounded-xl border border-gray-800 bg-gray-900/50"></div>
          <div class="h-44 rounded-xl border border-gray-800 bg-gray-900/50"></div>
          <div class="h-44 rounded-xl border border-gray-800 bg-gray-900/50"></div>
        </div>

        <div v-else-if="libraries.length" class="grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-3">
          <button
            v-for="lib in libraries"
            :key="lib.key"
            type="button"
            data-testid="library-card"
            class="rounded-xl border border-stroke bg-white dark:bg-gray-800 p-5 dark:border-gray-700"
            @click="navigateToSeries(lib.key, lib.title)"
          >
            <div>
              <span class="mb-4 inline-flex h-12 w-12 items-center justify-center rounded-lg bg-gray-800 text-amber-400 border border-gray-700/50 group-hover:bg-gray-700">
                <AppIcon name="tv" size="lg" />
              </span>
              <h3 class="text-xl font-semibold text-white tracking-tight">{{ lib.title }}</h3>
            </div>
            <span class="mt-6 flex items-center gap-1 text-sm font-medium text-gray-400 group-hover:text-amber-400 transition-colors">
              Ver séries
              <span class="transform transition-transform group-hover:translate-x-0.5">›</span>
            </span>
          </button>
        </div>

        <div v-else class="rounded-xl border border-stroke bg-white dark:bg-gray-800 p-12 text-center dark:border-gray-700">
          <AppIcon name="tv" size="lg" class="mx-auto text-gray-500" />
          <p class="mt-3 text-sm font-medium text-gray-400">
            Nenhuma biblioteca de séries encontrada.
          </p>
        </div>
      </div>
    </div>
  </div>
</template>