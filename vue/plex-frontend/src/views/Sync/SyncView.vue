<script setup>
import { ref, onUnmounted } from "vue";
import DefaultLayout from "@/components/layouts/DefaultLayout.vue";
import AppIcon from "@/components/AppIcon.vue";
import { apiFetch } from "@/api";

const status = ref("idle");
const processed = ref(0);
const skipped = ref(0);
const errors = ref(0);
const eventSource = ref(null);
const currentJobId = ref(null);
const logEntries = ref([]);

const logLevelIcons = { info: "▶", success: "✔", skip: "⏭", warning: "⚠", error: "✖" };
const logLevelColors = {
  info: "text-gray-500 dark:text-gray-400",
  success: "text-gray-600 dark:text-gray-300",
  skip: "text-gray-500 dark:text-gray-400",
  warning: "text-gray-600 dark:text-gray-300",
  error: "text-meta-1 dark:text-gray-200",
};

const statusClass = {
  idle: "bg-gray-2 text-body dark:bg-meta-4 dark:text-bodydark",
  running: "bg-meta-6/10 text-meta-6",
  completed: "bg-meta-3/10 text-meta-3",
  failed: "bg-meta-1/10 text-meta-1",
  cancelled: "bg-gray-2 text-body dark:bg-meta-4 dark:text-bodydark",
};

const formatLogTimestamp = (date) =>
  date.toLocaleTimeString("pt-BR", { hour: "2-digit", minute: "2-digit", second: "2-digit" });

async function startScan() {
  const response = await apiFetch("/api/plex/scan", { method: "POST" });
  if (response.ok) {
    const data = await response.json();
    currentJobId.value = data.job_id;
    connectSSE(data.job_id);
  }
}

const terminalStatuses = ["completed", "failed", "cancelled"];

function connectSSE(jobId) {
  if (eventSource.value) eventSource.value.close();
  eventSource.value = new EventSource(`/api/sync/log/stream/${jobId}`);

  eventSource.value.onmessage = (event) => {
    const data = JSON.parse(event.data);
    status.value = data.status;
    processed.value = data.processed;
    skipped.value = data.skipped;
    errors.value = data.errors || 0;

    if (data.logs && data.logs.length > 0) {
      logEntries.value = data.logs.slice(-100).map((log) => ({
        timestamp: new Date(log.timestamp),
        level: log.level,
        message: log.message,
        suggestion: log.suggestion || null,
      }));
    }

    if (terminalStatuses.includes(data.status)) eventSource.value?.close();
  };

  eventSource.value.onerror = () => {
    eventSource.value?.close();
    if (!terminalStatuses.includes(status.value)) status.value = "failed";
  };
}

async function cancelScan() {
  if (!currentJobId.value) return;
  await apiFetch(`/api/sync/cancel/${currentJobId.value}`, { method: "POST" });
  status.value = "cancelled";
}

function clearConsole() {
  logEntries.value = [];
}

onUnmounted(() => {
  if (eventSource.value) eventSource.value.close();
});
</script>

