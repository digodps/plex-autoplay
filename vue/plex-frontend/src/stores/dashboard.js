import { defineStore } from "pinia";
import { ref, computed } from "vue";
import { fetchStatus, fetchLibraries, fetchMarkers } from "../api";
import {
  createServiceHealthComputed,
  createLibrariesSummaryComputed,
  createMarkersSummaryComputed,
} from "../composables/useDashboardAggregators";

export const useDashboardStore = defineStore("dashboard", () => {
  const statusData = ref(null);
  const librariesData = ref([]);
  const markersData = ref([]);

  const loading = ref(false);
  const error = ref(null);
  const libraryError = ref(null);
  const partialErrors = ref({});

  let pollTimer = null;
  const POLL_INTERVAL = 30_000;

  const serviceHealth = createServiceHealthComputed(statusData);
  const librariesSummary = createLibrariesSummaryComputed(librariesData);
  const markersSummary = createMarkersSummaryComputed(markersData);

  const isLoading = computed(() => loading.value);
  const hasErrors = computed(() => Object.values(partialErrors.value).some((v) => v));
  const isTotalFailure = computed(() => {
    const keys = Object.keys(partialErrors.value);
    return keys.length > 0 && keys.every((k) => partialErrors.value[k]);
  });

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

      if (isTotalFailure.value) error.value = "Não foi possível carregar os dados do dashboard.";
    } catch (e) {
      error.value = "Erro inesperado ao carregar dashboard.";
      console.error("[Dashboard] fetchAll unexpected error:", e);
    } finally {
      loading.value = false;
    }
  }

  function startPolling() {
    if (pollTimer) return;
    pollTimer = setInterval(() => {
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
    statusData,
    librariesData,
    markersData,
    loading,
    error,
    libraryError,
    partialErrors,
    serviceHealth,
    librariesSummary,
    markersSummary,
    isLoading,
    hasErrors,
    isTotalFailure,
    fetchAll,
    startPolling,
    stopPolling,
    clearError,
    retry,
  };
});