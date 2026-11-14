import { defineConfig } from '@playwright/test';

export default defineConfig({
  testDir: './tests/e2e/tests',
  use: {
    baseURL: 'http://127.0.0.1:8080',
    headless: true,
    viewport: { width: 1280, height: 800 },
    launchOptions: {
      args: ['--no-sandbox', '--disable-setuid-sandbox', '--disable-dev-shm-usage', '--disable-gpu'],
    },
  },
  timeout: 120000,
});