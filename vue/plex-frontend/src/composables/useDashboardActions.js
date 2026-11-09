import { syncRun } from "../api";

export function useDashboardActions() {
  async function startScan() {
    try {
      const res = await syncRun();
      if (res.ok) {
        const data = await res.json().catch(() => ({}));
        return { ok: true, jobId: data.job_id };
      }
      return { ok: false, error: await res.text() };
    } catch (e) {
      return { ok: false, error: String(e) };
    }
  }

  return { startScan };
}