const { defineConfig, devices } = require("@playwright/test");

module.exports = defineConfig({
  testDir: "./tests",
  fullyParallel: true,
  forbidOnly: !!process.env.CI,
  retries: process.env.CI ? 2 : 0,
  workers: process.env.CI ? 2 : undefined,
  reporter: "html",

  // Timeout global de 5 minutos para a suíte
  timeout: 5 * 60 * 1000,

  use: {
    baseURL: process.env.PLAYWRIGHT_BASE_URL || "http://plex-autoplay:80",
    trace: "on-first-retry",
    screenshot: "only-on-failure",
    // Timeout de cada teste individual
    actionTimeout: 10 * 1000,
    navigationTimeout: 30 * 1000,
    // A aplicação no container de teste expõe HTTP (não HTTPS)
    ignoreHTTPSErrors: true,
  },

  projects: [
    {
      name: "chromium",
      use: { ...devices["Desktop Chrome"] },
    },
  ],

  // O servidor web já está rodando via docker-compose (serviço plex-autoplay).
  // Não é necessário webServer pois a aplicação já está em execução na rede Docker.
});
