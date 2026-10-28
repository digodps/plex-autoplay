<script setup>
import { onMounted, onUnmounted } from "vue";
import { useRouter } from "vue-router";
import { apiFetch } from "../api";
import { useDashboardStore } from "../stores/dashboard";
import { useDashboardCards } from "../composables/useDashboardCards";
import StatusCard from "../components/StatusCard.vue";
import ActionBar from "../components/ActionBar.vue";
import AppIcon from "../components/AppIcon.vue";

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
      router.push("/sync");
    } else {
      console.error("Falha ao iniciar escaneamento:", await res.text());
    }
  } catch (e) {
    console.error("Erro ao iniciar escaneamento:", e);
  }
}

function handleViewFullLog() {
  router.push("/sync");
}
</script>

<template>
  <div class="dashboard">
    <header class="dashboard-header">
      <div>
        <h1>Início</h1>
        <p class="dashboard-subtitle">Visão geral do Plex Autoplay</p>
      </div>
      <ActionBar>
        <template #secondary>
          <button class="btn btn-ghost" :disabled="store.isLoading" @click="handleViewFullLog">
            <AppIcon name="sync" /> Ver Log Completo
          </button>
        </template>
        <template #primary>
          <button class="btn" :disabled="store.isLoading" @click="handleStartScan">
            <AppIcon name="play" /> Iniciar Escaneamento
          </button>
        </template>
      </ActionBar>
    </header>

    <!-- Grid de cards -->
    <section class="dashboard-grid" aria-label="Métricas do sistema">
      <StatusCard v-bind="serviceCard" />
      <StatusCard v-bind="syncCard" />
      <StatusCard v-bind="librariesCard" />
      <StatusCard v-bind="markersCard" />
    </section>

    <!-- Erro total + retry -->
    <div v-if="store.isTotalFailure" class="dashboard-error">
      <AppIcon name="warning" size="lg" />
      <p>{{ store.error }}</p>
      <button class="btn" @click="store.retry"><AppIcon name="sync" /> Tentar novamente</button>
    </div>
  </div>
</template>

<style scoped>
.dashboard {
  max-width: 1200px;
  margin: 0 auto;
  padding: 1.5rem;
}

.dashboard-header {
  display: flex;
  align-items: flex-end;
  justify-content: space-between;
  gap: 1rem;
  flex-wrap: wrap;
  margin-bottom: 1.5rem;
}

.dashboard-header h1 {
  font-size: 1.5rem;
  font-weight: 700;
  margin: 0;
}

.dashboard-subtitle {
  margin: 0.25rem 0 0;
  color: var(--text-secondary);
  font-size: 0.95rem;
}

.dashboard-grid {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: 1rem;
}

@media (max-width: 768px) {
  .dashboard-grid {
    grid-template-columns: 1fr;
  }
}

.dashboard-error {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 1rem;
  margin-top: 2rem;
  padding: 2rem;
  text-align: center;
  background: color-mix(in srgb, var(--danger) 8%, transparent);
  border: 1px solid color-mix(in srgb, var(--danger) 25%, transparent);
  border-radius: var(--radius);
  color: var(--danger);
}

.dashboard-error p {
  margin: 0;
  font-size: 1rem;
}
</style>
