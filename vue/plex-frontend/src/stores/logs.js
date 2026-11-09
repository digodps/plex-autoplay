import { defineStore } from "pinia";
import { ref, computed } from "vue";
import { apiFetch } from "@/api";

export const useLogsStore = defineStore("logs", () => {
  const jobs = ref([]);
  const logs = ref([]);
  const loading = ref(false);
  const loadingLogs = ref(false);
  const error = ref(null);

  // Filtros
  const filters = ref({
    jobId: "",
    from: "",
    to: "",
    level: "",
    text: "",
    page: 1,
    pageSize: 100,
  });

  const groupBy = ref("date"); // "date" | "status" | "level"

  // Debounce para filtro live
  let debounceTimer = null;

  async function fetchJobs() {
    loading.value = true;
    error.value = null;
    try {
      const res = await apiFetch(`/api/sync/history?page=${filters.value.page}&pageSize=${filters.value.pageSize}`);
      if (res.ok) {
        jobs.value = await res.json();
      }
    } catch (e) {
      error.value = e.message;
    } finally {
      loading.value = false;
    }
  }

  async function fetchLogs() {
    loadingLogs.value = true;
    error.value = null;
    try {
      const params = new URLSearchParams();
      if (filters.value.jobId) params.set("jobId", filters.value.jobId);
      if (filters.value.from) params.set("from", filters.value.from);
      if (filters.value.to) params.set("to", filters.value.to);
      if (filters.value.level) params.set("level", filters.value.level);
      if (filters.value.text) params.set("text", filters.value.text);
      params.set("page", filters.value.page);
      params.set("pageSize", filters.value.pageSize);

      const res = await apiFetch(`/api/sync/logs?${params.toString()}`);
      if (res.ok) {
        logs.value = await res.json();
      }
    } catch (e) {
      error.value = e.message;
    } finally {
      loadingLogs.value = false;
    }
  }

  // Live text filter debounced
  function setTextFilter(text) {
    filters.value.text = text;
    if (debounceTimer) clearTimeout(debounceTimer);
    debounceTimer = setTimeout(() => {
      filters.value.page = 1;
      fetchLogs();
    }, 200);
  }

  function setLevelFilter(level) {
    filters.value.level = level || "";
    filters.value.page = 1;
    fetchLogs();
  }

  function setDateRange(from, to) {
    filters.value.from = from || "";
    filters.value.to = to || "";
    filters.value.page = 1;
    fetchLogs();
  }

  function setGroupBy(mode) {
    groupBy.value = mode;
  }

  function resetFilters() {
    filters.value = {
      jobId: "",
      from: "",
      to: "",
      level: "",
      text: "",
      page: 1,
      pageSize: 100,
    };
    fetchLogs();
  }

  // Agrupamento
  const groupedLogs = computed(() => {
    const groups = {};
    const items = logs.value || [];

    for (const log of items) {
      let key;
      if (groupBy.value === "date") {
        key = log.timestamp.split("T")[0]; // YYYY-MM-DD
      } else if (groupBy.value === "status") {
        // precisa de job status — use jobId lookup if needed
        key = log.level; // fallback
      } else {
        key = log.level; // "level" (info, error, warning, skip, success)
      }
      if (!groups[key]) groups[key] = [];
      groups[key].push(log);
    }

    // Ordena chaves decrescente (mais recente primeiro)
    const sortedKeys = Object.keys(groups).sort((a, b) => b.localeCompare(a));
    const result = {};
    for (const k of sortedKeys) result[k] = groups[k];
    return result;
  });

  async function clearLogs() {
    const res = await apiFetch("/api/sync/logs/clear", { method: "POST" });
    if (res.ok) {
      logs.value = [];
      await fetchJobs();
      return res.json();
    }
    throw new Error("Falha ao limpar logs");
  }

  async function cleanupLogs(retentionDays = 30) {
    const res = await apiFetch(`/api/sync/logs/cleanup?retentionDays=${retentionDays}`, { method: "POST" });
    if (res.ok) {
      await fetchLogs();
      await fetchJobs();
      return res.json();
    }
    throw new Error("Falha ao limpar logs antigos");
  }

  return {
    jobs,
    logs,
    loading,
    loadingLogs,
    error,
    filters,
    groupBy,
    groupedLogs,
    fetchJobs,
    fetchLogs,
    setTextFilter,
    setLevelFilter,
    setDateRange,
    setGroupBy,
    resetFilters,
    clearLogs,
    cleanupLogs,
  };
});