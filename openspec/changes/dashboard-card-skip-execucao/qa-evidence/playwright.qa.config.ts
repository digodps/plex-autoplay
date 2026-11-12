import { defineConfig } from '@playwright/test';

export default defineConfig({
  testDir: '/home/hermes/plex-autoplay-dashboard-card-skip-execucao/openspec/changes/dashboard-card-skip-execucao/qa-evidence',
  use: {
    baseURL: 'http://localhost:8080',
    headless: true,
    viewport: { width: 1280, height: 800 },
  },
});
