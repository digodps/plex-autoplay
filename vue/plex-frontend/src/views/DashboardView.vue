<script setup>
import { onMounted, onUnmounted } from "vue";
import { useRouter } from "vue-router";
import { useDashboardStore } from "../stores/dashboard";
import { useDashboardCards } from "../composables/useDashboardCards";
import { useDashboardActions } from "../composables/useDashboardActions";
import StatusCard from "../components/StatusCard.vue";
import ActionBar from "../components/ActionBar.vue";
import AppIcon from "../components/AppIcon.vue";

const router = useRouter();
const store = useDashboardStore();
const { serviceCard, syncCard, librariesCard, markersCard } = useDashboardCards();
const { startScan } = useDashboardActions();

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
  const result = await startScan();
  if (result.ok) {
    router.push("/sync");
  } else {
    console.error("Falha ao iniciar escaneamento:", result.error);
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

    <!-- Erro parcial da API do Plex (bibliotecas) com sugestão de ação -->
    <div v-else-if="store.libraryError" class="dashboard-error">
      <AppIcon name="warning" size="lg" />
      <p>{{ store.libraryError }}</p>
      <p class="dashboard-error-hint">
        Verifique se o servidor Plex está acessível, se a URL e o token (X-Plex-Token) estão
        configurados corretamente e se há conectividade de rede.
      </p>
      <button class="btn" @click="store.retry"><AppIcon name="sync" /> Tentar novamente</button>
    </div>
  </div>
</template>

