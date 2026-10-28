import { defineStore } from "pinia";
import { ref, computed } from "vue";
import { fetchStatus, fetchLibraries, fetchMarkers } from "../api";
import {
  createServiceHealthComputed,
  createLibrariesSummaryComputed,
  createMarkersSummaryComputed,
} from "../composables/useDashboardAggregators";

/**
 * Store do Dashboard: agrega saúde do serviço, última sincronização,
 * visão geral de bibliotecas e cobertura de marcadores.
 * Faz polling automático a cada 30s enquanto a view está ativa.
 */
export const useDashboardStore = defineStore("dashboard", () => {
  // Estado bruto das respostas da API
  const statusData = ref(null);
  const librariesData = ref([]);
  const markersData = ref([]);

  // Estado de UI
  const loading = ref(false);
  const error = ref(null);
  const partialErrors = ref({}); // { status: true, libraries: false, markers: false }

  // Polling
  let pollTimer = null;
  const POLL_INTERVAL = 30_000; // 30 segundos

  // --- Computeds: dados transformados (usam agregadores extraídos) ---
  const serviceHealth = createServiceHealthComputed(statusData);
  const librariesSummary = createLibrariesSummaryComputed(librariesData);
  const markersSummary = createMarkersSummaryComputed(markersData);

  /** Loading global (qualquer fetch em andamento) */
  const isLoading = computed(() => loading.value);

  /** Algum erro parcial ou total */
  const hasErrors = computed(() => Object.values(partialErrors.value).some((v) => v));

  /** Erro total (todos falharam) */
  const isTotalFailure = computed(() => {
    const keys = Object.keys(partialErrors.value);
    return keys.length > 0 && keys.every((k) => partialErrors.value[k]);
  });

  // --- Actions ---

  async function fetchAll() {
    loading.value = true;
    error.value = null;
    partialErrors.value = {};

    try {
      const [statusRes, librariesRes, markersRes] = await Promise.allSettled([
        fetchStatus(),
        fetchLibraries(),
        fetchMarkers(),
      ]);

      if (statusRes.status === "fulfilled") statusData.value = statusRes.value;
      else {
        partialErrors.value.status = true;
        console.error("[Dashboard] fetchStatus failed:", statusRes.reason);
      }

      if (librariesRes.status === "fulfilled") librariesData.value = librariesRes.value;
      else {
        partialErrors.value.libraries = true;
        console.error("[Dashboard] fetchLibraries failed:", librariesRes.reason);
      }

      if (markersRes.status === "fulfilled") markersData.value = markersRes.value;
      else {
        partialErrors.value.markers = true;
        console.error("[Dashboard] fetchMarkers failed:", markersRes.reason);
      }

      // Se todos falharam, define erro global
      if (isTotalFailure.value) {
        error.value = "Não foi possível carregar os dados do dashboard.";
      }
    } catch (e) {
      // Promise.allSettled não rejeita, mas mantemos para segurança
      error.value = "Erro inesperado ao carregar dashboard.";
      console.error("[Dashboard] fetchAll unexpected error:", e);
    } finally {
      loading.value = false;
    }
  }

  function startPolling() {
    if (pollTimer) return; // já rodando
    pollTimer = setInterval(() => {
      // Só faz fetch se não estiver carregando (evita sobreposição)
      if (!loading.value) fetchAll();
    }, POLL_INTERVAL);
  }

  function stopPolling() {
    if (pollTimer) {
      clearInterval(pollTimer);
      pollTimer = null;
    }
  }

  function clearError() {
    error.value = null;
    partialErrors.value = {};
  }

  function retry() {
    clearError();
    fetchAll();
  }

  return {
    // Estado
    statusData,
    librariesData,
    markersData,
    loading,
    error,
    partialErrors,
    // Computeds
    serviceHealth,
    librariesSummary,
    markersSummary,
    isLoading,
    hasErrors,
    isTotalFailure,
    // Actions
    fetchAll,
    startPolling,
    stopPolling,
    clearError,
    retry,
  };
});
