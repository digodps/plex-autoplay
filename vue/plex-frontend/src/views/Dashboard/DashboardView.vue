<script setup>
import { onMounted, onUnmounted } from "vue";
import { useRouter } from "vue-router";
import DefaultLayout from "@/components/layouts/DefaultLayout.vue";
import DataStatsOne from "@/components/DataStats/DataStatsOne.vue";
import AppIcon from "@/components/AppIcon.vue";
import { useDashboardStore } from "@/stores/dashboard";
import { useDashboardActions } from "@/composables/useDashboardActions";

const router = useRouter();
const store = useDashboardStore();
const { startScan } = useDashboardActions();

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
    <div class="mb-6 flex flex-col sm:flex-row sm:items-end justify-between gap-6">
      <div>
        <h1 class="text-2xl font-bold text-black dark:text-white">Visão geral</h1>
        <p class="mt-2 text-sm font-medium text-gray-600 dark:text-bodydark">
          Saúde do serviço, sincronização, bibliotecas e marcadores.
        </p>
      </div>

      <div class="flex flex-col sm:flex-row items-end gap-3">
        <button
          class="rounded-xl border border-stroke bg-white px-5 py-2.5 text-sm font-medium text-gray-600 transition hover:border-primary hover:text-primary focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-primary focus-visible:ring-offset-2 disabled:opacity-60 dark:border-gray-700 dark:bg-boxdark dark:text-bodydark dark:focus-visible:ring-offset-gray-900"
          :disabled="store.isLoading"
          @click="router.push('/sync')"
        >
          Ver Log Completo
        </button>
        <button
          class="flex items-center justify-center rounded-xl bg-primary px-5 py-2.5 text-sm font-medium text-white transition hover:bg-opacity-90 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-primary focus-visible:ring-offset-2 disabled:opacity-60 dark:focus-visible:ring-offset-gray-900"
          :disabled="store.isLoading"
          @click="handleStartScan"
        >
          <AppIcon name="play" size="sm" />
          Iniciar Escaneamento
        </button>
      </div>
    </div>

    <DataStatsOne />

    <div
      v-if="store.isTotalFailure"
      class="mt-8 rounded-2xl border border-meta-1/30 bg-meta-1/10 px-6 py-4 text-sm text-meta-1"
      role="alert"
    >
      <AppIcon name="warning" size="lg" class="mr-3" />
      <p class="flex-1">{{ store.error }}</p>
      <button
        class="rounded-xl border border-meta-1/40 px-4 py-2 font-medium transition hover:bg-meta-1/10"
        @click="store.retry"
      >
        Tentar novamente
      </button>
    </div>

    <div
      v-else-if="store.libraryError"
      class="mt-8 rounded-2xl rounded-tl-0 rounded-tr-0 border border-meta-6/30 bg-meta-6/10 px-6 py-4 text-sm text-body dark:text-bodydark"
      role="alert"
    >
      <div class="flex flex-wrap items-center gap-3">
        <AppIcon name="warning" size="lg" class="text-meta-6" />
        <p class="flex-1 font-medium text-meta-6">{{ store.libraryError }}</p>
        <button
          class="rounded-xl border border-meta-6/40 px-4 py-2 font-medium text-meta-6 transition hover:bg-meta-6/10"
          @click="store.retry"
        >
          Tentar novamente
        </button>
      </div>
      <p class="mt-3 text-xs text-body dark:text-bodydark">
        Verifique se o servidor Plex está acessível, se a URL e o token (X-Plex-Token) estão
        configurados corretamente e se há conectividade de rede.
      </p>
    </div>
  </DefaultLayout>
</template>