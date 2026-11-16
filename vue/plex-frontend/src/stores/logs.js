import { defineStore } from "pinia";
import { ref } from "vue";
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
    try {
      const params = new URLSearchParams();
      if (filters.value.jobId) params.set("jobId", filters.value.jobId);
      if (filters.value.from) params.set("from", filters.value.from);
      if (filters.value.to) params.set("to", filters.value.to);
      if (filters.value.level) params.set("level", filters.value.level);
      if (filters.value.text) params.set("text", filters.value.text);
      params.set("page", "1");
      params.set("pageSize", "1000");

      const res = await apiFetch(`/api/sync/logs?${params}`);
      if (res.ok) {
        logs.value = await res.json();
      }
    } catch (e) {
      error.value = e.message;
    } finally {
      loadingLogs.value = false;
    }
  }

  function setTextFilter(text) {
    filters.value.text = text;
    filters.value.page = 1;
    clearTimeout(debounceTimer);
    debounceTimer = setTimeout(() => {
      fetchLogs();
    }, 300);
  }

  function setLevelFilter(level) {
    filters.value.level = level;
    filters.value.page = 1;
    fetchLogs();
  }

  function setDateRange(from, to) {
    filters.value.from = from;
    filters.value.to = to;
    filters.value.page = 1;
    fetchLogs();
  }

  function setGroupBy(value) {
    groupBy.value = value;
  }

  async function clearLogs() {
    try {
      const res = await apiFetch("/api/sync/logs/clear", { method: "DELETE" });
      if (res.ok) {
        logs.value = [];
      }
    } catch (e) {
      error.value = e.message;
    }
  }

  async function cleanupLogs(days) {
    try {
      const res = await apiFetch("/api/sync/logs/cleanup", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ days }),
      });
      if (res.ok) {
        await fetchLogs();
      }
    } catch (e) {
      error.value = e.message;
    }
  }

  return {
    jobs,
    logs,
    loading,
    loadingLogs,
    error,
    filters,
    groupBy,
    fetchJobs,
    fetchLogs,
    setTextFilter,
    setLevelFilter,
    setDateRange,
    setGroupBy,
    clearLogs,
    cleanupLogs,
  };
});