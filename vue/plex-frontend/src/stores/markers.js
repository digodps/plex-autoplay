import { defineStore } from "pinia";
import { ref } from "vue";
import { apiFetch, extractErrorMessage } from "../api";

/**
 * Store central de marcadores: junta episódios da série (Plex) com marcadores
 * persistidos (SQLite), por ratingKey, e orquestra o salvamento otimista.
 */
export const useMarkerStore = defineStore("markers", () => {
  // episódios e marcadores são idenxados por ratingKey
  const episodesMap = ref({}); // ratingKey -> { title, season, episode, rating_key }
  const markersMap = ref({}); // ratingKey -> { intro: {startMs,endMs,origin}|null, credits: {...}|null }
  const loading = ref(false);
  const error = ref("");
  const seriesTitle = ref("");
  const savingKeys = ref({}); // `${ratingKey}:${kind}` -> true quando salvando

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
        throw new Error(detail || "Erro ao carregar série");
      }

      const episodes = await episodesRes.json();
      let allMarkers = [];
      try {
        allMarkers = await markersRes.json();
      } catch {
        allMarkers = [];
      }

      const episodesById = {};
      for (const ep of episodes) episodesById[ep.rating_key] = ep;
      episodesMap.value = episodesById;

      // merge: só marcadores de episódios desta série
      const markers = {};
      for (const ep of episodes) markers[ep.rating_key] = { intro: null, credits: null };
      for (const m of allMarkers) {
        if (!markers[m.ratingKey] && !markers[m.rating_key]) continue;
        // normaliza camel/snake
        const key = m.ratingKey ?? m.rating_key;
        const kind = m.kind;
        markers[key][kind] = {
          startMs: m.startMs ?? m.start_ms ?? 0,
          endMs: m.endMs ?? m.end_ms ?? 0,
          origin: m.origin ?? "manual",
        };
      }
      markersMap.value = markers;
    } catch (e) {
      error.value = e.message || "Não foi possível carregar os dados da série.";
    } finally {
      loading.value = false;
    }
  }

  /**
   * Cria marcador (POST) e o reflete no estado.
   */
  async function createMarker(ratingKey, kind, startMs, endMs) {
    const key = `${ratingKey}:${kind}`;
    savingKeys.value[key] = true;
    try {
      const res = await apiFetch("/api/markers", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ ratingKey, kind, startMs, endMs }),
      });
      if (!res.ok) throw new Error("Erro ao criar marcador");
      if (!markersMap.value[ratingKey])
        markersMap.value[ratingKey] = { intro: null, credits: null };
      markersMap.value[ratingKey][kind] = { startMs, endMs, origin: "manual" };
      return true;
    } catch {
      error.value = `Erro ao salvar marcador de ${kind}.`;
      return false;
    } finally {
      delete savingKeys.value[key];
    }
  }

  /**
   * Atualização otimista: reflete imediatamente, envia PUT e reverte em falha.
   */
  async function updateMarker(ratingKey, kind, startMs, endMs) {
    const key = `${ratingKey}:${kind}`;
    const previous = markersMap.value[ratingKey]?.[kind];
    savingKeys.value[key] = true;

    // otimista
    markersMap.value[ratingKey][kind] = { ...(previous ?? {}), startMs, endMs };

    try {
      const res = await apiFetch(
        `/api/markers/${encodeURIComponent(ratingKey)}/${encodeURIComponent(kind)}`,
        {
          method: "PUT",
          headers: { "Content-Type": "application/json" },
          body: JSON.stringify({ startMs, endMs }),
        },
      );
      if (!res.ok) {
        // rollback
        markersMap.value[ratingKey][kind] = previous;
        throw new Error("PUT falhou");
      }
      return true;
    } catch {
      error.value = `Erro ao atualizar marcador de ${kind}.`;
      return false;
    } finally {
      delete savingKeys.value[key];
    }
  }

  async function removeMarker(ratingKey, kind) {
    const key = `${ratingKey}:${kind}`;
    const previous = markersMap.value[ratingKey]?.[kind];
    savingKeys.value[key] = true;

    // otimista
    markersMap.value[ratingKey][kind] = null;

    try {
      const res = await apiFetch(
        `/api/markers/${encodeURIComponent(ratingKey)}/${encodeURIComponent(kind)}`,
        {
          method: "DELETE",
        },
      );
      if (!res.ok) {
        markersMap.value[ratingKey][kind] = previous;
        throw new Error("DELETE falhou");
      }
      return true;
    } catch {
      error.value = `Erro ao remover marcador de ${kind}.`;
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
