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
  info: "text-meta-5",
  success: "text-meta-3",
  skip: "text-bodydark2",
  warning: "text-meta-6",
  error: "text-meta-1",
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
      const newLogs = data.logs.map((log) => ({
        timestamp: new Date(log.timestamp),
        level: log.level,
        message: log.message,
        suggestion: log.suggestion || null,
      }));
      logEntries.value = [...logEntries.value, ...newLogs].slice(-100);
    }
  };

  eventSource.value.onerror = () => {
    eventSource.value?.close();
    status.value = "failed";
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
    <div class="mb-6 flex flex-wrap items-end justify-between gap-4">
      <div>
        <h1 class="text-title-md font-bold text-black dark:text-white">Sincronização</h1>
        <p class="mt-1 text-sm font-medium text-body dark:text-bodydark">
          Execução e log em tempo real do último escaneamento.
        </p>
      </div>
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
      </div>
    </div>

    <div class="mb-6 grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-4">
      <div class="rounded-sm border border-stroke bg-white px-5 py-4 shadow-default dark:border-strokedark dark:bg-boxdark">
        <p class="text-xs font-medium uppercase tracking-wide text-body dark:text-bodydark">Status</p>
        <span
          class="mt-2 inline-flex rounded-full px-2.5 py-0.5 text-sm font-semibold capitalize"
          :class="statusClass[status] ?? statusClass.idle"
        >
          {{ status }}
        </span>
      </div>
      <div class="rounded-sm border border-stroke bg-white px-5 py-4 shadow-default dark:border-strokedark dark:bg-boxdark">
        <p class="text-xs font-medium uppercase tracking-wide text-body dark:text-bodydark">Processados</p>
        <p class="mt-1 text-title-sm font-bold text-black dark:text-white">{{ processed }}</p>
      </div>
      <div class="rounded-sm border border-stroke bg-white px-5 py-4 shadow-default dark:border-strokedark dark:bg-boxdark">
        <p class="text-xs font-medium uppercase tracking-wide text-body dark:text-bodydark">Pulados</p>
        <p class="mt-1 text-title-sm font-bold text-black dark:text-white">{{ skipped }}</p>
      </div>
      <div class="rounded-sm border border-stroke bg-white px-5 py-4 shadow-default dark:border-strokedark dark:bg-boxdark">
        <p class="text-xs font-medium uppercase tracking-wide text-body dark:text-bodydark">Erros</p>
        <p class="mt-1 text-title-sm font-bold" :class="errors > 0 ? 'text-meta-1' : 'text-black dark:text-white'">
          {{ errors }}
        </p>
      </div>
    </div>

    <div class="rounded-sm border border-stroke bg-white shadow-default dark:border-strokedark dark:bg-boxdark">
      <div class="flex items-center justify-between border-b border-stroke px-6 py-4 dark:border-strokedark">
        <h2 class="text-title-sm font-semibold text-black dark:text-white">Console de Log</h2>
        <button
          class="rounded-lg border border-stroke px-3 py-1.5 text-sm font-medium text-body transition hover:border-primary hover:text-primary dark:border-strokedark dark:text-bodydark"
          @click="clearConsole"
        >
          Limpar Console
        </button>
      </div>

      <div class="max-h-550 min-h-70 overflow-y-auto p-4 font-mono text-sm">
        <div
          v-if="logEntries.length === 0"
          class="flex h-full min-h-35 items-center justify-center px-6 text-center text-body dark:text-bodydark"
        >
          Nenhum log disponível. Inicie uma varredura para ver as linhas de log.
        </div>
        <div v-else class="space-y-1">
          <div v-for="(entry, index) in logEntries" :key="index" class="flex flex-wrap gap-2 leading-relaxed">
            <span class="flex-none text-xs text-body dark:text-bodydark">{{ formatLogTimestamp(entry.timestamp) }}</span>
            <span class="flex-none" :class="logLevelColors[entry.level] || 'text-body'">
              {{ logLevelIcons[entry.level] || "▶" }}
            </span>
            <span class="text-black dark:text-white">{{ entry.message }}</span>
            <div v-if="entry.suggestion" class="w-full pl-14 text-xs text-meta-6">
              <strong>Sugestão:</strong> {{ entry.suggestion }}
            </div>
          </div>
        </div>
      </div>
    </div>
  </DefaultLayout>
</template>