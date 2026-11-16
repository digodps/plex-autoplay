<script setup>
import { onMounted, onUnmounted } from "vue";
import { useRouter } from "vue-router";
import { apiFetch } from "../api";
import { useDashboardStore } from "../stores/dashboard";
import { useDashboardCards } from "../composables/useDashboardCards";

const router = useRouter();
const store = useDashboardStore();
const { serviceCard, syncCard, librariesCard, markersCard } = useDashboardCards();

// Inicia polling ao montar, para ao desmontar
onMounted(() => {
  store.fetchAll();
  store.startPolling();
});

onUnmounted(() => {
  store.stopPolling();
});

// Handlers das ações
async function handleStartScan() {
  try {
    const res = await apiFetch("/api/plex/scan", { method: "POST" });
    if (res.ok) {
      router.push("/logs");
    } else {
      console.error("Falha ao iniciar escaneamento:", await res.text());
    }
  } catch (e) {
    console.error("Erro ao iniciar escaneamento:", e);
  }
}

function handleViewFullLog() {
  router.push("/logs");
}
</script>

<template>
  <div class="dashboard">
    <!-- HERO: type-led, 1 screen, data-enter -->
    <section class="hero hero--statement" data-enter aria-labelledby="dashboard-hero-h">
      <div class="wrap grid">
        <div class="hero-copy span-6">
          <h1 id="dashboard-hero-h">Início</h1>
          <p class="t-lead">Dashboard para gerenciar marcadores de intro/créditos no Plex via API oficial.</p>
          <div class="btn-row">
            <button class="btn btn--primary" :disabled="store.isLoading" @click="handleStartScan" data-testid="btn-start-scan">
              <img src="/assets/icons/play.svg" data-icon="play" class="icon" width="20" height="20" aria-hidden="true" />
              Iniciar Escaneamento
            </button>
          </div>
        </div>
        <div class="hero-visual span-6 start-7" aria-hidden="true">
          <div class="panel frame" data-ui="dashboard-metrics">
            <div class="metrics-grid grid" role="list" aria-label="Métricas do sistema">
              <StatusCard v-bind="serviceCard" role="listitem" />
              <StatusCard v-bind="syncCard" role="listitem" />
              <StatusCard v-bind="librariesCard" role="listitem" />
              <StatusCard v-bind="markersCard" role="listitem" />
            </div>
          </div>
        </div>
      </div>
    </section>

    <!-- SECTION: Métricas (dealt moment: pulsing diagram) -->
    <section class="section section--metrics" aria-labelledby="metrics-h">
      <div class="wrap">
        <h2 id="metrics-h" class="t-h2">Métricas do Sistema</h2>
        <div class="metrics-grid grid" role="list" aria-label="Métricas detalhadas">
          <StatusCard v-bind="serviceCard" role="listitem" />
          <StatusCard v-bind="syncCard" role="listitem" />
          <StatusCard v-bind="librariesCard" role="listitem" />
          <StatusCard v-bind="markersCard" role="listitem" />
        </div>

        <!-- Dealt Moment: pulsing diagram (hub/lanes) -->
        <div class="moment moment--diagram" data-diagram="hub" aria-label="Diagrama de arquitetura do sistema">
          <div class="diagram-frame">
            <div class="diagram-node diagram-node--center">
              <img src="/assets/icons/database.svg" data-icon="database" class="icon" width="32" height="32" aria-hidden="true" />
            </div>
            <div class="diagram-node">
              <img src="/assets/icons/server.svg" data-icon="server" class="icon" width="24" height="24" aria-hidden="true" />
            </div>
            <div class="diagram-node">
              <img src="/assets/icons/activity.svg" data-icon="activity" class="icon" width="24" height="24" aria-hidden="true" />
            </div>
            <div class="diagram-node">
              <img src="/assets/icons/tv.svg" data-icon="tv" class="icon" width="24" height="24" aria-hidden="true" />
            </div>
          </div>
        </div>
      </div>
    </section>

    <!-- Erro total + retry -->
    <div v-if="store.isTotalFailure" class="dashboard-error" role="alert">
      <img src="/assets/icons/warning.svg" data-icon="warning" class="icon icon-lg" width="48" height="48" aria-hidden="true" />
      <p>{{ store.error }}</p>
      <button class="btn btn--primary" @click="store.retry" data-testid="btn-retry">
        <img src="/assets/icons/sync.svg" data-icon="sync" class="icon" width="20" height="20" aria-hidden="true" />
        Tentar novamente
      </button>
    </div>

    <!-- Erro parcial da API do Plex (bibliotecas) com sugestão de ação -->
    <div v-else-if="store.libraryError" class="dashboard-error" role="alert">
      <img src="/assets/icons/warning.svg" data-icon="warning" class="icon icon-lg" width="48" height="48" aria-hidden="true" />
      <p>{{ store.libraryError }}</p>
      <p class="dashboard-error-hint">
        Verifique se o servidor Plex está acessível, se a URL e o token (X-Plex-Token) estão
        configurados corretamente e se há conectividade de rede.
      </p>
      <button class="btn btn--primary" @click="store.retry" data-testid="btn-retry">
        <img src="/assets/icons/sync.svg" data-icon="sync" class="icon" width="20" height="20" aria-hidden="true" />
        Tentar novamente
      </button>
    </div>
  </div>
</template>

