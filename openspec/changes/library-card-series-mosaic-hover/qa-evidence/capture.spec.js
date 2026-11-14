const { test } = require("@playwright/test");
const { loadEnv } = require("../../../../tests/e2e/utils/env");

loadEnv();

const BASE_URL = process.env.PLAYWRIGHT_BASE_URL || "http://localhost:8080";
const ADMIN_PASSWORD = process.env.OPERATOR_PASSWORD || process.env.ADMIN_PASSWORD || "admin123";
const OUT = __dirname;

async function login(page) {
  await page.goto(`${BASE_URL}/login`, { waitUntil: "networkidle" });
  await page.waitForSelector('[data-testid="password-input"]', { state: "visible", timeout: 10000 });
  await page.fill('[data-testid="password-input"]', ADMIN_PASSWORD);
  await page.click('[data-testid="login-submit"]');
  await page.waitForURL(`${BASE_URL}/dashboard`);
}

test("capture libraries mosaic evidence", async ({ page }) => {
  await login(page);
  await page.goto(`${BASE_URL}/libraries`, { waitUntil: "networkidle" });
  await page.waitForTimeout(2500);

  await page.screenshot({ path: `${OUT}/01-libraries-mosaic-light.png`, fullPage: true });

  // Hover to trigger shuffle
  const mosaic = page.locator(".library-mosaic").first();
  await mosaic.hover();
  await page.waitForTimeout(900);
  await page.screenshot({ path: `${OUT}/02-mosaic-hover-shuffle.png`, fullPage: true });

  // Theme toggle to dark
  const toggle = page.locator('button[aria-label="Tema claro"], button[aria-label="Tema escuro"]').first();
  await toggle.click();
  await page.waitForTimeout(400);
  await page.screenshot({ path: `${OUT}/03-libraries-mosaic-dark.png`, fullPage: true });
});