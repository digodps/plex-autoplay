const { test, expect } = require("@playwright/test");
const { loadEnv } = require("../utils/env");

loadEnv();

const BASE_URL = process.env.PLAYWRIGHT_BASE_URL || "http://localhost:8080";
const ADMIN_PASSWORD = process.env.OPERATOR_PASSWORD || process.env.ADMIN_PASSWORD || "admin123";

async function login(page) {
  await page.goto(`${BASE_URL}/login`, { waitUntil: "networkidle" });
  await page.waitForSelector('[data-testid="password-input"]', { state: "visible", timeout: 10000 });
  await page.fill('[data-testid="password-input"]', ADMIN_PASSWORD);
  await page.click('[data-testid="login-submit"]');
  await page.waitForURL(`${BASE_URL}/dashboard`);
}

test.describe("Library Card Mosaic Hover", () => {
  test.beforeEach(async ({ page }) => {
    await login(page);
    await page.goto(`${BASE_URL}/libraries`, { waitUntil: "networkidle" });
    await page.waitForTimeout(2500);
  });

  test("library cards display mosaic of series posters", async ({ page }) => {
    const libraryCards = page.locator('[data-testid="library-card"]');
    await expect(libraryCards.first()).toBeVisible({ timeout: 10000 });
    expect(await libraryCards.count()).toBeGreaterThan(0);

    // At least one card should render a mosaic with posters (mock Plex has 2 series)
    const mosaics = page.locator(".library-mosaic .mosaic-grid");
    await expect(mosaics.first()).toBeVisible({ timeout: 10000 });
    const posters = page.locator(".library-mosaic .mosaic-poster");
    expect(await posters.count()).toBeGreaterThan(0);
  });

  test("mosaic has a fallback path (icon) available for empty libraries", async ({ page }) => {
    const cardCount = await page.locator('[data-testid="library-card"]').count();
    const mosaicCount = await page.locator(".library-mosaic .mosaic-grid").count();
    const fallbackCount = await page.locator(".library-mosaic .mosaic-fallback").count();
    // Every card renders exactly one of the two states
    expect(mosaicCount + fallbackCount).toBe(cardCount);
  });

  test("hover on library card triggers mosaic shuffle animation", async ({ page }) => {
    const mosaic = page.locator(".library-mosaic").first();
    await expect(mosaic).toBeVisible({ timeout: 10000 });

    // Capture poster order before hover
    const before = await page.locator(".mosaic-poster").evaluateAll((els) =>
      els.map((e) => e.style.backgroundImage),
    );

    await mosaic.hover();
    await page.waitForTimeout(1200);

    const after = await page.locator(".mosaic-poster").evaluateAll((els) =>
      els.map((e) => e.style.backgroundImage),
    );

    // Shuffle should reorder posters while hovering
    expect(after.length).toBe(before.length);
    expect(JSON.stringify(after)).not.toBe(JSON.stringify(before));

    await page.mouse.move(0, 0);
    await page.waitForTimeout(600);
  });

  test("theme toggle works on libraries page", async ({ page }) => {
    const toggle = page.locator('button[aria-label="Tema claro"], button[aria-label="Tema escuro"]').first();
    await expect(toggle).toBeVisible({ timeout: 5000 });
    const initial = await page.locator("html").getAttribute("data-theme");
    await toggle.click();
    await page.waitForTimeout(300);
    const changed = await page.locator("html").getAttribute("data-theme");
    expect(changed).not.toBe(initial);
    await toggle.click();
    await page.waitForTimeout(300);
    expect(await page.locator("html").getAttribute("data-theme")).toBe(initial);
  });

  test("navigation menu works on libraries page", async ({ page }) => {
    const menuBtn = page.locator('button[aria-label="Menu do usuário"]').first();
    await expect(menuBtn).toBeVisible();
    await expect(menuBtn).toHaveAttribute("aria-expanded", "false");
    await menuBtn.click();
    await expect(menuBtn).toHaveAttribute("aria-expanded", "true");
    await page.click('a[href="/settings"]');
    await expect(page).toHaveURL(`${BASE_URL}/settings`);
    await expect(menuBtn).toHaveAttribute("aria-expanded", "false");
  });
});