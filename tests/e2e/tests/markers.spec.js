const { test, expect } = require("@playwright/test");
const { loadEnv } = require("../utils/env");

loadEnv();

const BASE_URL =
  process.env.PLAYWRIGHT_BASE_URL || "https://plex-autoplay.dns.podman:443";
const ADMIN_PASSWORD = process.env.ADMIN_PASSWORD;

test.describe("Marcadores (CRUD)", () => {
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

  test("cria marcador manual via formulário", async ({ page }) => {
    // Obtém token do cookie HTTP-only
    const cookies = await page.context().cookies();
    const authCookie = cookies.find((c) => c.name === "auth_token");
    const token = authCookie?.value;

    // Navega para a página de marcadores
    await page.goto(`${BASE_URL}/markers`, { waitUntil: "networkidle" });

    // Preenche o formulário de criação de marcador
    await page.waitForSelector('input[data-testid="marker-title"]', {
      state: "visible",
      timeout: 10000,
    });
    await page.fill('input[data-testid="marker-title"]', "Marcador Teste E2E");
    await page.fill('input[data-testid="marker-note"]', "Teste de criação de marcador");
    await page.click("button[data-testid=\"create-marker\"]");

    // Verifica que o marcador foi criado
    await expect(page.locator(".marker-item, [data-testid=\"marker-item\"]")).toContainText(
      "Marcador Teste E2E",
      { timeout: 5000 },
    );
  });

  test("remove marcador existente", async ({ page }) => {
    // Obtém token do cookie HTTP-only
    const cookies = await page.context().cookies();
    const authCookie = cookies.find((c) => c.name === "auth_token");
    const token = authCookie?.value;

    // Navega para a página de marcadores
    await page.goto(`${BASE_URL}/markers`, { waitUntil: "networkidle" });

    // Remove o primeiro marcador existente
    const firstMarker = page.locator(".marker-item, [data-testid=\"marker-item\"]").first();
    const markerCountBefore = await page.locator(".marker-item, [data-testid=\"marker-item\"]").count();

    await firstMarker.click();
    await page.click("button[data-testid=\"delete-marker\"]");

    // Verifica que o marcador foi removido
    const markerCountAfter = await page.locator(".marker-item, [data-testid=\"marker-item\"]").count();
    expect(markerCountAfter).toBeLessThan(markerCountBefore);
  });

  test("lista marcadores via GET /api/markers", async ({ page }) => {
    // Obtém token do cookie HTTP-only
    const cookies = await page.context().cookies();
    const authCookie = cookies.find((c) => c.name === "auth_token");
    const token = authCookie?.value;

    const markersResponse = await page.request.get(
      `${BASE_URL}/api/markers`,
      {
        headers: { Authorization: `Bearer ${token}` },
      },
    );
    expect(markersResponse.status()).toBe(200);
    const markers = await markersResponse.json();
    expect(Array.isArray(markers)).toBe(true);
  });

  test("marcador criado via API é gravado em custom.json", async ({ page }) => {
    // Obtém token do cookie HTTP-only
    const cookies = await page.context().cookies();
    const authCookie = cookies.find((c) => c.name === "auth_token");
    const token = authCookie?.value;

    // Cria marcador via API
    await page.request.post(
      `${BASE_URL}/api/markers`,
      {
        headers: {
          Authorization: `Bearer ${token}`,
          "Content-Type": "application/json",
        },
        data: JSON.stringify({
          title: "Marcador API Test",
          note: "Teste via API",
        }),
      },
    );

    // Verifica que o custom.json foi atualizado
    const markersResponse = await page.request.get(
      `${BASE_URL}/api/markers`,
      {
        headers: { Authorization: `Bearer ${token}` },
      },
    );
    const markers = await markersResponse.json();
    const createdMarker = markers.find(
      (m) => m.title === "Marcador API Test",
    );
    expect(createdMarker).toBeTruthy();
  });
});