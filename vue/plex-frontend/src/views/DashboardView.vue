<script setup>
import { onMounted, onUnmounted, watch } from "vue";
import { useRouter } from "vue-router";
import { apiFetch } from "../api";
import { useDashboardStore } from "../stores/dashboard";
import { useDashboardCards } from "../composables/useDashboardCards";
import { useThemeStore } from "../stores/theme";
import StatusCard from "../components/StatusCard.vue";
import ActionBar from "../components/ActionBar.vue";
import AppIcon from "../components/AppIcon.vue";

const router = useRouter();
const store = useDashboardStore();
const themeStore = useThemeStore();
const { serviceCard, syncCard, librariesCard, markersCard } = useDashboardCards();

// Paleta de cores de destaque (acessibilidade validada para claro/escuro)
const accentPalette = [
  { name: "amber", value: "#e5a00d" },
  { name: "blue", value: "#2563eb" },
  { name: "green", value: "#16a34a" },
  { name: "purple", value: "#7c3aed" },
  { name: "pink", value: "#db2777" },
  { name: "indigo", value: "#4f46e5" },
];

function toggleDarkMode() {
  const modes = ["light", "dark", "system"];
  const currentIndex = modes.indexOf(themeStore.mode);
  themeStore.mode = modes[(currentIndex + 1) % modes.length];
  themeStore.saveTheme();
}

function setAccentColor(color) {
  themeStore.accent = color;
  themeStore.saveTheme();
}

// Apply accent and mode to root element
function applyTheme() {
  document.documentElement.style.setProperty("--accent", themeStore.accent);
  document.documentElement.classList.toggle("theme-dark", themeStore.mode === "dark");
  document.documentElement.classList.toggle("theme-light", themeStore.mode === "light");
}

watch(
  () => themeStore.accent,
  () => applyTheme()
);
watch(
  () => themeStore.mode,
  () => applyTheme()
);

applyTheme();

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
      <div class="theme-controls" role="group" aria-label="Controles de tema">
        <button
          class="theme-toggle"
          type="button"
          :aria-label="`Modo de tema: ${themeStore.mode}. Clique para alternar.`"
          @click="toggleDarkMode"
        >
          {{ themeStore.mode === "dark" ? "🌙" : themeStore.mode === "light" ? "☀️" : "💻" }}
        </button>
        <div class="accent-palette" role="group" aria-label="Cor de destaque">
          <button
            v-for="color in accentPalette"
            :key="color.value"
            class="accent-swatch"
            :class="{ selected: themeStore.accent === color.value }"
            :style="{ backgroundColor: color.value }"
            :aria-label="`Selecionar cor ${color.name}`"
            :aria-pressed="themeStore.accent === color.value"
            type="button"
            @click="setAccentColor(color.value)"
          />
        </div>
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

<style scoped>
.theme-controls {
  display: flex;
  align-items: center;
  gap: 0.75rem;
}

.theme-toggle {
  padding: 0.5rem;
  border: 1px solid var(--border);
  border-radius: 999px;
  background: var(--surface);
  color: var(--text);
  cursor: pointer;
  font-size: 1rem;
}

.theme-toggle:focus-visible,
.accent-swatch:focus-visible {
  outline: 2px solid var(--accent);
  outline-offset: 2px;
}

.accent-palette {
  display: flex;
  align-items: center;
  gap: 0.35rem;
}

.accent-swatch {
  width: 1.5rem;
  height: 1.5rem;
  border: 2px solid transparent;
  border-radius: 50%;
  cursor: pointer;
}

.accent-swatch.selected {
  border-color: var(--text);
  box-shadow: 0 0 0 2px var(--surface);
}

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

.dashboard-error-hint {
  margin: 0.5rem 0 0;
  font-size: 0.85rem;
  color: color-mix(in srgb, var(--danger) 70%, transparent);
}
</style>
