<script setup>
import { ref, onUnmounted } from "vue";
import { useRouter } from "vue-router";
import { apiFetch } from "../api";
import AppIcon from "@/components/AppIcon.vue";

const router = useRouter();

const status = ref("idle");
const processed = ref(0);
const skipped = ref(0);
const errors = ref(0);
const eventSource = ref(null);
const currentJobId = ref(null);

const logEntries = ref([]);

const logLevelIcons = {
  info: "▶",
  success: "✔",
  skip: "⏭",
  warning: "⚠",
  error: "✖",
};

const logLevelColors = {
  info: "text-info",
  success: "text-success",
  skip: "text-secondary",
  warning: "text-warning",
  error: "text-danger",
};

const formatLogTimestamp = (date) => {
  return date.toLocaleTimeString("pt-BR", {
    hour: "2-digit",
    minute: "2-digit",
    second: "2-digit",
  });
};

const startScan = async () => {
  const response = await apiFetch("/api/plex/scan", {
    method: "POST",
  });
  if (response.ok) {
    const data = await response.json();
    currentJobId.value = data.job_id;
    connectSSE(data.job_id);
  }
};

const connectSSE = (jobId) => {
  if (eventSource.value) eventSource.value.close();
  eventSource.value = new EventSource(`/api/sync/log/stream/${jobId}`);

  eventSource.value.onmessage = (event) => {
    const data = JSON.parse(event.data);
    status.value = data.status;
    processed.value = data.processed;
    skipped.value = data.skipped;
    errors.value = data.errors || 0;

    if (data.logs && data.logs.length > 0) {
      const newLogs = data.logs.map((log) => ({
        timestamp: new Date(log.timestamp),
        level: log.level,
        message: log.message,
        suggestion: log.suggestion || null,
      }));
      logEntries.value = [...logEntries.value, ...newLogs];
      if (logEntries.value.length > 100) {
        logEntries.value = logEntries.value.slice(-100);
      }
    }
  };

  eventSource.value.onerror = () => {
    eventSource.value.close();
    status.value = "failed";
  };
};

const cancelScan = async () => {
  if (!currentJobId.value) return;
  await apiFetch(`/api/sync/cancel/${currentJobId.value}`, {
    method: "POST",
  });
  status.value = "cancelled";
};

const clearConsole = () => {
  logEntries.value = [];
};

const getLogIcon = (level) => {
  return logLevelIcons[level] || "▶";
};

const getLogColorClass = (level) => {
  return logLevelColors[level] || "text-body";
};

onUnmounted(() => {
  if (eventSource.value) eventSource.value.close();
});
</script>

<template>
  <div class="min-h-screen bg-black text-white">
    <div class="mx-auto max-w-6xl px-4 py-3">
      <div class="flex items-center justify-between gap-3">
        <div>
          <h1 class="text-2xl font-bold text-white">Sincronização</h1>
          <p class="text-sm text-gray-400">
            Varredura do Plex, progresso em tempo real e console de logs.
          </p>
        </div>
        <div class="flex items-center gap-2">
          <button
            v-if="status !== 'running'"
            class="rounded-lg bg-primary px-4 py-2 text-sm font-medium text-white transition hover:opacity-90"
            @click="startScan"
          >
            Iniciar Escaneamento
          </button>
          <button
            v-if="status === 'running'"
            class="rounded-lg border border-red-500 bg-red-500/10 px-4 py-2 text-sm font-medium text-red-400 transition hover:bg-red-500/20"
            @click="cancelScan"
          >
            Cancelar
          </button>
          <button
            class="rounded-lg border border-gray-700 bg-gray-800 px-4 py-2 text-sm font-medium text-gray-300 transition hover:border-gray-500 hover:text-white"
            @click="clearConsole"
          >
            Limpar Console
          </button>
        </div>
      </div>

      <div class="p-4 md:p-6 lg:p-8 max-w-6xl mx-auto space-y-6">
        <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
          <div class="rounded-xl border border-gray-800 bg-gray-900 p-4">
            <span class="text-xs text-gray-400 uppercase tracking-wider">Status</span>
            <div class="mt-2 text-lg font-semibold uppercase" :class="status === 'running' ? 'text-yellow-400' : status === 'completed' ? 'text-green-400' : 'text-gray-200'">
              {{ status }}
            </div>
          </div>
          <div class="rounded-xl border border-gray-800 bg-gray-900 p-4">
            <span class="text-xs text-gray-400 uppercase tracking-wider">Processados / Pulados / Erros</span>
            <div class="mt-2 text-lg font-semibold text-gray-200">
              {{ processed }} / {{ skipped }} / {{ errors }}
            </div>
          </div>
        </div>

        <section class="rounded-xl border border-gray-800 bg-gray-900 p-5">
          <h2 class="text-sm font-semibold uppercase tracking-wider text-gray-400 mb-4 border-b border-gray-800 pb-2">
            Console de Log
          </h2>
          <div class="log-container font-mono text-sm space-y-2 max-h-96 overflow-y-auto">
            <div v-if="logEntries.length === 0" class="text-gray-500 italic text-center py-8">
              Nenhum log disponível. Inicie uma varredura para ver as linhas de log.
            </div>
            <div v-for="(entry, index) in logEntries" :key="index" class="border-b border-gray-800/50 pb-2">
              <div class="flex items-start gap-2">
                <span class="text-xs text-gray-500">{{ formatLogTimestamp(entry.timestamp) }}</span>
                <span :class="getLogColorClass(entry.level)">{{ getLogIcon(entry.level) }}</span>
                <span class="text-gray-300 flex-1">{{ entry.message }}</span>
              </div>
              <div v-if="entry.suggestion" class="text-xs text-red-400 mt-1 pl-6">
                <strong>Sugestão:</strong> {{ entry.suggestion }}
              </div>
            </div>
          </div>
        </section>
      </div>
    </div>
  </div>
</template>
