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
    <div>
      <div>
        <div class="max-w-2xl mx-auto px-4 py-3 flex items-center gap-2">
          <h1 class="text-28 font-bold text-gray-900 dark:text-gray-100">Bibliotecas</h1>
          <p class="text-sm text-gray-500 dark:text-gray-400">
            Bibliotecas de séries detectadas no Plex.
          </p>
        </div>
      </div>

      <div class="p-4 md:p-6 lg:p-8 flex flex-col gap-6">
        <div
          v-if="error"
          class="rounded-lg border border-gray-200 dark:border-gray-600 bg-gray-900 dark:bg-gray-800 p-4 text-sm"
          role="alert"
        >
          <svg
            class="w-4 h-4 text-gray-400 dark:text-gray-600"
            fill="none"
            stroke="currentColor"
            viewBox="0 0 24 24"
          >
            <path
              stroke-linecap="round"
              stroke-linejoin="round"
              stroke-width="2"
              d="M12 9v2m0 4h.01m-6.938 4h13.856c1.535-3 0-3.506 0-5 0-1.506.376-2.926 1.029-4.2m-6.938 4L12 9m-6.938 4h13.856c1.535-3 0-3.506 0-5 0-1.506.376-2.926 1.029-4.2"
            />
          </svg>
          <span class="ml-2">{{ error }}</span>
        </div>

        <div
          v-if="loading"
          class="space-y-4 animate-pulse"
        >
          <div
            class="h-24 rounded-lg border border-gray-200 dark:border-gray-700 bg-gray-100 dark:bg-gray-800"
          ></div>
          <div
            class="h-20 rounded-lg border border-gray-200 dark:border-gray-700 bg-gray-100 dark:bg-gray-800"
          ></div>
          <div
            class="h-16 rounded-lg border border-gray-200 dark:border-gray-700 bg-gray-100 dark:bg-gray-800"
          ></div>
        </div>

        <div
          v-else-if="libraries.length"
          class="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4"
        >
          <button
            v-for="lib in libraries"
            :key="lib.key"
            type="button"
            class="group flex flex-col rounded-lg border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-800 p-4 shadow-sm transition-colors hover:border-primary dark:hover:text-primary"
            @click="navigateToSeries(lib.key)"
          >
            <span
              class="flex h-10 w-10 items-center justify-center rounded-lg bg-gray-100 dark:bg-gray-700 text-primary transition group-hover:bg-primary dark:group-hover:text-white mb-3"
            >
              <AppIcon name="tv" size="lg" />
            </span>
            <h3 class="text-title-sm font-semibold text-gray-900 dark:text-gray-100">{{ lib.title }}</h3>
            <span class="mt-1 flex items-center gap-1 text-sm font-medium text-gray-600 dark:text-gray-400">
              Ver séries
              <AppIcon name="chevron-right" size="xs" class="hidden sm:inline" />
            </span>
          </button>
        </div>

        <div
          v-else
          class="rounded-lg border border-gray-200 dark:border-gray-700 bg-gray-900 dark:bg-gray-800 p-8 text-center"
        >
          <AppIcon name="tv" size="lg" class="mx-auto text-gray-400 dark:text-gray-600" />
          <p class="mt-3 text-sm font-medium text-gray-300 dark:text-gray-400">
            Nenhuma biblioteca de séries encontrada.
          </p>
        </div>
      </div>
      </div>
    </DefaultLayout>
  </template>