<script setup>
import { ref, onMounted, onUnmounted, computed } from "vue";
import { useI18n } from "vue-i18n";
import { apiFetch } from "../api";


const { t } = useI18n();

const status = ref("idle");
const processed = ref(0);
const skipped = ref(0);
const errors = ref(0);
const eventSource = ref(null);
const currentJobId = ref(null);
const logEntries = ref([]);
const loading = ref(false);
const lastSyncTime = ref(null);

const logLevelIcons = {
  info: "ui-flag",
  success: "ui-circle-check-big",
  skip: "ui-chevron-right",
  warning: "ui-triangle-alert",
  error: "ui-circle-x",
};

const formatLogTimestamp = (date) => {
  const locale = localStorage.getItem("app.locale") || "pt-BR";
  return date.toLocaleTimeString(locale === "pt-BR" ? "pt-BR" : "en-US", {
    hour: "2-digit",
    minute: "2-digit",
    second: "2-digit",
  });
};

const getLogIcon = (level) => {
  return logLevelIcons[level] || "ui-info";
};

const getLogColorClass = (level) => {
  switch (level) {
    case "success": return "text-success";
    case "warning": return "text-warning";
    case "error": return "text-danger";
    case "skip": return "text-muted";
    default: return "text-body";
  }
};

const startScan = async () => {
  loading.value = true;
  try {
    const response = await apiFetch("/api/plex/scan", { method: "POST" });
    if (response.ok) {
      const data = await response.json();
      currentJobId.value = data.job_id;
      connectSSE(data.job_id);
    }
  } catch (e) {
    console.error("Start scan failed:", e);
    loading.value = false;
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
    loading.value = false;
  };
};

const cancelScan = async () => {
  if (!currentJobId.value) return;
  await apiFetch(`/api/sync/cancel/${currentJobId.value}`, { method: "POST" });
  status.value = "cancelled";
  loading.value = false;
};

const clearConsole = () => {
  logEntries.value = [];
};

const fetchStatus = async () => {
  try {
    const res = await apiFetch("/api/sync/status");
    if (res.ok) {
      const data = await res.json();
      status.value = data.status || "idle";
      processed.value = data.processed || 0;
      skipped.value = data.skipped || 0;
      errors.value = data.errors || 0;
      if (data.lastRun) lastSyncTime.value = new Date(data.lastRun);
    }
  } catch (e) {
    console.error("Fetch status failed:", e);
  }
};

onMounted(async () => {
  await fetchStatus();
  if (status.value === "running") {
    // Try to reconnect SSE if there was a running job
    // In practice, job_id would need to be stored
  }
});

onUnmounted(() => {
  if (eventSource.value) eventSource.value.close();
});



const statusText = computed(() => {
  switch (status.value) {
    case "running": return t("sync.status.running");
    case "completed": return t("sync.status.completed");
    case "failed": return t("sync.status.failed");
    case "cancelled": return t("sync.status.cancelled");
    case "idle": return t("sync.status.idle");
    default: return status.value;
  }
});

const statusVariant = computed(() => {
  switch (status.value) {
    case "running": return "warning";
    case "completed": return "success";
    case "failed": return "danger";
    default: return "neutral";
  }
});

const isRunning = computed(() => status.value === "running");
</script>

