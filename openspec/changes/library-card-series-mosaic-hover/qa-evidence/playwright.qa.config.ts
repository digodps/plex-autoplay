import { defineConfig } from "@playwright/test";

export default defineConfig({
  testDir: "/home/hermes/plex-autoplay-library-card-shape-magic-hover/openspec/changes/library-card-series-mosaic-hover/qa-evidence",
  use: {
    baseURL: "http://localhost:8080",
    headless: true,
    viewport: { width: 1280, height: 800 },
  },
});