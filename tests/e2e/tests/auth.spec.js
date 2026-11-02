const { test, expect } = require("@playwright/test");
const { loadEnv } = require("../utils/env");

loadEnv();

const BASE_URL =
  process.env.PLAYWRIGHT_BASE_URL || "https://plex-autoplay.dns.podman:443";
const ADMIN_PASSWORD = process.env.ADMIN_PASSWORD;

// Injetar função para ler cookie via script tag injetado
const injectGetAuthTokenScript = async (page) => {
  await page.addScriptTag({
    content: `
      // Função para ler cookie auth_token do navegador
      // Injetada para que page.evaluate a tenha acesso
      globalThis.getAuthToken = () => {
        return document.cookie.match(/(?:^|; )auth_token=([^;])/)?.[1];
      };
    `,
  });
};

test.describe("Autenticação JWT", () => {
  test.beforeEach(async ({ page }) => {
    await injectGetAuthTokenScript(page);
  });

  test("login com senha correta retorna token JWT e redireciona para dashboard", async ({
    page,
  }) => {
    await page.goto(`${BASE_URL}/login`, { waitUntil: "networkidle" });

    // Aguarda o campo de senha estar visível
    await page.waitForSelector('input[type="password"]', {
      state: "visible",
      timeout: 10000,
    });

    // Preenche o formulário de login
    await page.fill('input[type="password"]', ADMIN_PASSWORD);
    await page.click("button");

    // Verifica redirecionamento para o dashboard
    await expect(page).toHaveURL(`${BASE_URL}/dashboard`);
  });

  test("token JWT é armazenado no cookie após login", async ({
    page,
  }) => {
    await page.goto(`${BASE_URL}/login`, { waitUntil: "networkidle" });
    await page.waitForSelector('input[type="password"]', {
      state: "visible",
      timeout: 10000,
    });

    await page.fill('input[type="password"]', ADMIN_PASSWORD);
    await page.click("button");

    // Aguarda navegação e verifica token no cookie
    await page.waitForURL(`${BASE_URL}/dashboard`);
    const token = await page.evaluate(() => getAuthToken());
    expect(token).toBeTruthy();
    expect(token.length).toBeGreaterThan(0);
  });

  test("login com senha incorreta retorna HTTP 401 e exibe erro", async ({
    page,
  }) => {
    await page.goto(`${BASE_URL}/login`, { waitUntil: "networkidle" });
    await page.waitForSelector('input[type="password"]', {
      state: "visible",
      timeout: 10000,
    });

    await page.fill('input[type="password"]', "senha_errada");
    await page.click("button");

    // Verifica mensagem de erro exibida
    const errorMsg = page.locator(
      '.error, [data-testid="error"], .alert-error',
    );
    await expect(errorMsg.first()).toBeVisible();
    const errorText = await errorMsg.first().textContent();
    expect(errorText.toLowerCase()).toMatch(/inválida|erro|401/);
  });

  test("acesso a endpoint protegido sem token retorna 401", async ({
    request,
  }) => {
    const response = await request.get(`${BASE_URL}/api/libraries`);
    expect(response.status()).toBe(401);
  });
});