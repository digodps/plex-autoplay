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
    <div class="mb-6 flex flex-wrap items-end justify-between gap-4">
      <div>
        <h1 class="text-title-md font-bold text-black dark:text-white">Visão geral</h1>
        <p class="mt-1 text-sm font-medium text-body dark:text-bodydark">
          Saúde do serviço, sincronização, bibliotecas e marcadores.
        </p>
      </div>
      <div class="flex flex-wrap items-center gap-3">
        <button
          class="rounded-lg border border-stroke bg-white px-4 py-2.5 text-sm font-medium text-body transition hover:border-primary hover:text-primary dark:border-strokedark dark:bg-boxdark dark:text-bodydark"
          :disabled="store.isLoading"
          @click="router.push('/sync')"
        >
          Ver Log Completo
        </button>
        <button
          class="flex items-center gap-2 rounded-lg bg-primary px-4 py-2.5 text-sm font-medium text-white transition hover:bg-opacity-90 disabled:opacity-60"
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
      class="mt-6 flex flex-wrap items-center gap-3 rounded-sm border border-meta-1/30 bg-meta-1/10 px-5 py-4 text-sm text-meta-1"
      role="alert"
    >
      <AppIcon name="warning" size="lg" />
      <p class="flex-1">{{ store.error }}</p>
      <button
        class="rounded-lg border border-meta-1/40 px-3 py-1.5 font-medium transition hover:bg-meta-1/10"
        @click="store.retry"
      >
        Tentar novamente
      </button>
    </div>

    <div
      v-else-if="store.libraryError"
      class="mt-6 rounded-sm border border-meta-6/30 bg-meta-6/10 px-5 py-4 text-sm text-body dark:text-bodydark"
      role="alert"
    >
      <div class="flex flex-wrap items-center gap-3">
        <AppIcon name="warning" size="lg" class="text-meta-6" />
        <p class="flex-1 font-medium text-meta-6">{{ store.libraryError }}</p>
        <button
          class="rounded-lg border border-meta-6/40 px-3 py-1.5 font-medium text-meta-6 transition hover:bg-meta-6/10"
          @click="store.retry"
        >
          Tentar novamente
        </button>
      </div>
      <p class="mt-2 text-xs text-body dark:text-bodydark">
        Verifique se o servidor Plex está acessível, se a URL e o token (X-Plex-Token) estão
        configurados corretamente e se há conectividade de rede.
      </p>
    </div>
  </DefaultLayout>
</template>