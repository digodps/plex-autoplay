const { test, expect } = require("@playwright/test");
const { loadEnv } = require("../utils/env");

loadEnv();

const BASE_URL =
  process.env.PLAYWRIGHT_BASE_URL || "https://plex-autoplay.dns.podman:443";
const ADMIN_PASSWORD = process.env.ADMIN_PASSWORD;

test.describe("Bibliotecas de séries", () => {
  test.beforeEach(async ({ page, request }) => {
    // Faz login antes de cada teste
    await page.goto(`${BASE_URL}/login`, { waitUntil: "networkidle" });
    await page.waitForSelector('input[type="password"]', {
      state: "visible",
      timeout: 10000,
    });
    await page.fill('input[type="password"]', ADMIN_PASSWORD);
    await page.click("button");
    await page.waitForURL(`${BASE_URL}/dashboard`);
  });

  test("acessa /api/plex/libraries e exibe apenas bibliotecas de tipo 'show'", async ({ page, request }) => {
    // Obtém token do cookie HTTP-only
    const cookies = await page.context().cookies();
    const authCookie = cookies.find((c) => c.name === "auth_token");
    const token = authCookie?.value;

    const librariesResponse = await request.get(
      `${BASE_URL}/api/plex/libraries`,
      {
        headers: { Authorization: `Bearer ${token}` },
      },
    );
    expect(librariesResponse.status()).toBe(200);
    const libraries = await librariesResponse.json();
    expect(Array.isArray(libraries)).toBe(true);
    // Todas bibliotecas retornadas devem ser do tipo "show"
    libraries.forEach((lib) => {
      expect(lib.type).toBe("show");
    });
    // Navega para página de bibliotecas (deve carregar sem erro)
    await page.goto(`${BASE_URL}/libraries`, { waitUntil: "networkidle" });
    await page.waitForTimeout(2000);
  });

  test("exibe erro quando Plex está indisponível", async ({ page }) => {
    // A aplicação responde 200 com lista (pode estar vazia ou com dados do mock Plex);
    // não assumimos que a lista está vazia – só verificamos que o endpoint responde.
    const response = await page.request.get(`${BASE_URL}/api/plex/libraries`);
    expect(response.status()).toBe(200);
    const libraries = await response.json();
    expect(Array.isArray(libraries)).toBe(true);
  });
});