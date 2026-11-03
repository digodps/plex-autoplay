import { apiFetch } from "../api";

export function useDashboardActions() {
  async function startScan() {
    try {
      const res = await apiFetch("/api/plex/scan", { method: "POST" });
      if (res.ok) {
        return { ok: true };
      }
      return { ok: false, error: await res.text() };
    } catch (e) {
      return { ok: false, error: String(e) };
    }
  }

  return { startScan };
}
