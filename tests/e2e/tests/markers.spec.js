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

  test("lista marcadores via GET /api/markers", async ({ page, request }) => {
    // Faz login para obter token
    await page.goto(`${BASE_URL}/login`, { waitUntil: "networkidle" });
    await page.waitForSelector('input[type="password"]', {
      state: "visible",
      timeout: 10000,
    });
    await page.fill('input[type="password"]', ADMIN_PASSWORD);
    await page.click("button");
    await page.waitForURL(`${BASE_URL}/dashboard`);

    // Obtém token do cookie HTTP-only
    const cookies = await page.context().cookies();
    const authCookie = cookies.find((c) => c.name === "auth_token");
    const token = authCookie?.value;

    // Chama a API de marcadores
    const response = await request.get(`${BASE_URL}/api/markers`, {
      headers: { Authorization: `Bearer ${token}` },
    });

    expect(response.status()).toBe(200);
    const markers = await response.json();
    expect(Array.isArray(markers)).toBe(true);
  });

  test("marcador criado via API é gravado", async ({ page, request }) => {
    // Obtém token do cookie HTTP-only
    const cookies = await page.context().cookies();
    const authCookie = cookies.find((c) => c.name === "auth_token");
    const token = authCookie?.value;

    const ratingKey = `e2e-${Date.now()}`;

    // Cria marcador via API
    await page.request.post(
      `${BASE_URL}/api/markers`,
      {
        headers: {
          Authorization: `Bearer ${token}`,
          "Content-Type": "application/json",
        },
        data: JSON.stringify({
          ratingKey: ratingKey,
          kind: "intro",
          startMs: 1000,
          endMs: 5000,
        }),
      },
    );

    // Verifica que o marcador apareceu na listagem
    const listResponse = await request.get(
      `${BASE_URL}/api/markers`,
      {
        headers: { Authorization: `Bearer ${token}` },
      },
    );
    expect(listResponse.status()).toBe(200);
    const markers = await listResponse.json();
    expect(Array.isArray(markers)).toBe(true);
    const created = markers.find((m) => m.ratingKey === ratingKey);
    expect(created).toBeTruthy();
  });
});