import { test, expect } from "@playwright/test";

const BASE = process.env.PLAYWRIGHT_BASE_URL || "http://localhost:8080";
const PASSWORD = process.env.OPERATOR_PASSWORD || process.env.ADMIN_PASSWORD || "admin123";

async function login(page) {
  await page.goto(`${BASE}/login`, { waitUntil: "networkidle" });
  await page.waitForSelector("[data-testid=login-form]", { state: "visible", timeout: 15000 });
  await page.fill("[data-testid=password-input]", PASSWORD);
  await page.click("[data-testid=login-submit]");
  await page.waitForURL(`${BASE}/dashboard`, { timeout: 15000 });
}

test("breadcrumb da página de séries mostra nome dinâmico da biblioteca", async ({ page }) => {
  await login(page);

  await page.goto(`${BASE}/libraries/1/series`, { waitUntil: "networkidle" });
  await page.waitForSelector("nav[aria-label=Breadcrumb]", { state: "visible", timeout: 15000 });

  const breadcrumb = page.locator("nav[aria-label=Breadcrumb]");
  await expect(breadcrumb).toContainText("Animes", { timeout: 10000 });

  const text = await breadcrumb.textContent();
  expect(text).toContain("Bibliotecas");
  expect(text).not.toContain("Biblioteca indisponível");
});
