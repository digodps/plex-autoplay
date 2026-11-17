<script setup>
import { onMounted, ref, computed } from "vue";
import { useI18n } from "vue-i18n";
import LogFilterBar from "@/components/Logs/LogFilterBar.vue";
import LogCarouselView from "@/components/Logs/LogCarouselView.vue";
import { useLogsStore } from "@/stores/logs";
import { apiFetch } from "@/api";

const { t } = useI18n();
const logsStore = useLogsStore();
const loading = ref(true);


onMounted(() => {
  logsStore.fetchLogs();
  logsStore.fetchJobs();
  loading.value = false;
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
      window.alert(t("logs.syncFailed"));
    }
  } catch (e) {
    window.alert(t("logs.syncFailed") + ": " + e.message);
  }
}

async function handleClearLogs() {
  if (!window.confirm(t("logs.confirmClearLogs"))) return;
  try {
    await logsStore.clearLogs();
    window.alert(t("logs.logsCleared"));
  } catch (e) {
    window.alert(t("errors.generic") + ": " + e.message);
  }
}

async function handleCleanup() {
  const days = window.prompt(t("logs.cleanupPrompt"), "30");
  if (days === null) return;
  if (!days.match(/^\d+$/)) {
    window.alert(t("errors.validation"));
    return;
  }
  try {
    await logsStore.cleanupLogs(parseInt(days));
    window.alert(t("logs.cleanupComplete"));
  } catch (e) {
    window.alert(t("errors.generic") + ": " + e.message);
  }
}

const jobRunning = computed(() => logsStore.jobs.some((j) => j.status === "running"));
</script>

<template>
  <div class="logs-view">
    <!-- HERO: type-led, 1 screen, data-enter -->
    <section class="hero hero--statement" data-enter aria-labelledby="logs-hero-h">
      <div class="wrap grid">
        <div class="hero-copy span-6">
          <h1 id="logs-hero-h">Logs</h1>
          <p class="t-lead">Acompanhe o processamento de escaneamentos, geração de marcadores e erros do sistema.</p>
          <div class="btn-row">
            <button
              class="btn btn--primary"
              :disabled="jobRunning"
              data-testid="log-btn-scan"
              @click="handleStartScan"
            >
              <img src="/assets/icons/ui-play.svg" data-icon="ui-play" class="icon" width="20" height="20" aria-hidden="true" />
              Iniciar Escaneamento
            </button>
            <button class="btn btn--secondary" data-testid="log-btn-clear" @click="handleClearLogs">
              <img src="/assets/icons/ui-square-pen.svg" data-icon="ui-square-pen" class="icon" width="20" height="20" aria-hidden="true" />
              Limpar Logs
            </button>
            <button class="btn btn--secondary" data-testid="log-btn-cleanup" @click="handleCleanup">
              <img src="/assets/icons/ui-folder-open.svg" data-icon="ui-folder-open" class="icon" width="20" height="20" aria-hidden="true" />
              Limpar Antigos
            </button>
          </div>
        </div>
        <div class="hero-visual span-6 start-7" aria-hidden="true">
          <div class="panel frame" data-fx="log-stream">
            <div class="stage at-corner" data-ui="log-stream-visual" data-seed="logs"></div>
          </div>
        </div>
      </div>
    </section>

    <!-- SECTION: Logs de Processamento -->
    <section class="section" aria-labelledby="logs-h" style="--h: 2">
      <div class="wrap">
        <h2 id="logs-h" class="t-h2">Logs de Processamento</h2>

        <div v-if="loading" class="logs-loading" style="grid-column: 1 / -1;">
          <div class="skeleton" style="height: 200px; border-radius: var(--r-lg);"></div>
        </div>

        <div v-else>
          <LogFilterBar />
          <LogCarouselView
            :items="logsStore.logs"
            :group-by="logsStore.groupBy"
            :loading="logsStore.loadingLogs"
          />
        </div>
      </div>
    </section>
  </div>
</template>

<style scoped>
.logs-view {
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

/* Loading skeleton */
.logs-loading {
  padding: var(--s-6) 0;
}
.skeleton {
  background: linear-gradient(90deg, var(--card) 25%, var(--surface) 50%, var(--card) 75%);
  background-size: 200% 100%;
  animation: shimmer 1.5s infinite;
}
@keyframes shimmer {
  0% { background-position: 200% 0; }
  100% { background-position: -200% 0; }
}

/* Card loops animation */
@media (prefers-reduced-motion: no-preference) {
  .hero .panel {
    animation: card-loop 3.2s ease-in-out infinite;
  }
}

@keyframes card-loop {
  0%, 100% { opacity: 1; transform: translateY(0); }
  50% { opacity: 0.9; transform: translateY(-4px); }
}

.hero .panel:hover {
  animation-play-state: paused;
}
</style>