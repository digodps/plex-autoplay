const { defineConfig, devices } = require("@playwright/test");

// Aceitação da stack real (scripts/start.sh), não do compose de teste.
// Roda apenas o spec de aceitação, com navegador de verdade.
module.exports = defineConfig({
  testDir: "./tests",
  testMatch: /acceptance-real\.spec\.js/,
  fullyParallel: false,
  workers: 1,
  reporter: "list",
  timeout: 90 * 1000,

  use: {
    baseURL: process.env.ACCEPTANCE_BASE_URL || "https://localhost:8443",
    ignoreHTTPSErrors: true,
    trace: "retain-on-failure",
    screenshot: "only-on-failure",
    actionTimeout: 10 * 1000,
    navigationTimeout: 30 * 1000,
  },

  projects: [
    {
      name: "chromium",
      use: { ...devices["Desktop Chrome"] },
    },
  ],
});
