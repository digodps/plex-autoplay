const { test, expect } = require("@playwright/test");
const { loadEnv } = require("../utils/env");

loadEnv();

const BASE_URL = process.env.PLAYWRIGHT_BASE_URL || "https://plex-autoplay.dns.podman:443";
const ADMIN_PASSWORD = process.env.ADMIN_PASSWORD;

test.describe("Autenticação JWT", () => {
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

  test("token JWT é armazenado em cookie após login", async ({ page, context }) => {
    await page.goto(`${BASE_URL}/login`, { waitUntil: "networkidle" });
    await page.waitForSelector('input[type="password"]', {
      state: "visible",
      timeout: 10000,
    });

    await page.fill('input[type="password"]', ADMIN_PASSWORD);
    await page.click("button");

    // Aguarda navegação e verifica token no cookie
    await page.waitForURL(`${BASE_URL}/dashboard`);
    const cookies = await context.cookies();
    const authCookie = cookies.find((c) => c.name === "auth_token");
    expect(authCookie).toBeTruthy();
    expect(authCookie.value.length).toBeGreaterThan(0);
    // Sem "manter-me conectado": cookie de sessão (sem max-age/expiry)
    expect(authCookie.expires).toBe(-1);

    // localStorage não deve mais guardar o token
    const localStorageToken = await page.evaluate(() => localStorage.getItem("auth_token"));
    expect(localStorageToken).toBeFalsy();
  });

  test("login com senha incorreta retorna HTTP 401 e exibe erro", async ({ page }) => {
    await page.goto(`${BASE_URL}/login`, { waitUntil: "networkidle" });
    await page.waitForSelector('input[type="password"]', {
      state: "visible",
      timeout: 10000,
    });

    await page.fill('input[type="password"]', "senha_errada");
    await page.click("button");

    // Verifica mensagem de erro exibida
    const errorMsg = page.locator('.error, [data-testid="error"], .alert-error');
    await expect(errorMsg.first()).toBeVisible();
    const errorText = await errorMsg.first().textContent();
    expect(errorText.toLowerCase()).toMatch(/inválida|erro|401/);
  });

  test("acesso a endpoint protegido sem token retorna 401", async ({ request }) => {
    const response = await request.get(`${BASE_URL}/api/libraries`);
    expect(response.status()).toBe(401);
  });

  test("rota interna sem sessão redireciona ao login e retorna à rota original após autenticação", async ({
    page,
  }) => {
    // Acessa rota interna sem sessão
    await page.goto(`${BASE_URL}/series`, { waitUntil: "networkidle" });

    // Guard manda para o login preservando o destino
    await page.waitForURL(/\/login\?redirect=/, { timeout: 15000 });
    expect(decodeURIComponent(page.url())).toContain("/login?redirect=/series");

    // Autentica
    await page.waitForSelector('input[type="password"]', { state: "visible" });
    await page.fill('input[type="password"]', ADMIN_PASSWORD);
    await page.click("button");

    // Retorna à rota original, não ao dashboard
    await page.waitForURL(`${BASE_URL}/series`, { timeout: 15000 });
  });
});
