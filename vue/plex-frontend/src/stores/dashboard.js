import { defineStore } from "pinia";
import { ref, computed } from "vue";
import { fetchStatus, fetchLibraries, fetchMarkers } from "../api";
import {
  createServiceHealthComputed,
  createLibrariesSummaryComputed,
  createMarkersSummaryComputed,
} from "../composables/useDashboardAggregators";

<<<<<<< HEAD
export const useDashboardStore = defineStore("dashboard", () => {
=======
/**
 * Store do Dashboard: agrega saúde do serviço, última sincronização,
 * visão geral de bibliotecas e cobertura de marcadores.
 * Faz polling automático a cada 30s enquanto a view está ativa.
 */
export const useDashboardStore = defineStore("dashboard", () => {
  // Estado bruto das respostas da API
>>>>>>> origin/feature/dashboard-redesign-31-10-2026-13-29-43-856
  const statusData = ref(null);
  const librariesData = ref([]);
  const markersData = ref([]);

<<<<<<< HEAD
  const loading = ref(false);
  const error = ref(null);
  const libraryError = ref(null);
  const partialErrors = ref({});

  let pollTimer = null;
  const POLL_INTERVAL = 30_000;

=======
  // Estado de UI
  const loading = ref(false);
  const error = ref(null);
  const libraryError = ref(null); // mensagem detalhada da falha da API do Plex
  const partialErrors = ref({}); // { status: true, libraries: false, markers: false }

  // Polling
  let pollTimer = null;
  const POLL_INTERVAL = 30_000; // 30 segundos

  // --- Computeds: dados transformados (usam agregadores extraídos) ---
>>>>>>> origin/feature/dashboard-redesign-31-10-2026-13-29-43-856
  const serviceHealth = createServiceHealthComputed(statusData);
  const librariesSummary = createLibrariesSummaryComputed(librariesData);
  const markersSummary = createMarkersSummaryComputed(markersData);

<<<<<<< HEAD
  const isLoading = computed(() => loading.value);
  const hasErrors = computed(() => Object.values(partialErrors.value).some((v) => v));
=======
  /** Loading global (qualquer fetch em andamento) */
  const isLoading = computed(() => loading.value);

  /** Algum erro parcial ou total */
  const hasErrors = computed(() => Object.values(partialErrors.value).some((v) => v));

  /** Erro total (todos falharam) */
>>>>>>> origin/feature/dashboard-redesign-31-10-2026-13-29-43-856
  const isTotalFailure = computed(() => {
    const keys = Object.keys(partialErrors.value);
    return keys.length > 0 && keys.every((k) => partialErrors.value[k]);
  });

<<<<<<< HEAD
=======
  // --- Actions ---

>>>>>>> origin/feature/dashboard-redesign-31-10-2026-13-29-43-856
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

      if (librariesRes.status === "fulfilled") {
        librariesData.value = librariesRes.value;
        libraryError.value = null;
      } else {
        partialErrors.value.libraries = true;
        libraryError.value = librariesRes.reason?.message || "Falha ao buscar bibliotecas do Plex.";
        console.error("[Dashboard] fetchLibraries failed:", librariesRes.reason);
      }

      if (markersRes.status === "fulfilled") markersData.value = markersRes.value;
      else {
        partialErrors.value.markers = true;
        console.error("[Dashboard] fetchMarkers failed:", markersRes.reason);
      }

<<<<<<< HEAD
      if (isTotalFailure.value) error.value = "Não foi possível carregar os dados do dashboard.";
    } catch (e) {
=======
      // Se todos falharam, define erro global
      if (isTotalFailure.value) {
        error.value = "Não foi possível carregar os dados do dashboard.";
      }
    } catch (e) {
      // Promise.allSettled não rejeita, mas mantemos para segurança
>>>>>>> origin/feature/dashboard-redesign-31-10-2026-13-29-43-856
      error.value = "Erro inesperado ao carregar dashboard.";
      console.error("[Dashboard] fetchAll unexpected error:", e);
    } finally {
      loading.value = false;
    }
  }

  function startPolling() {
<<<<<<< HEAD
    if (pollTimer) return;
    pollTimer = setInterval(() => {
=======
    if (pollTimer) return; // já rodando
    pollTimer = setInterval(() => {
      // Só faz fetch se não estiver carregando (evita sobreposição)
>>>>>>> origin/feature/dashboard-redesign-31-10-2026-13-29-43-856
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
    libraryError.value = null;
    partialErrors.value = {};
  }

  function retry() {
    clearError();
    fetchAll();
  }

  return {
<<<<<<< HEAD
=======
    // Estado
>>>>>>> origin/feature/dashboard-redesign-31-10-2026-13-29-43-856
    statusData,
    librariesData,
    markersData,
    loading,
    error,
    libraryError,
    partialErrors,
<<<<<<< HEAD
=======
    // Computeds
>>>>>>> origin/feature/dashboard-redesign-31-10-2026-13-29-43-856
    serviceHealth,
    librariesSummary,
    markersSummary,
    isLoading,
    hasErrors,
    isTotalFailure,
<<<<<<< HEAD
=======
    // Actions
>>>>>>> origin/feature/dashboard-redesign-31-10-2026-13-29-43-856
    fetchAll,
    startPolling,
    stopPolling,
    clearError,
    retry,
  };
<<<<<<< HEAD
});
=======
});
>>>>>>> origin/feature/dashboard-redesign-31-10-2026-13-29-43-856
