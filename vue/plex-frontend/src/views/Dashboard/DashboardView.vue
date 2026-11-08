<script setup>
import { onMounted, onUnmounted } from "vue";
import { useRouter } from "vue-router";
import DefaultLayout from "@/components/layouts/DefaultLayout.vue";
import DataStatsOne from "@/components/DataStats/DataStatsOne.vue";
import { useDashboardStore } from "@/stores/dashboard";
import { useDashboardActions } from "@/composables/useDashboardActions";
import { cycleMode, useThemeStore } from "@/stores/theme";

const router = useRouter();
const store = useDashboardStore();
const { startScan } = useDashboardActions();
const { mode, cycleMode: cycleTheme } = useThemeStore();

onMounted(() => {
  store.fetchAll();
  store.startPolling();
});

onUnmounted(() => {
  store.stopPolling();
});

async function handleStartScan() {
  const result = await startScan();
  if (result.ok) router.push("/sync");
  else console.error("Falha ao iniciar escaneamento:", result.error);
}
</script>

<template>
  <DefaultLayout>
    <div class="bg-gray-50 dark:bg-gray-900 min-h-screen">
      <div class="border-b border-gray-200 dark:border-gray-800 sticky top-0 z-20 bg-gray-50 dark:bg-gray-900">
        <div class="max-w-2xl mx-auto px-4 py-3 flex items-center gap-2">
          <h1 class="text-28 font-bold text-gray-900 dark:text-gray-100">Visão geral</h1>
          <p class="text-sm text-gray-500 dark:text-gray-400">
            Saúde do serviço, sincronização, bibliotecas e marcadores.
          </p>
        </div>
      </div>

      <div class="p-4 md:p-6 lg:p-8 flex flex-col gap-6">
        <DataStatsOne />

        <div
          v-if="store.isTotalFailure"
          class="rounded-lg border border-gray-200 dark:border-gray-800 bg-gray-900 dark:bg-gray-800 p-4 text-sm"
          role="alert"
        >
          <svg
            class="w-5 h-5 text-gray-400 dark:text-gray-600"
            fill="none"
            stroke="currentColor"
            viewBox="0 0 24 24"
          >
            <path
              stroke-linecap="round"
              stroke-linejoin="round"
              stroke-width="2"
              d="M12 9v2m0 4h.01m-6.938 4h13.856c1.535-3 0-3.506 0-5 0-1.506.376-2.926 1.029-4.2m-6.938 4L12 9m-6.938 4h13.856c1.535-3 0-3.506 0-5 0-1.506.376-2.926 1.029-4.2m6.938-4H3m6.938 4h13.856c1.535-3 0-3.506 0-5 0-1.506.376-2.926 1.029-4.2"
            />
          </svg>
          <span class="ml-2">{{ store.error }}</span>
          <button
            class="mt-2 rounded-lg border border-gray-300 dark:border-gray-600 bg-gray-100 dark:bg-gray-800 px-3 py-1.5 text-sm font-medium text-gray-800 dark:text-gray-100 transition hover:bg-gray-200 dark:hover:bg-gray-700"
            @click="store.retry"
          >
            Tentar novamente
          </button>
        </div>

        <div
          v-else-if="store.libraryError"
          class="rounded-lg border border-gray-200 dark:border-gray-600 bg-gray-900 dark:bg-gray-800 p-4 text-sm"
          role="alert"
        >
          <svg
            class="w-5 h-5 text-gray-400 dark:text-gray-600"
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
          <span class="ml-2 font-medium text-gray-300 dark:text-gray-400">{{ store.libraryError }}</span>
          <button
            class="mt-2 rounded-lg border border-gray-300 dark:border-gray-600 bg-gray-100 dark:bg-gray-800 px-3 py-1.5 text-sm font-medium text-gray-800 dark:text-gray-100 transition hover:bg-gray-200 dark:hover:bg-gray-700"
            @click="store.retry"
          >
            Tentar novamente
          </button>
          <p class="mt-2 text-xs text-gray-500 dark:text-gray-400">
            Verifique se o servidor Plex está acessível, se a URL e o token (X-Plex-Token) estão
            configurados corretamente e se há conectividade de rede.
          </p>
        </div>
      </div>
    </DefaultLayout>
  </template>