<template>
  <DefaultLayout>
    <div>
      <div>
        <div class="max-w-2xl mx-auto px-4 py-3 flex items-center gap-2">
          <h1 class="text-28 font-bold text-gray-900 dark:text-gray-100">Sincronização</h1>
          <p class="text-sm text-gray-500 dark:text-gray-400">Execução e log em tempo real do último escaneamento.</p>
        </div>
      </div>

      <div class="p-4 md:p-6 lg:p-8">
        <!-- Status e controles -->
        <div class="rounded-lg border border-gray-200 dark:border-gray-600 bg-white dark:bg-gray-800 p-4 mb-4">
          <div class="grid grid-cols-2 gap-4">
            <div>
              <p class="text-xs font-medium uppercase tracking-wide text-gray-500 dark:text-gray-400">Status</p>
              <span
                class="mt-1 inline-flex rounded-full px-2.5 py-0.5 text-sm font-semibold capitalize"
                :class="statusClass[status] ?? statusClass.idle"
              >
                {{ status }}
              </span>
            </div>
            <div>
              <p class="text-xs font-medium uppercase tracking-wide text-gray-500 dark:text-gray-400">Processados</p>
              <p class="mt-1 text-title-sm font-bold text-gray-900 dark:text-gray-100">{{ processed }}</p>
            </div>
            <div>
              <p class="text-xs font-medium uppercase tracking-wide text-gray-500 dark:text-gray-400">Pulados</p>
              <p class="mt-1 text-title-sm font-bold text-gray-900 dark:text-gray-100">{{ skipped }}</p>
            </div>
            <div>
              <p class="text-xs font-medium uppercase tracking-wide text-gray-500 dark:text-gray-400">Erros</p>
              <p class="mt-1 text-title-sm font-bold" :class="errors > 0 ? 'text-meta-1' : 'text-gray-900 dark:text-gray-100'">
                {{ errors }}
              </p>
            </div>
          </div>
        </div>

        <!-- Controles -->
        <div class="rounded-lg border border-gray-200 dark:border-gray-600 bg-white dark:bg-gray-800 p-4 mb-6">
          <div class="flex items-center gap-3">
            <button
              v-if="status !== 'running'"
              class="flex items-center gap-2 rounded-lg bg-primary px-4 py-2.5 text-sm font-medium text-white transition hover:bg-opacity-90"
              @click="startScan"
            >
              <AppIcon name="play" size="sm" />
              Iniciar Escaneamento
            </button>
            <button
              v-else
              class="rounded-lg border border-meta-1/40 bg-meta-1/10 px-4 py-2.5 text-sm font-medium text-meta-1 transition hover:bg-meta-1/20"
              @click="cancelScan"
            >
              Cancelar
            </button>
            <button
              class="rounded-lg border border-gray-300 dark:border-gray-600 bg-gray-100 dark:bg-gray-700 px-4 py-2.5 text-sm font-medium text-gray-800 dark:text-gray-100 transition hover:border-gray-400 dark:hover:bg-gray-600"
              @click="clearConsole"
            >
              Limpar Console
            </button>
          </div>
        </div>

        <!-- Console de Log -->
        <div class="rounded-lg border border-gray-200 dark:border-gray-600 bg-white dark:bg-gray-800">
          <div class="flex items-center justify-between border-b border-gray-200 dark:border-gray-600 px-6 py-4">
            <h2 class="text-title-sm font-semibold text-gray-900 dark:text-gray-100">Console de Log</h2>
            <button
              class="rounded-lg border border-stroke px-3 py-1.5 text-sm font-medium text-gray-500 dark:text-gray-400 transition hover:border-primary hover:text-primary dark:border-strokedark dark:text-bodydark"
              @click="clearConsole"
            >
              Limpar Console
            </button>
          </div>

          <div class="max-h-550 min-h-70 overflow-y-auto p-4 font-mono text-sm no-scrollbar overscroll-contain">
            <div
              v-if="logEntries.length === 0"
              class="flex h-full min-h-35 items-center justify-center px-6 text-center text-gray-500 dark:text-gray-400"
            >
              Nenhum log disponível. Inicie uma varredura para ver as linhas de log.
            </div>
            <div v-else class="space-y-1">
              <div
                v-for="(entry, index) in logEntries" :key="index"
                class="flex flex-wrap gap-2 leading-relaxed text-sm"
              >
                <span class="flex-none text-xs text-gray-500 dark:text-gray-400">{{ formatLogTimestamp(entry.timestamp) }}</span>
                <span
                  class="flex-none text-xs capitalize"
                  :class="logLevelColors[entry.level] || 'text-gray-500 dark:text-gray-400'"
                >
                  {{ logLevelIcons[entry.level] || "▶" }}
                </span>
                <span class="text-gray-900 dark:text-gray-100">{{ entry.message }}</span>
                <div v-if="entry.suggestion" class="w-full pl-14 text-xs text-gray-600 dark:text-gray-300">
                  <strong>Sugestão:</strong> {{ entry.suggestion }}
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
      </div>
    </DefaultLayout>
  </template>