<template>
  <div class="sync-view">
    <!-- HERO: type-led, 1 screen, data-enter -->
    <section class="hero hero--statement" data-enter aria-labelledby="sync-hero-h">
      <div class="wrap grid">
        <div class="hero-copy span-6">
          <h1 id="sync-hero-h">Sincronização</h1>
          <p class="t-lead">Sincronize marcadores de intro/créditos com o Plex. Escaneia bibliotecas, detecta novos episódios e gera banco SQLite para PlexAutoSkip.</p>
          <div class="btn-row">
            <button
              v-if="!isRunning"
              class="btn btn--primary"
              :disabled="loading"
              data-testid="sync-btn-start"
              @click="startScan"
            >
              <img src="/assets/icons/ui-play.svg" data-icon="ui-play" class="icon" width="20" height="20" aria-hidden="true" />
              Sincronizar Agora
            </button>
            <button
              v-if="isRunning"
              class="btn btn--secondary"
              :disabled="loading"
              data-testid="sync-btn-cancel"
              @click="cancelScan"
            >
              <img src="/assets/icons/ui-circle-x.svg" data-icon="ui-circle-x" class="icon" width="20" height="20" aria-hidden="true" />
              Cancelar Sincronização
            </button>
            <button class="btn btn--secondary" data-testid="sync-btn-clear" @click="clearConsole">
              <img src="/assets/icons/ui-square-pen.svg" data-icon="ui-square-pen" class="icon" width="20" height="20" aria-hidden="true" />
              Limpar Console
            </button>
          </div>
        </div>
        <div class="hero-visual span-6 start-7" aria-hidden="true">
          <div class="panel frame" data-fx="sync-mesh">
            <div class="stage at-corner" data-ui="sync-visual" :data-status="status" data-seed="sync"></div>
          </div>
        </div>
      </div>
    </section>

    <!-- SECTION: Status da Sincronização -->
    <section class="section" aria-labelledby="sync-status-h" style="--h: 2">
      <div class="wrap">
        <h2 id="sync-status-h" class="t-h2">Status da Sincronização</h2>

        <div class="sync-metrics grid" role="list" aria-label="Métricas de sincronização" style="margin-bottom: var(--s-8);">
          <article class="card span-4" role="listitem">
            <div class="metric">
              <span class="metric-label">Status</span>
              <div class="metric-value" :class="statusVariant">
                <img
                  :src="`/assets/icons/${{
                    running: 'ui-activity',
                    completed: 'ui-circle-check-big',
                    failed: 'ui-circle-x',
                    cancelled: 'ui-square',
                    idle: 'ui-clock',
                  }[status] || 'ui-clock'}.svg`"
                  :data-icon="{
                    running: 'ui-activity',
                    completed: 'ui-circle-check-big',
                    failed: 'ui-circle-x',
                    cancelled: 'ui-square',
                    idle: 'ui-clock',
                  }[status] || 'ui-clock'"
                  class="icon"
                  width="20"
                  height="20"
                  aria-hidden="true"
                />
                <span>{{ statusText }}</span>
              </div>
            </div>
          </article>
          <article class="card span-4" role="listitem">
            <div class="metric">
              <span class="metric-label">{{ t("sync.processed") }}</span>
              <div class="metric-value">{{ processed }}</div>
            </div>
          </article>
          <article class="card span-4" role="listitem">
            <div class="metric">
              <span class="metric-label">{{ t("sync.skipped") }}</span>
              <div class="metric-value text-muted">{{ skipped }}</div>
            </div>
          </article>
          <article class="card span-4" role="listitem">
            <div class="metric">
              <span class="metric-label">{{ t("sync.errors") }}</span>
              <div class="metric-value" :class="errors > 0 ? 'text-danger' : ''">{{ errors }}</div>
            </div>
          </article>
          <article class="card span-4" role="listitem">
            <div class="metric">
              <span class="metric-label">{{ t("sync.lastRun") }}</span>
              <div class="metric-value">{{ lastSyncTime ? formatLogTimestamp(lastSyncTime) : "—" }}</div>
            </div>
          </article>
        </div>

        <!-- Log Console -->
        <div class="log-console">
          <h3 class="t-h3" style="margin-bottom: var(--s-4);">{{ t("sync.logConsole") }}</h3>
          <div class="log-container font-mono text-sm space-y-2 max-h-96 overflow-y-auto" style="border-radius: var(--r-lg); background: var(--bg); border: 1px solid var(--border); padding: var(--s-4);">
            <div v-if="logEntries.length === 0" class="text-muted italic text-center py-8">
              {{ t("sync.noLogs") }}
            </div>
            <div v-for="(entry, index) in logEntries" :key="index" class="log-entry border-b border-gray-800/50 pb-2 last:border-0 last:pb-0">
              <div class="flex items-start gap-2">
                <span class="text-xs text-muted">{{ formatLogTimestamp(entry.timestamp) }}</span>
                <img :src="`/assets/icons/${getLogIcon(entry.level)}.svg`" :data-icon="getLogIcon(entry.level)" class="icon" width="14" height="14" aria-hidden="true" :class="getLogColorClass(entry.level)" />
                <span class="text-body flex-1">{{ entry.message }}</span>
              </div>
              <div v-if="entry.suggestion" class="text-xs text-warning mt-1 pl-6">
                <strong>{{ t("sync.suggestion") }}:</strong> {{ entry.suggestion }}
              </div>
            </div>
          </div>
        </div>
      </div>
    </section>
  </div>
</template>

<style scoped>
.sync-view {
  /* Page wrapper */
}

.hero {
  position: relative;
  min-height: calc(100vh - var(--nav-h, 64px));
  display: flex;
  align-items: center;
}

.hero-text {
  display: grid;
  grid-template-columns: repeat(12, minmax(0, 1fr));
  column-gap: var(--gutter);
  align-items: center;
  height: 100%;
}

.hero-copy {
  display: flex;
  flex-direction: column;
  justify-content: center;
}

.hero-copy h1 {
  font: var(--font-display);
  font-size: var(--t-h1);
  font-weight: var(--display-weight);
  line-height: 1.1;
  letter-spacing: var(--display-tracking);
  margin: 0 0 var(--s-4);
  color: var(--ink);
}

.hero-copy .t-lead {
  font-size: var(--t-lead);
  line-height: 1.6;
  color: var(--muted);
  margin: 0 0 var(--s-6);
  max-width: 60ch;
}

.btn-row {
  display: flex;
  gap: var(--s-3);
  flex-wrap: wrap;
}

.btn {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  gap: var(--s-2);
  padding: var(--s-2) var(--s-5);
  font: var(--font-text);
  font-size: var(--t-sm);
  font-weight: 500;
  border-radius: var(--r-btn);
  border: 1px solid transparent;
  cursor: pointer;
  transition: background var(--dur) var(--ease), border-color var(--dur) var(--ease), color var(--dur) var(--ease), transform var(--dur) var(--ease), box-shadow var(--dur) var(--ease);
}
.btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}
.btn:focus-visible {
  outline: 2px solid var(--action);
  outline-offset: 2px;
}

.btn--primary {
  background: var(--p-action);
  color: var(--p-action-ink);
  border-color: var(--p-action);
}
@media (hover: hover) and (pointer: fine) {
  .btn--primary:hover:not(:disabled) {
    background: color-mix(in oklab, var(--p-action) 84%, var(--bg));
    transform: translateY(-1px);
  }
}
.btn--primary:active:not(:disabled) {
  transform: translateY(1px) scale(0.985);
}

.btn--secondary {
  background: var(--surface);
  color: var(--ink);
  border-color: var(--border);
}
@media (hover: hover) and (pointer: fine) {
  .btn--secondary:hover:not(:disabled) {
    background: color-mix(in oklab, var(--bg) 92%, var(--ink));
    border-color: color-mix(in oklab, var(--ink) 32%, transparent);
    transform: translateY(-1px);
  }
}
.btn--secondary:active:not(:disabled) {
  transform: translateY(1px) scale(0.985);
}

.icon {
  display: inline-block;
  vertical-align: middle;
  flex-shrink: 0;
}

.hero-visual {
  display: flex;
  align-items: center;
  justify-content: center;
}

.frame {
  border-radius: var(--r-lg);
  overflow: hidden;
  background: var(--surface);
  border: 1px solid var(--border);
}

.section {
  padding: var(--padt) 0 calc(var(--padb) + var(--bar));
}

.t-h2 {
  font: var(--font-display);
  font-size: var(--t-h2);
  font-weight: 600;
  letter-spacing: var(--display-tracking);
  margin: 0 0 var(--s-6);
  color: var(--ink);
}

.t-h3 {
  font-family: var(--font-display);
  font-size: var(--t-h3);
  font-weight: 600;
  letter-spacing: var(--display-tracking);
  margin: 0 0 var(--s-4);
  color: var(--ink);
}

/* Sync metrics */
.sync-metrics {
  margin-top: var(--s-4);
}
.sync-metrics .card {
  padding: var(--s-4);
}
.metric {
  display: flex;
  flex-direction: column;
  gap: var(--s-1);
}
.metric-label {
  font-size: var(--t-small);
  font-weight: 500;
  color: var(--muted);
  text-transform: uppercase;
  letter-spacing: 0.05em;
}
.metric-value {
  font-family: var(--font-display);
  font-size: var(--t-h3);
  font-weight: 600;
  color: var(--ink);
  display: flex;
  align-items: center;
  gap: var(--s-2);
}
.metric-value.text-success { color: var(--success); }
.metric-value.text-warning { color: var(--warning); }
.metric-value.text-danger { color: var(--danger); }
.metric-value.text-muted { color: var(--muted); }

/* Log console */
.log-console {
  margin-top: var(--s-8);
}
.log-container {
  font-family: var(--font-mono);
}
.log-entry {
  display: flex;
  flex-direction: column;
  gap: var(--s-1);
}
.log-entry .icon {
  flex-shrink: 0;
  margin-top: 2px;
}

/* Card loops animation */
@media (prefers-reduced-motion: no-preference) {
  .sync-metrics .card > .panel,
  .hero .panel {
    animation: card-loop 3.2s ease-in-out infinite;
  }
  .sync-metrics .card:nth-child(2) > .panel { animation-delay: 0.8s; }
  .sync-metrics .card:nth-child(3) > .panel { animation-delay: 1.6s; }
  .sync-metrics .card:nth-child(4) > .panel { animation-delay: 2.4s; }
  .sync-metrics .card:nth-child(5) > .panel { animation-delay: 0.4s; }
}

@keyframes card-loop {
  0%, 100% { opacity: 1; transform: translateY(0); }
  50% { opacity: 0.9; transform: translateY(-4px); }
}

.sync-metrics .panel:hover,
.hero .panel:hover {
  animation-play-state: paused;
}

/* Utility classes */
.font-mono { font-family: var(--font-mono); }
.text-sm { font-size: var(--t-small); }
.space-y-2 > * + * { margin-top: var(--s-2); }
.max-h-96 { max-height: 24rem; }
.overflow-y-auto { overflow-y: auto; }
.border-b { border-bottom: 1px solid var(--border); }
.border-gray-800\/50 { border-color: color-mix(in oklab, var(--ink) 8%, transparent); }
.pb-2 { padding-bottom: var(--s-2); }
.last\:border-0:last-child { border-bottom: none; }
.last\:pb-0:last-child { padding-bottom: 0; }
.text-xs { font-size: var(--t-label); }
.text-muted { color: var(--muted); }
.flex { display: flex; }
.items-start { align-items: flex-start; }
.gap-2 { gap: var(--s-2); }
.flex-1 { flex: 1 1 0%; }
.mt-1 { margin-top: var(--s-1); }
.pl-6 { padding-left: var(--s-6); }
.text-center { text-align: center; }
.py-8 { padding-block: var(--s-8); }
.italic { font-style: italic; }
</style>