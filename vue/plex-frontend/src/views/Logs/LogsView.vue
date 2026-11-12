<script setup>
import { onMounted } from "vue";
import { useRouter } from "vue-router";
import LogFilterBar from "@/components/Logs/LogFilterBar.vue";
import LogGroupedView from "@/components/Logs/LogGroupedView.vue";
import { useLogsStore } from "@/stores/logs";
import { apiFetch } from "@/api";
import AppIcon from "@/components/AppIcon.vue";

const logsStore = useLogsStore();
const router = useRouter();

onMounted(() => {
  logsStore.fetchLogs();
  logsStore.fetchJobs();
});

async function handleStartScan() {
  try {
    const res = await apiFetch("/api/plex/scan", { method: "POST" });
    if (res.ok) {
      logsStore.fetchJobs();
      logsStore.fetchLogs();
      // Polling para atualizar logs de forma assíncrona enquanto roda
      const interval = setInterval(() => {
        logsStore.fetchJobs();
        logsStore.fetchLogs();
      }, 2000);
      setTimeout(() => clearInterval(interval), 30000); // Para após 30 segundos
    } else {
      window.alert("Erro ao iniciar escaneamento");
    }
  } catch (e) {
    window.alert("Erro ao iniciar escaneamento: " + e.message);
  }
}

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
  <div class="min-h-screen bg-black text-white">
    <div class="mx-auto max-w-6xl px-4 py-3">
      <div class="flex items-center justify-between gap-3">
        <div>
          <h1 class="text-2xl font-bold text-white">Logs de processamento</h1>
          <p class="text-sm text-gray-400">
            Histórico de varreduras, filtros e agrupamento.
          </p>
        </div>
        <div class="flex items-center gap-2">
          <button
            data-testid="log-btn-scan"
            class="rounded-lg bg-primary px-4 py-2 text-sm font-medium text-white transition hover:opacity-90"
            @click="handleStartScan"
          >
            <AppIcon name="play" class="mr-2 inline-block w-4 h-4" /> Iniciar Escaneamento
          </button>
          <button
            data-testid="log-btn-clear"
            class="rounded-lg border border-gray-700 bg-gray-800 px-4 py-2 text-sm font-medium text-gray-300 transition hover:border-gray-500 hover:text-white"
            @click="handleClearLogs"
          >
            Limpar Logs
          </button>
          <button
            data-testid="log-btn-cleanup"
            class="rounded-lg border border-gray-700 bg-gray-800 px-4 py-2 text-sm font-medium text-gray-300 transition hover:border-gray-500 hover:text-white"
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
  </div>
</template>