<style scoped>
.dashboard {
  /* Page wrapper - no max-width here, sections handle it */
}

.hero {
  position: relative;
  min-height: calc(100vh - var(--nav-h, 64px));
  display: flex;
  align-items: center;
  padding-top: var(--s-8);
  padding-bottom: var(--s-8);
}

/* Hero entrance animation */
.hero[data-enter] {
  opacity: 0;
  transform: translateY(20px);
  transition: opacity 600ms var(--ease), transform 600ms var(--ease);
}
.hero[data-enter].m-enter-done {
  opacity: 1;
  transform: translateY(0);
}
@media (prefers-reduced-motion: reduce) {
  .hero[data-enter] {
    opacity: 1;
    transform: translateY(0);
    transition: none;
  }
}

.hero-copy {
  display: flex;
  flex-direction: column;
  justify-content: center;
}

.hero-copy h1 {
  font: var(--font-display);
  font-size: var(--t-4xl);
  font-weight: var(--display-weight);
  line-height: 1.1;
  letter-spacing: var(--display-tracking);
  margin: 0 0 var(--s-4);
  color: var(--p-ink);
}
.hero-copy .t-lead {
  font-size: var(--t-lg);
  line-height: 1.6;
  color: var(--p-muted);
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
  border-radius: var(--r-full);
  border: 1px solid transparent;
  cursor: pointer;
  transition: background var(--speed) var(--ease), border-color var(--speed) var(--ease), color var(--speed) var(--ease), transform var(--speed) var(--ease), box-shadow var(--speed) var(--ease);
}
.btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}
.btn:focus-visible {
  outline: 2px solid var(--p-accent);
  outline-offset: 2px;
}

.btn--primary {
  background: var(--p-action);
  color: var(--p-action-ink);
  border-color: var(--p-action);
}
.btn--primary:hover:not(:disabled) {
  background: color-mix(in srgb, var(--p-action) 90%, black);
  border-color: color-mix(in srgb, var(--p-action) 90%, black);
}
.btn--primary:active:not(:disabled) {
  transform: scale(0.98);
}

.btn--secondary {
  background: var(--p-surface);
  color: var(--p-ink);
  border-color: var(--border);
}
.btn--secondary:hover:not(:disabled) {
  background: var(--p-muted);
  border-color: var(--p-accent);
}
.btn--secondary:active:not(:disabled) {
  transform: scale(0.98);
}

.icon {
  display: inline-block;
  vertical-align: middle;
  flex-shrink: 0;
}
.icon-lg { width: 48px; height: 48px; }

.hero-visual {
  display: flex;
  align-items: center;
  justify-content: center;
}

.frame {
  border-radius: var(--r-lg);
  overflow: hidden;
  background: var(--p-surface);
  border: 1px solid var(--border);
}

.metrics-grid {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: var(--s-4);
}
@media (max-width: 768px) {
  .metrics-grid {
    grid-template-columns: 1fr;
  }
}

.section {
  padding: var(--s-12) 0;
}

.t-h2 {
  font: var(--font-display);
  font-size: var(--t-2xl);
  font-weight: 600;
  letter-spacing: var(--display-tracking);
  margin: 0 0 var(--s-6);
  color: var(--p-ink);
}

/* Dealt Moment: Pulsing Diagram */
.moment--diagram {
  margin-top: var(--s-8);
  padding: var(--s-6);
  background: var(--p-surface);
  border: 1px solid var(--border);
  border-radius: var(--r-lg);
}
.diagram-frame {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: var(--s-8);
  flex-wrap: wrap;
}
.diagram-node {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 80px;
  height: 80px;
  background: var(--p-bg);
  border: 1px solid var(--border);
  border-radius: var(--r-full);
  transition: transform var(--speed) var(--ease), box-shadow var(--speed) var(--ease);
}
.diagram-node:hover {
  transform: scale(1.05);
  box-shadow: var(--shadow-2);
}
.diagram-node--center {
  width: 100px;
  height: 100px;
  background: color-mix(in srgb, var(--p-accent) 15%, transparent);
  border-color: var(--p-accent);
}

/* Card loops animation */
@media (prefers-reduced-motion: no-preference) {
  .metrics-grid .panel {
    animation: card-loop 3s ease-in-out infinite;
  }
  .metrics-grid .panel:nth-child(2) { animation-delay: 0.75s; }
  .metrics-grid .panel:nth-child(3) { animation-delay: 1.5s; }
  .metrics-grid .panel:nth-child(4) { animation-delay: 2.25s; }
}

@keyframes card-loop {
  0%, 100% { opacity: 1; transform: translateY(0); }
  50% { opacity: 0.9; transform: translateY(-4px); }
}

/* Pause on hover */
.metrics-grid .panel:hover {
  animation-play-state: paused;
}

/* Error states */
.dashboard-error {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: var(--s-4);
  margin-top: var(--s-8);
  padding: var(--s-8);
  text-align: center;
  background: color-mix(in srgb, var(--danger) 8%, transparent);
  border: 1px solid color-mix(in srgb, var(--danger) 25%, transparent);
  border-radius: var(--r-lg);
  color: var(--danger);
}
.dashboard-error p {
  margin: 0;
  font-size: var(--t-base);
}
.dashboard-error-hint {
  margin: var(--s-2) 0 0;
  font-size: var(--t-sm);
  color: color-mix(in srgb, var(--danger) 70%, transparent);
}
</style>