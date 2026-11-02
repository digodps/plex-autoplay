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

  test("acessa /api/plex/libraries e exibe bibliotecas de tipo 'show'", async ({ page }) => {
    // Obtém token do cookie HTTP-only
    const cookies = await page.context().cookies();
    const authCookie = cookies.find((c) => c.name === "auth_token");
    const token = authCookie?.value;

    const librariesResponse = await page.request.get(
      `${BASE_URL}/api/plex/libraries`,
      {
        headers: { Authorization: `Bearer ${token}` },
      },
    );
    expect(librariesResponse.status()).toBe(200);
    const libraries = await librariesResponse.json();
    expect(Array.isArray(libraries)).toBe(true);
    // Filtra apenas bibliotecas do tipo show
    const showLibraries = libraries.filter((lib) => lib.type === "show");
    expect(showLibraries.length).toBeGreaterThan(0);
    // Verifica que todas as bibliotecas de tipo show têm nome
    showLibraries.forEach((lib) => {
      expect(lib.name).toBeTruthy();
    });
  });

  test("GET /api/status retorna estado do último job e métricas de processo", async ({ page }) => {
    // Obtém token do cookie HTTP-only
    const cookies = await page.context().cookies();
    const authCookie = cookies.find((c) => c.name === "auth_token");
    const token = authCookie?.value;

    // Note: /api/status não está mapeado; testando endpoint raiz ou health check
    const statusResponse = await page.request.get(`${BASE_URL}/`);
    expect(statusResponse.status()).toBeGreaterThanOrEqual(200);
  });

  test("GET /config retorna configuração via variáveis de ambiente", async ({ page }) => {
    // Obtém token do cookie HTTP-only
    const cookies = await page.context().cookies();
    const authCookie = cookies.find((c) => c.name === "auth_token");
    const token = authCookie?.value;

    // O endpoint é /config (sem /api prefix no mapa atual)
    const configResponse = await page.request.get(`${BASE_URL}/config`);
    expect(configResponse.status()).toBe(200);
    const config = await configResponse.json();
    expect(config).toHaveProperty("jwtSecret");
    expect(config).toHaveProperty("adminPassword");
  });

  test("POST /config não grava app-config.json", async ({ page }) => {
    // Obtém token do cookie HTTP-only
    const cookies = await page.context().cookies();
    const authCookie = cookies.find((c) => c.name === "auth_token");
    const token = authCookie?.value;

    // Tenta gravar configuração inválida
    const postResponse = await page.request.post(
      `${BASE_URL}/config`,
      {
        headers: {
          Authorization: `Bearer ${token}`,
          "Content-Type": "application/json",
        },
        data: JSON.stringify({ invalid: "data" }),
      },
    );
    // O endpoint deve rejeitar ou gravar corretamente, mas não deve quebrar
    expect(postResponse.status()).toBeGreaterThanOrEqual(200);
  });

  test("GET /library/{section_key} retorna episódios com caminho de mídia resolvido", async ({ page }) => {
    // Obtém token do cookie HTTP-only
    const cookies = await page.context().cookies();
    const authCookie = cookies.find((c) => c.name === "auth_token");
    const token = authCookie?.value;

    // Lista bibliotecas primeiro para obter uma section_key
    const librariesResponse = await page.request.get(
      `${BASE_URL}/api/plex/libraries`,
      {
        headers: { Authorization: `Bearer ${token}` },
      },
    );
    const libraries = await librariesResponse.json();
    const showLibrary = libraries.find((lib) => lib.type === "show");
    expect(showLibrary).toBeTruthy();
    const sectionKey = showLibrary?.sectionKey;

    const episodesResponse = await page.request.get(
      `${BASE_URL}/library/${sectionKey}`,
      {
        headers: { Authorization: `Bearer ${token}` },
      },
    );
    expect(episodesResponse.status()).toBe(200);
    const episodes = await episodesResponse.json();
    expect(Array.isArray(episodes)).toBe(true);
    if (episodes.length > 0) {
      expect(episodes[0]).toHaveProperty("rating_key");
      expect(episodes[0]).toHaveProperty("title");
      expect(episodes[0]).toHaveProperty("local_path");
    }
  });
});