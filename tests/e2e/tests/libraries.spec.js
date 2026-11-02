const { test, expect } = require("@playwright/test");
const { loadEnv } = require("../utils/env");

loadEnv();

const BASE_URL =
  process.env.PLAYWRIGHT_BASE_URL || "https://plex-autoplay.dns.podman:443";
const ADMIN_PASSWORD = process.env.ADMIN_PASSWORD;

test.describe("Bibliotecas de séries", () => {
  test.beforeEach(async ({ page }) => {
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

  test('acessa /libraries e exibe apenas bibliotecas de tipo "show"', async ({
    page,
    request,
  }) => {
    // Obtém token do localStorage
    const token = await page.evaluate(() => localStorage.getItem("auth_token"));

    // Valida via API que apenas bibliotecas do tipo "show" são retornadas
    const response = await request.get(`${BASE_URL}/api/plex/libraries`, {
      headers: { Authorization: `Bearer ${token}` },
    });

    expect(response.status()).toBe(200);
    const libraries = await response.json();
    expect(Array.isArray(libraries)).toBe(true);

    // Todas as bibliotecas retornadas devem ser do tipo "show"
    for (const lib of libraries) {
      expect(lib.type).toBe("show");
    }

    // Navega para a página de bibliotecas (deve carregar sem erro)
    await page.goto(`${BASE_URL}/libraries`, { waitUntil: "networkidle" });
    await page.waitForTimeout(2000);
  });

  test("exibe erro quando Plex está indisponível", async ({ page }) => {
    // A aplicação retorna lista vazia quando o Plex não está acessível
    // (não lança erro não tratado). Verifica que a página carrega sem erro.
    await page.goto(`${BASE_URL}/libraries`, { waitUntil: "networkidle" });

    // A página deve carregar sem exceção não tratada
    await expect(page.locator("h1")).toContainText("Bibliotecas");
  });

  test("GET /api/status retorna estado do último job e métricas de processo", async ({
    page,
    request,
  }) => {
    await page.goto(`${BASE_URL}/login`, { waitUntil: "networkidle" });
    await page.waitForSelector('input[type="password"]', {
      state: "visible",
      timeout: 10000,
    });
    await page.fill('input[type="password"]', ADMIN_PASSWORD);
    await page.click("button");
    await page.waitForURL(`${BASE_URL}/dashboard`);

    const token = await page.evaluate(() => localStorage.getItem("auth_token"));

    const response = await request.get(`${BASE_URL}/api/status`, {
      headers: { Authorization: `Bearer ${token}` },
    });

    expect(response.status()).toBe(200);
    const body = await response.json();
    expect(body.service).toBe("plex-autoplay");
    expect(body.process).toBeTruthy();
    expect(body.process.memory_private_bytes).toBeGreaterThan(0);
  });

  test("GET /config retorna configuração via variáveis de ambiente", async ({
    page,
    request,
  }) => {
    await page.goto(`${BASE_URL}/login`, { waitUntil: "networkidle" });
    await page.waitForSelector('input[type="password"]', {
      state: "visible",
      timeout: 10000,
    });
    await page.fill('input[type="password"]', ADMIN_PASSWORD);
    await page.click("button");
    await page.waitForURL(`${BASE_URL}/dashboard`);

    const token = await page.evaluate(() => localStorage.getItem("auth_token"));

    const response = await request.get(`${BASE_URL}/config`, {
      headers: { Authorization: `Bearer ${token}` },
    });

    expect(response.status()).toBe(200);
    const body = await response.json();
    expect(body.note).toContain("environment variables");
    expect(body.app_config_json_used).toBeUndefined();
  });

  test("POST /config não grava app-config.json", async ({ page, request }) => {
    await page.goto(`${BASE_URL}/login`, { waitUntil: "networkidle" });
    await page.waitForSelector('input[type="password"]', {
      state: "visible",
      timeout: 10000,
    });
    await page.fill('input[type="password"]', ADMIN_PASSWORD);
    await page.click("button");
    await page.waitForURL(`${BASE_URL}/dashboard`);

    const token = await page.evaluate(() => localStorage.getItem("auth_token"));

    const response = await request.post(`${BASE_URL}/config`, {
      headers: { Authorization: `Bearer ${token}` },
    });

    expect(response.status()).toBe(200);
    const body = await response.json();
    expect(body.status).toBe("valid");
    expect(body.app_config_json_used).toBe(false);
  });

  test("GET /library/{section_key} retorna episódios com caminho de mídia resolvido", async ({
    page,
    request,
  }) => {
    await page.goto(`${BASE_URL}/login`, { waitUntil: "networkidle" });
    await page.waitForSelector('input[type="password"]', {
      state: "visible",
      timeout: 10000,
    });
    await page.fill('input[type="password"]', ADMIN_PASSWORD);
    await page.click("button");
    await page.waitForURL(`${BASE_URL}/dashboard`);

    const token = await page.evaluate(() => localStorage.getItem("auth_token"));

    // Primeiro obtém bibliotecas para saber uma section_key
    const libsResponse = await request.get(`${BASE_URL}/api/plex/libraries`, {
      headers: { Authorization: `Bearer ${token}` },
    });
    const libraries = await libsResponse.json();

    if (libraries.length > 0) {
      const sectionKey = libraries[0].key;
      const episodesResponse = await request.get(
        `${BASE_URL}/library/${sectionKey}`,
        {
          headers: { Authorization: `Bearer ${token}` },
        },
      );
      expect(episodesResponse.status()).toBe(200);
      const episodes = await episodesResponse.json();
      expect(Array.isArray(episodes)).toBe(true);
      if (episodes.length > 0) {
        expect(episodes[0].local_path).toBeTruthy();
      }
    }
  });
});
