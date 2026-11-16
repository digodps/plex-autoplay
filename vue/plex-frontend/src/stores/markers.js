import { defineStore } from "pinia";
import { ref } from "vue";
import { useI18n } from "vue-i18n";
import { apiFetch, extractErrorMessage } from "../api";

export const useMarkerStore = defineStore("markers", () => {
  const { t } = useI18n();
  const episodesMap = ref({});
  const markersMap = ref({});
  const loading = ref(false);
  const error = ref("");
  const seriesTitle = ref("");
  const savingKeys = ref({});

  async function fetchSeries(seriesKey) {
    loading.value = true;
    error.value = "";
    episodesMap.value = {};
    markersMap.value = {};

    try {
      const [episodesRes, markersRes] = await Promise.all([
        apiFetch(`/api/series/0/${encodeURIComponent(seriesKey)}/episodes`),
        apiFetch("/api/markers"),
      ]);

      if (!episodesRes.ok || !markersRes.ok) {
        const failedRes = episodesRes.ok ? markersRes : episodesRes;
        const detail = await extractErrorMessage(failedRes);
        throw new Error(detail || t("markers.loadError"));
      }

      const episodes = await episodesRes.json();
      let allMarkers = [];
      try {
        allMarkers = await markersRes.json();
      } catch {
        allMarkers = [];
      }

      const episodesById = {};
      for (const ep of episodes) {
        const epCopy = { ...ep };
        if (!('thumb' in epCopy)) epCopy.thumb = null;
        episodesById[epCopy.rating_key] = epCopy;
      }
      episodesMap.value = episodesById;

      const markers = {};
      for (const ratingKey of Object.keys(episodesById)) markers[ratingKey] = { intro: null, credits: null };
      for (const m of allMarkers) {
        const key = m.ratingKey ?? m.rating_key;
        if (!markers[key]) markers[key] = { intro: null, credits: null };
        const kind = m.kind;
        markers[key][kind] = {
          startMs: m.startMs ?? m.start_ms ?? 0,
          endMs: m.endMs ?? m.end_ms ?? 0,
          origin: m.origin ?? "manual",
        };
      }
      markersMap.value = markers;
    } catch (e) {
      error.value = e.message || t("markers.loadError");
    } finally {
      loading.value = false;
    }
  }

  async function createMarker(ratingKey, kind, startMs, endMs) {
    const key = `${ratingKey}:${kind}`;
    savingKeys.value[key] = true;
    try {
      const res = await apiFetch("/api/markers", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ ratingKey, kind, startMs, endMs }),
      });
      if (!res.ok) throw new Error(t("markers.createError", { kind }));
      if (!markersMap.value[ratingKey])
        markersMap.value[ratingKey] = { intro: null, credits: null };
      markersMap.value[ratingKey][kind] = { startMs, endMs, origin: "manual" };
      return true;
    } catch {
      error.value = t("markers.saveError", { kind });
      return false;
    } finally {
      delete savingKeys.value[key];
    }
  }

  async function updateMarker(ratingKey, kind, startMs, endMs) {
    const key = `${ratingKey}:${kind}`;
    const previous = markersMap.value[ratingKey]?.[kind];
    savingKeys.value[key] = true;

    markersMap.value[ratingKey][kind] = { ...(previous ?? {}), startMs, endMs };

    try {
      const res = await apiFetch(
        `/api/markers/${encodeURIComponent(ratingKey)}/${encodeURIComponent(kind)}`,
        {
          method: "PUT",
          headers: { "Content-Type": "application/json" },
          body: JSON.stringify({ startMs, endMs }),
        }
      );
      if (!res.ok) {
        markersMap.value[ratingKey][kind] = previous;
        throw new Error(t("markers.updateError", { kind }));
      }
      return true;
    } catch {
      error.value = t("markers.updateError", { kind });
      return false;
    } finally {
      delete savingKeys.value[key];
    }
  }

  async function removeMarker(ratingKey, kind) {
    const key = `${ratingKey}:${kind}`;
    const previous = markersMap.value[ratingKey]?.[kind];
    savingKeys.value[key] = true;

    markersMap.value[ratingKey][kind] = null;

    try {
      const res = await apiFetch(
        `/api/markers/${encodeURIComponent(ratingKey)}/${encodeURIComponent(kind)}`,
        { method: "DELETE" }
      );
      if (!res.ok) {
        markersMap.value[ratingKey][kind] = previous;
        throw new Error(t("markers.deleteError", { kind }));
      }
      return true;
    } catch {
      error.value = t("markers.deleteError", { kind });
      return false;
    } finally {
      delete savingKeys.value[key];
    }
  }

  function clearError() {
    error.value = "";
  }

  return {
    episodesMap,
    markersMap,
    loading,
    error,
    seriesTitle,
    savingKeys,
    fetchSeries,
    createMarker,
    updateMarker,
    removeMarker,
    clearError,
  };
});