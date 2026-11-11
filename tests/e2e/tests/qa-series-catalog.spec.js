const { test, expect } = require("@playwright/test");

const BASE = process.env.PLAYWRIGHT_BASE_URL || "http://localhost:8080";
const PASSWORD = process.env.OPERATOR_PASSWORD || process.env.ADMIN_PASSWORD || "admin123";

async function login(page) {
  await page.goto(`${BASE}/login`, { waitUntil: "networkidle" });
  await page.waitForSelector("[data-testid=login-form]", { state: "visible", timeout: 15000 });
  await page.fill("[data-testid=password-input]", PASSWORD);
  await page.click("[data-testid=login-submit]");
  await page.waitForURL(`${BASE}/dashboard`, { timeout: 15000 });
}

test("séries: exibe séries da biblioteca após sincronização de catálogo", async ({ page }) => {
  await login(page);

  // Navegar para página de séries da biblioteca 1
  await page.goto(`${BASE}/libraries/1/series`, { waitUntil: "networkidle" });

  // Aguarda o grid de séries carregar (usa data-testid="series-card" no card individual)
  const seriesCard = page.locator('[data-testid="series-card"]').first();
  await expect(seriesCard).toBeVisible({ timeout: 15000 });

  // Verifica títulos conhecidos (pelo menos 3 dos retornados pela API)
  const titles = ["Aoashi", "Avatar: Seven Havens", "Baka and Test", "DARK MACHINE", "FX Fighter", "From Far Away", "Hello, I Am a Witch", "The Iceblade", "Welsh & Shedar"];
  let foundCount = 0;
  for (const title of titles) {
    if (await page.locator(`text=${title}`).first().isVisible({ timeout: 1000 }).catch(() => false)) {
      foundCount++;
    }
  }
  expect(foundCount).toBeGreaterThanOrEqual(3);
});

test("breadcrumb: exibe trilha correta em /libraries/1/series", async ({ page }) => {
  await login(page);
  await page.goto(`${BASE}/libraries/1/series`, { waitUntil: "networkidle" });

  const breadcrumb = page.locator("nav[aria-label=Breadcrumb]");
  await expect(breadcrumb).toBeVisible();
  const current = breadcrumb.locator('[aria-current="page"]');
  await expect(current.first()).toBeVisible();
});