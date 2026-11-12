<script setup>
import { onMounted } from "vue";
import LogFilterBar from "@/components/Logs/LogFilterBar.vue";
import LogGroupedView from "@/components/Logs/LogGroupedView.vue";
import { useLogsStore } from "@/stores/logs";

const logsStore = useLogsStore();

onMounted(() => {
  logsStore.fetchLogs();
  logsStore.fetchJobs();
});

async function handleClearLogs() {
  if (!window.confirm("Confirma limpeza de TODOS os logs? Esta ação não pode ser desfeita.")) return;
  try {
    await logsStore.clearLogs();
    window.alert("Logs limpos com sucesso");
  } catch (e) {
    window.alert("Erro ao limpar logs: " + e.message);
  }
}

async function handleCleanup() {
  const days = window.prompt("Retenção em dias (padrão 30):", "30");
  if (days === null) return;
  if (!days.match(/^\d+$/)) {
    window.alert("Valor inválido");
    return;
  }
  try {
    await logsStore.cleanupLogs(parseInt(days));
    window.alert("Limpeza de logs antigos concluída");
  } catch (e) {
    window.alert("Erro na limpeza: " + e.message);
  }
}
</script>

<template>
  <div class="mx-auto max-w-6xl px-4 py-3">
    <div class="flex items-center justify-between gap-3">
        <div>
          <h1 class="text-28 font-bold text-gray-900 dark:text-gray-100">Logs de processamento</h1>
          <p class="text-sm text-gray-500 dark:text-gray-400">Histórico de varreduras, filtros e agrupamento.</p>
        </div>
        <div class="flex items-center gap-2">
          <button
            data-testid="log-btn-clear"
            class="rounded-lg border border-gray-300 bg-gray-100 px-4 py-2 text-sm font-medium text-gray-800 transition hover:border-red-400 hover:text-red-600 dark:border-gray-600 dark:bg-gray-700 dark:text-gray-200"
            @click="handleClearLogs"
          >
            Limpar Logs
          </button>
          <button
            data-testid="log-btn-cleanup"
            class="rounded-lg border border-stroke px-4 py-2 text-sm font-medium text-gray-500 transition hover:border-primary hover:text-primary dark:border-strokedark dark:text-bodydark"
            @click="handleCleanup"
          >
            Limpar > 30 dias
          </button>
        </div>
      </div>

    <div class="p-4 md:p-6 lg:p-8 max-w-6xl mx-auto">
      <LogFilterBar />
      <LogGroupedView
        :items="logsStore.logs"
        :group-by="logsStore.groupBy"
        :loading="logsStore.loadingLogs"
      />
    </div>
  </div>
</template>