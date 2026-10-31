<script setup>
import { ref, onUnmounted } from "vue";
import { apiFetch } from "../api";

const status = ref("idle");
const processed = ref(0);
const skipped = ref(0);
const errors = ref(0);
const eventSource = ref(null);
const currentJobId = ref(null);

// Log entries from SSE
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
      // Keep only last 100 entries to prevent unlimited growth
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
  <div class="sync">
    <h1>Sincronização</h1>

    <div class="status">
      <span class="status-badge" :class="`status-${status}`">{{ status }}</span>
    </div>

    <div class="progress">
      <p>Processados: {{ processed }}</p>
      <p>Pulados: {{ skipped }}</p>
      <p>Erros: {{ errors }}</p>
    </div>

    <div class="log-panel">
      <div class="log-header">
        <h2>Console de Log</h2>
        <div class="log-actions">
          <button class="btn btn-sm btn-ghost" @click="clearConsole">Limpar Console</button>
        </div>
      </div>

      <div class="log-container">
        <div v-if="logEntries.length === 0" class="empty-log">
          Nenhum log disponível. Inicie uma varredura para ver as linhas de log.
        </div>
        <div v-for="(entry, index) in logEntries" :key="index" class="log-entry">
          <span class="log-timestamp">{{ formatLogTimestamp(entry.timestamp) }}</span>
          <span :class="getLogColorClass(entry.level)" class="log-icon">{{
            getLogIcon(entry.level)
          }}</span>
          <span class="log-message">{{ entry.message }}</span>
          <div v-if="entry.suggestion" class="log-suggestion">
            <strong>Sugestão:</strong> {{ entry.suggestion }}
          </div>
        </div>
      </div>
    </div>

    <div class="actions">
      <button v-if="status !== 'running'" @click="startScan">Iniciar Escaneamento</button>
      <button v-if="status === 'running'" class="cancel" @click="cancelScan">Cancelar</button>
    </div>
  </div>
</template>

<style scoped src="./sync-view.css"></style>
