import { defineStore } from "pinia";
import { ref, computed } from "vue";
import { useI18n } from "vue-i18n";
import { apiFetch, throwHttpError } from "../api";

export const useSeriesStore = defineStore("series", () => {
  const { t } = useI18n();
  const episodes = ref([]);
  const currentEpisode = ref(null);
  const autoPlayEnabled = ref(true);
  const loading = ref(false);
  const error = ref(null);
  const seriesKey = ref(null);

  const watchedEpisodes = computed(() =>
    episodes.value.filter((e) => e.watched).length
  );

  const currentEpisodeIndex = computed(() => {
    if (!currentEpisode.value) return -1;
    return episodes.value.findIndex((e) => e.rating_key === currentEpisode.value.rating_key);
  });

  const nextEpisode = computed(() => {
    const idx = currentEpisodeIndex.value;
    if (idx >= 0 && idx < episodes.value.length - 1) {
      return episodes.value[idx + 1];
    }
    return null;
  });

  const previousEpisode = computed(() => {
    const idx = currentEpisodeIndex.value;
    if (idx > 0) {
      return episodes.value[idx - 1];
    }
    return null;
  });

  async function fetchEpisodes(key) {
    loading.value = true;
    error.value = null;
    seriesKey.value = key;
    try {
      const res = await apiFetch(`/api/series/${encodeURIComponent(key)}/episodes`);
      if (!res.ok) await throwHttpError(res, t("series.loadError"));
      const data = await res.json();
      episodes.value = data;
    } catch (err) {
      error.value = err.message || t("errors.network");
      episodes.value = [];
    } finally {
      loading.value = false;
    }
  }

  async function fetchAutoPlayPreference() {
    try {
      const res = await apiFetch("/api/user/preferences/autoplay");
      if (!res.ok) await throwHttpError(res, t("series.fetchAutoPlayError"));
      const data = await res.json();
      autoPlayEnabled.value = data.enabled ?? true;
    } catch (err) {
      console.error("Failed to fetch auto-play preference:", err);
      autoPlayEnabled.value = true;
    }
  }

  async function setAutoPlayPreference(enabled) {
    try {
      const res = await apiFetch("/api/user/preferences/autoplay", {
        method: "PUT",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ enabled }),
      });
      if (!res.ok) await throwHttpError(res, t("series.saveAutoPlayError"));
      autoPlayEnabled.value = enabled;
    } catch (err) {
      console.error("Failed to set auto-play preference:", err);
      throw err;
    }
  }

  async function updateWatchProgress(ratingKey, watched, watchProgress) {
    try {
      const res = await apiFetch(`/api/series/episodes/${encodeURIComponent(ratingKey)}/progress`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ watched, watch_progress: watchProgress }),
      });
      if (!res.ok) await throwHttpError(res, t("series.updateProgressError"));

      const episode = episodes.value.find((e) => e.rating_key === ratingKey);
      if (episode) {
        episode.watched = watched;
        episode.watch_progress = watchProgress;
      }
    } catch (err) {
      console.error("Failed to update watch progress:", err);
      throw err;
    }
  }

  function setCurrentEpisode(episode) {
    currentEpisode.value = episode;
  }

  function clearEpisodes() {
    episodes.value = [];
    currentEpisode.value = null;
    seriesKey.value = null;
    error.value = null;
  }

  return {
    episodes,
    currentEpisode,
    autoPlayEnabled,
    loading,
    error,
    seriesKey,
    watchedEpisodes,
    currentEpisodeIndex,
    nextEpisode,
    previousEpisode,
    fetchEpisodes,
    fetchAutoPlayPreference,
    setAutoPlayPreference,
    updateWatchProgress,
    setCurrentEpisode,
    clearEpisodes,
  };
});