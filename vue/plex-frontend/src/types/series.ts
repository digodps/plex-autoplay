export interface SeriesEpisode {
  rating_key: string;
  title: string;
  season: number;
  episode: number;
  local_path: string;
  thumb: string | null;
  watched: boolean;
  watch_progress: number;
  duration_seconds: number;
}

export interface SeriesDetail {
  seriesKey: string;
  episodes: SeriesEpisode[];
  autoPlayEnabled: boolean;
}

export interface WatchProgressRequest {
  watched: boolean;
  watch_progress: number;
}

export interface AutoPlayPreferenceRequest {
  enabled: boolean;
}