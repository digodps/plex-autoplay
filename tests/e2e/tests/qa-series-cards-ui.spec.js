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

test("tema: toggle alterna light/dark e persiste", async ({ page }) => {
  await login(page);

  const toggle = page.locator('button[aria-label="Tema claro"], button[aria-label="Tema escuro"]').first();
  await expect(toggle).toBeVisible({ timeout: 5000 });

  // Start in light mode
  await page.evaluate(() => localStorage.setItem("theme.mode", "light"));
  await page.reload({ waitUntil: "networkidle" });
  await expect(page.locator("html")).toHaveAttribute("data-theme", "light");

  // light -> dark
  await toggle.click();
  await expect(page.locator("html")).toHaveAttribute("data-theme", "dark");
  await expect(page.evaluate(() => localStorage.getItem("theme.mode"))).resolves.toBe("dark");

  // persistence after reload
  await page.reload({ waitUntil: "networkidle" });
  await expect(page.locator("html")).toHaveAttribute("data-theme", "dark");

  // dark -> light
  await toggle.click();
  await expect(page.locator("html")).toHaveAttribute("data-theme", "light");
});

test("breadcrumb: exibe trilha em /libraries", async ({ page }) => {
  await login(page);
  await page.goto(`${BASE}/libraries`, { waitUntil: "networkidle" });

  const breadcrumb = page.locator('nav[aria-label="Breadcrumb"]');
  await expect(breadcrumb).toBeVisible();

  const current = breadcrumb.locator('[aria-current="page"]');
  await expect(current.first()).toBeVisible();
  await expect(current.first()).toHaveText(/Bibliotecas/);
});

test("menu do usuário: abre e fecha", async ({ page }) => {
  await login(page);

  const menuBtn = page.locator('button[aria-label="Menu do usuário"]').first();
  await expect(menuBtn).toBeVisible();
  await expect(menuBtn).toHaveAttribute("aria-expanded", "false");

  await menuBtn.click();
  await expect(menuBtn).toHaveAttribute("aria-expanded", "true");

  // click settings closes menu and navigates
  await page.click('a[href="/settings"]');
  await expect(page).toHaveURL(`${BASE}/settings`);
  await expect(menuBtn).toHaveAttribute("aria-expanded", "false");
});

test("cards de séries: renderizam em /libraries/{key}/series com layout streaming", async ({ page }) => {
  await login(page);

  // Navigate to first library's series page
  await page.goto(`${BASE}/libraries`, { waitUntil: "networkidle" });
  const libLink = page.locator('nav[aria-label="Breadcrumb"] a:has-text("Bibliotecas")').first();
  // Better: click first library card on /libraries
  await page.locator('[data-testid="library-card"]').first().click({ timeout: 5000 });
  await page.waitForURL(/\/libraries\/.*\/series/, { timeout: 10000 });

  // Check series grid exists
  const grid = page.locator('[data-testid="series-card"]').first();
  await expect(grid).toBeVisible({ timeout: 10000 });

  // Check card structure: image with aspect-2/3, title overlay
  const card = page.locator('[data-testid="series-card"]').first();
  await expect(card.locator('[data-testid="series-thumb-inner"]')).toBeVisible();
  const title = card.locator('h4');
  await expect(title).toBeVisible();

  // Hover scales image (motion-safe)
  await card.hover();
  // Can't easily test transform, just ensure no error

  // Click navigates to markers
  await card.click();
  await page.waitForURL(/\/series\/.*\/markers/, { timeout: 10000 });
});

test("cards de séries: tema dark/light aplicado corretamente", async ({ page }) => {
  await login(page);

  // Force dark mode
  await page.evaluate(() => localStorage.setItem("theme.mode", "dark"));
  await page.goto(`${BASE}/libraries`, { waitUntil: "networkidle" });
  await page.locator('[data-testid="library-card"]').first().click();
  await page.waitForURL(/\/libraries\/.*\/series/, { timeout: 10000 });

  // Card uses dark theme colors (dark:bg-gray-800 etc.)
  const card = page.locator('[data-testid="series-card"]').first();
  await expect(card).toHaveClass(/dark:bg-gray-800/);

  // Force light mode
  await page.evaluate(() => localStorage.setItem("theme.mode", "light"));
  await page.reload({ waitUntil: "networkidle" });
  await expect(page.locator("html")).toHaveAttribute("data-theme", "light");

  // Card uses light theme colors
  await expect(card).toHaveClass(/bg-white/);
});