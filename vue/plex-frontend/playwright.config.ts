import { defineConfig } from '@playwright/test';

export default defineConfig({
  testDir: '/home/hermes/plex-autoplay-dashboard-botoes-tamanhos/openspec/changes/dashboard-botoes-tamanhos/qa-evidence',
  use: {
    baseURL: 'http://localhost:8080',
    headless: true,
    viewport: { width: 1280, height: 800 },
  },
});