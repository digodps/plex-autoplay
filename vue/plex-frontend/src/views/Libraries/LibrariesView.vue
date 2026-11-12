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
  <div class="min-h-screen bg-black text-white">
    <div class="mx-auto max-w-6xl px-4 py-3">
      <div class="flex items-center justify-between gap-3">
        <div>
          <h1 class="text-2xl font-bold text-white">Bibliotecas</h1>
          <p class="text-sm text-gray-400">
            Bibliotecas de séries detectadas no Plex.
          </p>
        </div>
        <div class="flex items-center gap-2">
          <button
            class="rounded-lg border border-gray-700 bg-gray-800 px-4 py-2 text-sm font-medium text-gray-300 transition hover:border-gray-500 hover:text-white"
            @click="loadLibraries"
          >
            Atualizar
          </button>
        </div>
      </div>

      <div class="p-4 md:p-6 lg:p-8 max-w-6xl mx-auto">
        <div v-if="error" class="rounded-lg border border-red-900 bg-red-900/20 p-4 text-sm text-red-300" role="alert">
          {{ error }}
        </div>

        <div v-if="loading" class="space-y-4 animate-pulse">
          <div class="h-24 rounded-lg border border-gray-800 bg-gray-800"></div>
          <div class="h-20 rounded-lg border border-gray-800 bg-gray-800"></div>
          <div class="h-16 rounded-lg border border-gray-800 bg-gray-800"></div>
        </div>

        <div v-else-if="libraries.length" class="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4">
          <button
            v-for="lib in libraries"
            :key="lib.key"
            type="button"
            class="group flex flex-col rounded-lg border border-gray-800 bg-gray-900 p-4 shadow-sm transition-colors hover:border-gray-600 hover:bg-gray-800"
            @click="navigateToSeries(lib.key)"
          >
            <span class="flex h-10 w-10 items-center justify-center rounded-lg bg-gray-800 text-yellow-500 transition group-hover:bg-yellow-500/20 mb-3">
              <AppIcon name="tv" size="lg" />
            </span>
            <h3 class="text-title-sm font-semibold text-white">{{ lib.title }}</h3>
            <span class="mt-1 flex items-center gap-1 text-sm font-medium text-gray-400">
              Ver séries
              <AppIcon name="chevron-right" size="xs" class="hidden sm:inline" />
            </span>
          </button>
        </div>

        <div v-else class="rounded-lg border border-gray-800 bg-gray-900 p-8 text-center">
          <AppIcon name="tv" size="lg" class="mx-auto text-gray-500" />
          <p class="mt-3 text-sm font-medium text-gray-400">
            Nenhuma biblioteca de séries encontrada.
          </p>
        </div>
      </div>
    </div>
  </div>
</template>