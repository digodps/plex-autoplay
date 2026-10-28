import { computed } from "vue";

/**
 * Funções puras de agregação para o dashboard.
 * Extraídas do store para reduzir sua complexidade (SRP).
 */

/** Formata uptime em ms para string legível (ex: "2d 4h 30min") */
export function formatUptime(ms) {
  const sec = Math.floor(ms / 1000);
  const d = Math.floor(sec / 86400);
  const h = Math.floor((sec % 86400) / 3600);
  const m = Math.floor((sec % 3600) / 60);
  const parts = [];
  if (d) parts.push(`${d}d`);
  if (h) parts.push(`${h}h`);
  if (m || parts.length === 0) parts.push(`${m}min`);
  return parts.join(" ");
}

/** Cria computed de serviceHealth a partir de statusData */
export function createServiceHealthComputed(statusData) {
  return computed(() => {
    if (!statusData.value) return null;
    const { service, version, lastJob, process } = statusData.value;
    const uptimeMs = process?.uptime_ms ?? 0;
    const uptimeHuman = formatUptime(uptimeMs);
    const cpuPercent = process?.cpu_total_ms
      ? ((process.cpu_total_ms / uptimeMs) * 100).toFixed(1)
      : "0.0";
    const memMB = process?.memory_working_set_bytes
      ? Math.round(process.memory_working_set_bytes / 1024 / 1024)
      : 0;

    return {
      service,
      version,
      status: "ok",
      uptime: uptimeHuman,
      cpuPercent: `${cpuPercent}%`,
      memMB,
      lastJob: lastJob
        ? {
            status: lastJob.status,
            processed: lastJob.processed ?? 0,
            skipped: lastJob.skipped ?? 0,
            errors: lastJob.errors ?? 0,
            error: lastJob.error,
            updatedAt: lastJob.updated_at,
          }
        : null,
    };
  });
}

/** Cria computed de librariesSummary a partir de librariesData */
export function createLibrariesSummaryComputed(librariesData) {
  return computed(() => {
    if (!librariesData.value?.length) return { count: 0, totalSeries: 0, items: [] };
    const totalSeries = librariesData.value.reduce((sum, lib) => sum + (lib.seriesCount ?? 0), 0);
    return {
      count: librariesData.value.length,
      totalSeries,
      items: librariesData.value.map((lib) => ({
        key: lib.key,
        title: lib.title,
        seriesCount: lib.seriesCount ?? 0,
      })),
    };
  });
}

/** Cria computed de markersSummary a partir de markersData */
export function createMarkersSummaryComputed(markersData) {
  return computed(() => {
    if (!markersData.value?.length) {
      return { total: 0, byKind: { intro: 0, credits: 0 }, byOrigin: { detected: 0, manual: 0 } };
    }
    const byKind = { intro: 0, credits: 0 };
    const byOrigin = { detected: 0, manual: 0 };
    for (const m of markersData.value) {
      const kind = m.kind === "credits" ? "credits" : "intro";
      byKind[kind] = (byKind[kind] ?? 0) + 1;
      const origin = m.origin === "manual" ? "manual" : "detected";
      byOrigin[origin] = (byOrigin[origin] ?? 0) + 1;
    }
    return {
      total: markersData.value.length,
      byKind,
      byOrigin,
    };
  });
}
