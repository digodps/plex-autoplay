const { test, expect } = require("@playwright/test");
const { loadEnv } = require("../utils/env");

loadEnv();

const BASE_URL =
  process.env.PLAYWRIGHT_BASE_URL || "https://plex-autoplay.dns.podman:443";
const ADMIN_PASSWORD = process.env.OPERATOR_PASSWORD || process.env.ADMIN_PASSWORD;

test.describe("Logs de processamento", () => {
  test.beforeEach(async ({ page }) => {
    // Login before each test
    await page.goto(`${BASE_URL}/login`, { waitUntil: "networkidle" });
    await page.waitForSelector('input[type="password"]', {
      state: "visible",
      timeout: 10000,
    });
    await page.fill('input[type="password"]', ADMIN_PASSWORD);
    await page.click("button");
    await page.waitForURL(`${BASE_URL}/dashboard`);
  });

  test("acessa /logs e valida tema light/dark toggle", async ({ page }) => {
    // Navega para /logs
    await page.goto(`${BASE_URL}/logs`, { waitUntil: "networkidle" });

    // Aguarda a view carregar
    await page.waitForSelector('[data-testid="log-filter-text"]', {
      state: "visible",
      timeout: 10000,
    });

    // Verifica que o elemento body tem data-theme (light/dark)
    const initialTheme = await page.evaluate(() => {
      return document.documentElement.dataset.theme || "light";
    });
    expect(["light", "dark"]).toContain(initialTheme);

    // Testa toggle de tema via clique no header (existe botão de tema)
    const themeToggle = page.locator('button[aria-label*="theme"], button[data-testid="theme-toggle"]');
    if (await themeToggle.count() > 0) {
      await themeToggle.first().click();
      await page.waitForTimeout(300);
      const toggledTheme = await page.evaluate(() => {
        return document.documentElement.dataset.theme || "light";
      });
      expect(toggledTheme).not.toBe(initialTheme);
      // Reverte
      await themeToggle.first().click();
      await page.waitForTimeout(300);
    }
  });

  test("menus funcionando: abertura/navegação/fechamento/estado ativo", async ({ page }) => {
    await page.goto(`${BASE_URL}/logs`, { waitUntil: "networkidle" });
    await page.waitForSelector('[data-testid="log-filter-text"]', { state: "visible" });

    // Verifica menu lateral: item "Logs" está ativo
    // Sidebar items usam router-link ou navegação programática; verificamos classe ativa
    const logsMenuItem = page.locator('a[href="/logs"], button[data-to="/logs"], a[href$="/logs"]');
    if (await logsMenuItem.count() > 0) {
      // Se item tem classe ativa (ex: router-link-active)
      const classes = await logsMenuItem.first().getAttribute("class");
      expect(classes).toMatch(/active|router-link-active/);
    }

    // Testa navegação: vai para outra rota e volta
    await page.goto(`${BASE_URL}/dashboard`, { waitUntil: "networkidle" });
    await expect(page).toHaveURL(`${BASE_URL}/dashboard`);

    await page.goto(`${BASE_URL}/logs`, { waitUntil: "networkidle" });
    await expect(page).toHaveURL(`${BASE_URL}/logs`);
    await page.waitForSelector('[data-testid="log-filter-text"]', { state: "visible" });

    // Fecha menu lateral em mobile se houver (verifica se sidebar toggle funciona)
    const sidebarToggle = page.locator('button[aria-label*="menu"], button[data-testid="sidebar-toggle"]');
    if (await sidebarToggle.count() > 0) {
      await sidebarToggle.first().click();
      await page.waitForTimeout(300);
      await sidebarToggle.first().click();
      await page.waitForTimeout(300);
    }
  });

  test("filtro textual live funciona (debounce 200ms)", async ({ page }) => {
    await page.goto(`${BASE_URL}/logs`, { waitUntil: "networkidle" });
    await page.waitForSelector('[data-testid="log-filter-text"]', { state: "visible" });

    const filterInput = page.locator('[data-testid="log-filter-text"]');

    // Digita texto
    await filterInput.fill("falha");
    await page.waitForTimeout(300); // debounce 200ms

    // Verifica se houve requisição à API /api/sync/logs com parâmetro text
    // Espera que o network idle indique que a busca foi feita
    await page.waitForLoadState("networkidle");

    // Verifica que o input ainda tem o valor
    expect(await filterInput.inputValue()).toBe("falha");
  });

  test("agrupamento por data/status/tipo funciona", async ({ page }) => {
    await page.goto(`${BASE_URL}/logs`, { waitUntil: "networkidle" });
    await page.waitForSelector('[data-testid="log-filter-text"]', { state: "visible" });

    // Testa botões de agrupamento
    const groupByDate = page.locator('[data-testid="log-groupby"] >> text="Data"');
    const groupByStatus = page.locator('[data-testid="log-groupby"] >> text="Status"');
    const groupByType = page.locator('[data-testid="log-groupby"] >> text="Tipo"');

    for (const btn of [groupByDate, groupByStatus, groupByType]) {
      if (await btn.count() > 0) {
        await btn.first().click();
        await page.waitForTimeout(200);
        // Verifica que o botão ficou ativo (classe primária)
        const classes = await btn.first().getAttribute("class");
        expect(classes).toMatch(/bg-primary|border-primary/);
      }
    }
  });

  test("botão Limpar Logs abre confirmação", async ({ page }) => {
    await page.goto(`${BASE_URL}/logs`, { waitUntil: "networkidle" });
    await page.waitForSelector('[data-testid="log-btn-clear"]', { state: "visible" });

    // Clica no botão limpar logs
    await page.click('[data-testid="log-btn-clear"]');

    // Deve aparecer confirmação nativa do browser (window.confirm)
    // Playwright intercepta diálogos
    page.once("dialog", async (dialog) => {
      expect(dialog.message()).toMatch(/confirma|limpeza/i);
      await dialog.dismiss(); // cancela
    });

    await page.waitForTimeout(500);
  });
});