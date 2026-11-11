const { test, expect } = require("@playwright/test");
const { loadEnv } = require("../utils/env");
const { loginByUi, bearerToken } = require("../utils/auth");

loadEnv();

<<<<<<< HEAD
const BASE_URL =
  process.env.PLAYWRIGHT_BASE_URL || "https://plex-autoplay.dns.podman:443";
const ADMIN_PASSWORD = process.env.OPERATOR_PASSWORD || process.env.ADMIN_PASSWORD;

test.describe("Marcadores (CRUD)", () => {
  test("lista marcadores via GET /api/markers", async ({ page, request }) => {
    // Faz login
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
=======
const BASE_URL = process.env.PLAYWRIGHT_BASE_URL || "https://plex-autoplay.dns.podman:443";

test.describe("Marcadores (CRUD)", () => {
  test.beforeEach(async ({ page }) => {
    await loginByUi(page);
  });

  test("lista marcadores via GET /api/markers exibindo origem", async ({ context, request }) => {
    const token = await bearerToken(context);
>>>>>>> origin/feature/dashboard-redesign-31-10-2026-13-29-43-856
    const response = await request.get(`${BASE_URL}/api/markers`, {
      headers: { Authorization: `Bearer ${token}` },
    });

    expect(response.status()).toBe(200);
    const markers = await response.json();
    expect(Array.isArray(markers)).toBe(true);
    // campo origin no contrato aditivo
    markers.forEach((m) => {
      expect(["manual", "detected"]).toContain(m.origin);
    });
  });

<<<<<<< HEAD
  test("marcador criado via API é gravado", async ({ page, request }) => {
    // Faz login
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
=======
  test("cria marcador via POST query string (compat E2E legado) e grava origem manual", async ({
    context,
    request,
  }) => {
    const token = await bearerToken(context);
    const ratingKey = `e2e-${Date.now()}`;
    const kind = "intro";

    const response = await request.post(
      `${BASE_URL}/api/markers?RatingKey=${ratingKey}&Kind=${kind}&StartMs=1000&EndMs=90000`,
      { headers: { Authorization: `Bearer ${token}` } },
    );
    expect(response.status()).toBe(201);
    const body = await response.json();
    expect(body.origin).toBe("manual");

    // duplicado retorna 409 (contrato mantido)
    const dup = await request.post(
      `${BASE_URL}/api/markers?RatingKey=${ratingKey}&Kind=${kind}&StartMs=1000&EndMs=90000`,
      { headers: { Authorization: `Bearer ${token}` } },
    );
    expect(dup.status()).toBe(409);

    const del = await request.delete(`${BASE_URL}/api/markers/${ratingKey}/${kind}`, {
      headers: { Authorization: `Bearer ${token}` },
    });
    expect(del.status()).toBe(200);
  });

  test("edita marcador existente via PUT, preservando origem", async ({ context, request }) => {
    const token = await bearerToken(context);
    const ratingKey = `e2e-put-${Date.now()}`;
    const kind = "intro";

    await request.post(
      `${BASE_URL}/api/markers?RatingKey=${ratingKey}&Kind=${kind}&StartMs=1000&EndMs=90000`,
      { headers: { Authorization: `Bearer ${token}` } },
    );

    const put = await request.put(`${BASE_URL}/api/markers/${ratingKey}/${kind}`, {
      headers: { Authorization: `Bearer ${token}` },
      data: { startMs: 2000, endMs: 95000 },
    });
    expect(put.status()).toBe(200);
    const body = await put.json();
    expect(body.startMs).toBe(2000);
    expect(body.origin).toBe("manual");

    // Valor atualizado na listagem
    const list = await request.get(`${BASE_URL}/api/markers/${ratingKey}`, {
      headers: { Authorization: `Bearer ${token}` },
    });
    const markers = await list.json();
    const marker = markers.find((m) => m.kind === kind);
    expect(marker.startMs).toBe(2000);

    await request.delete(`${BASE_URL}/api/markers/${ratingKey}/${kind}`, {
      headers: { Authorization: `Bearer ${token}` },
    });
  });

  test("PUT em marcador inexistente retorna 404", async ({ context, request }) => {
    const token = await bearerToken(context);
    const put = await request.put(`/api/markers/e2e-nao-existe/intro`, {
      headers: { Authorization: `Bearer ${token}` },
      data: { startMs: 1, endMs: 2 },
    });
    expect(put.status()).toBe(404);
  });

  test("rotas mutantes sem token são rejeitadas", async ({ request }) => {
    const response = await request.post(`${BASE_URL}/api/markers`, {
      headers: { "Content-Type": "application/json" },
      data: { RatingKey: "no-auth", Kind: "intro", StartMs: 0, EndMs: 90000 },
    });
    expect([401, 403]).toContain(response.status());
  });
});
>>>>>>> origin/feature/dashboard-redesign-31-10-2026-13-29-43-856
