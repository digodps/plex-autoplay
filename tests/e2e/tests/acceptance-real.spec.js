// Aceitação da stack real: usa navegador de verdade (Chromium) contra a
// aplicação subida por scripts/start.sh, reproduzindo o que o usuário faz.
// Dados dependentes do Plex viram SKIP; erro de UI ou HTTP reprova.
const { test, expect } = require("@playwright/test");
const { loadEnv } = require("../utils/env");

loadEnv();

const ADMIN_PASSWORD = process.env.ADMIN_PASSWORD;
const PASSWORD_INPUT = 'input[type="password"]';
const LOGIN_BUTTON = "button";

async function login(page) {
  await page.goto("/login", { waitUntil: "networkidle" });
  await page.fill(PASSWORD_INPUT, ADMIN_PASSWORD);
  await page.click(LOGIN_BUTTON);
  await page.waitForURL("**/dashboard");
}

test.describe("Aceitação — login no navegador", () => {
  test("senha incorreta mostra erro e mantém o usuário no login", async ({ page }) => {
    await page.goto("/login", { waitUntil: "networkidle" });
    await page.fill(PASSWORD_INPUT, "__acceptance_wrong_password__");
    await page.click(LOGIN_BUTTON);
    await expect(page.locator(".error, .alert-error, [data-testid='error']").first()).toBeVisible();
    await expect(page).toHaveURL(/\/login/);
  });

  test("senha correta abre o dashboard e define o cookie auth_token", async ({ page, context }) => {
    await login(page);
    const cookie = (await context.cookies()).find((c) => c.name === "auth_token");
    expect(cookie?.value).toBeTruthy();
  });
});

test.describe("Aceitação — funções pelo navegador", () => {
  test.beforeEach(async ({ page }) => {
    await login(page);
  });

  test("dashboard renderiza métricas do sistema", async ({ page }) => {
    await page.goto("/dashboard", { waitUntil: "networkidle" });
    await expect(page.locator("h1")).toContainText("Início");
    await expect(page.locator(".dashboard-grid")).toBeVisible();
    await expect(page.locator(".dashboard-error")).toHaveCount(0);
  });

  test("configurações salvam o intervalo de sincronização", async ({ page }) => {
    await page.goto("/settings", { waitUntil: "networkidle" });
    await expect(page.locator("h1")).toContainText("Configurações");
    await page.fill('.setting-group input[type="number"]', "7");
    await page.click('button:has-text("Salvar")');
    await expect(page.locator(".message")).toContainText("salvo com sucesso");
  });

  test("bibliotecas, séries e editor de marcadores abrem pelo navegador", async ({ page }) => {
    await page.goto("/libraries", { waitUntil: "networkidle" });
    await expect(page.locator("h1")).toContainText("Bibliotecas");

    const alert = page.locator(".alert");
    if (await alert.count()) {
      test.skip(true, `Plex indisponível: ${(await alert.first().innerText()).trim()}`);
    }

    const libraries = page.locator(".library-card");
    if ((await libraries.count()) === 0) {
      test.skip(true, "Plex sem bibliotecas de séries");
    }

    await libraries.first().click();
    await page.waitForURL(/\/libraries\/[^/]+\/series/);

    const series = page.locator(".series-card:not(.skeleton)");
    const loaded = page.locator(".series-card:not(.skeleton), .empty");
    await loaded.first().waitFor({ state: "visible", timeout: 30000 });
    if ((await series.count()) === 0) {
      test.skip(true, "biblioteca sem séries");
    }

    await series.first().click();
    await page.waitForLoadState("networkidle");
    await page.waitForURL(/\/series\/[^/]+\/markers/);
    await expect(page.locator(".season-title").first()).toBeVisible();
    await expect(page.locator(".marker-row").first()).toBeVisible();
    await expect(page.locator(".marker-row").first().locator(".btn-icon.add")).toBeVisible();
  });

  test("sincronização inicia o scan pelo navegador e não falha", async ({ page }) => {
    await page.goto("/sync", { waitUntil: "networkidle" });
    await expect(page.locator("h1")).toContainText("Sincronização");

    const badge = page.locator(".status-badge");
    const start = page.locator('button:has-text("Iniciar Escaneamento")');
    if (!(await start.count())) {
      test.skip(true, "job de scan já em andamento");
    }

    await start.click();
    await expect(badge).not.toHaveText("idle", { timeout: 20000 });
    const status = (await badge.innerText()).trim();
    expect(status, "job de scan falhou").not.toBe("failed");

    const cancel = page.locator('button:has-text("Cancelar")');
    if (await cancel.count()) {
      await cancel.click();
      // Scan rápido pode concluir antes do cancelamento chegar ao backend.
      await expect(badge).toHaveText(/cancelled|completed/, { timeout: 20000 });
    }
  });
});
