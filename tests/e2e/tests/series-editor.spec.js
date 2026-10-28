const { test, expect } = require("@playwright/test");
const { loadEnv } = require("../utils/env");
const { loginByUi } = require("../utils/auth");

loadEnv();

const BASE_URL = process.env.PLAYWRIGHT_BASE_URL || "https://plex-autoplay.dns.podman:443";

test.describe("Catálogo de séries e editor de marcadores (UI)", () => {
  test.beforeEach(async ({ page, context, request }) => {
    await loginByUi(page);

    // Limpeza: execuções anteriores podem ter persistido marcadores nos
    // episódios do mock (o banco SQLite sobrevive ao container). Garante
    // estado limpo e testes idempotentes.
    const cookies = await context.cookies();
    const token = cookies.find((c) => c.name === "auth_token")?.value ?? "";
    for (const ratingKey of ["5001", "5002", "6001"]) {
      for (const kind of ["intro", "credits"]) {
        await request.delete(`${BASE_URL}/api/markers/${ratingKey}/${kind}`, {
          headers: { Authorization: `Bearer ${token}` },
        });
      }
    }
  });

  test("página de séries exibe capas com descrição do catálogo", async ({ page }) => {
    await page.goto(`${BASE_URL}/series`, { waitUntil: "networkidle" });

    // Auto-redireção para a única biblioteca de séries do mock
    await page.waitForURL(/\/libraries\/\d+\/series/, { timeout: 15000 });

    const cards = page.locator(".card");
    await expect(cards).toHaveCount(2, { timeout: 15000 });

    const first = cards.first();
    await expect(first.locator(".card-title")).toHaveText("Arrow");
    await expect(first.locator(".card-summary")).toContainText("bilionário");
  });

  test("clique na série abre o editor de marcadores estilo Plex", async ({ page }) => {
    await page.goto(`${BASE_URL}/series`, { waitUntil: "networkidle" });
    await page.waitForURL(/\/libraries\/\d+\/series/, { timeout: 15000 });

    await page.locator(".card").first().click();

    // Editor: rota /series/{key}/markers, temporadas e episódios
    await page.waitForURL(/\/series\/\d+\/markers/, { timeout: 15000 });
    await expect(page.locator(".season-title").first()).toContainText("Temporada 1", {
      timeout: 15000,
    });
    await expect(page.locator(".episode-title").first()).toContainText("Pilot");
    // badge de marcador ausente: botões de adicionar (mais) por linha
    await expect(page.locator(".btn-icon.add").first()).toBeVisible();
  });

  test("adiciona e edita marcador inline no editor com salvamento otimista", async ({ page }) => {
    await page.goto(`${BASE_URL}/series`, { waitUntil: "networkidle" });
    await page.waitForURL(/\/libraries\/\d+\/series/, { timeout: 15000 });

    await page.locator(".card").first().click();
    await page.waitForURL(/\/series\/\d+\/markers/, { timeout: 15000 });
    await page.waitForSelector(".season-title", { timeout: 15000 });

    // Abre a edição do marcador de intro do primeiro episódio
    const row = page.locator(".marker-row").first();
    await row.locator(".btn-icon.add").click();

    const startInput = row.locator('label:has-text("Início") input');
    const endInput = row.locator('label:has-text("Fim") input');
    await startInput.fill("500");
    await endInput.fill("85000");

    await row.locator('button:has-text("Salvar")').click();

    // Estado otimista: linha em modo leitura com badge manual
    await expect(row.locator(".badge-manual")).toBeVisible({ timeout: 15000 });
    await expect(row.locator(".times")).toContainText("00:00.50");
  });
});
