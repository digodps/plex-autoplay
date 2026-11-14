import { defineConfig } from '@playwright/test';

export default defineConfig({
  testDir: '/home/hermes/plex-autoplay-refactor-session-mobile-layout/openspec/changes/refactor-session-mobile-layout/qa-evidence',
  use: {
    baseURL: 'http://localhost:4173',
    headless: true,
    viewport: { width: 1280, height: 800 },
  },
});