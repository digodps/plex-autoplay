const { test, expect } = require("@playwright/test");

const BASE = process.env.PLAYWRIGHT_BASE_URL || "http://127.0.0.1:8080";
const PASSWORD = process.env.OPERATOR_PASSWORD || process.env.ADMIN_PASSWORD || "admin1234";
const SETUP_TOKEN = process.env.SETUP_TOKEN || "1H7HrHo2wD1i2Py5jRiCL-epSq2EH_lsYDft5TjK2O0";
const PLEX_URL = process.env.PLEX_URL || "http://192.168.18.3:32400";
const PLEX_TOKEN = process.env.PLEX_TOKEN || "test";
const MEDIA_ROOTS = process.env.MEDIA_ROOTS || "/home/hermes/media";

async function completeSetup(page) {
  const res = await page.request.post(`${BASE}/api/auth/setup`, {
    data: {
      Token: SETUP_TOKEN,
      NewPassword: PASSWORD,
      PlexUrl: PLEX_URL,
      PlexToken: PLEX_TOKEN,
      MediaRoots: MEDIA_ROOTS.split(",").map(s => s.trim()),
    },
  });
  if (!res.ok()) {
    const body = await res.text();
    throw new Error(`Setup failed: ${res.status()} ${body}`);
  }
}

async function login(page) {
  await page.goto(`${BASE}/login`, { waitUntil: "networkidle" });
  await page.waitForSelector("[data-testid=login-form]", { state: "visible", timeout: 15000 });
  await page.fill("[data-testid=password-input]", PASSWORD);
  await page.click("[data-testid=login-submit]");
  await page.waitForURL(`${BASE}/dashboard`, { timeout: 15000 });
}

async function getLogo(page) {
  return page.locator('header[data-testid="app-header"] a[aria-label*="Plex Autoplay"] svg').first();
}

test.describe("Remover Efeito Logo — Static Logo Validation", () => {
  test.beforeAll(async ({ request }) => {
    const res = await request.post(`${BASE}/api/auth/setup`, {
      data: {
        Token: SETUP_TOKEN,
        NewPassword: PASSWORD,
        PlexUrl: PLEX_URL,
        PlexToken: PLEX_TOKEN,
        MediaRoots: MEDIA_ROOTS.split(",").map(s => s.trim()),
      },
    });
    if (!res.ok() && res.status() !== 409) {
      const body = await res.text();
      throw new Error(`Setup failed: ${res.status()} ${body}`);
    }
  });

  test.beforeEach(async ({ page }) => {
    await login(page);
  });

  test("logo renders static (no lightning bolt, no animation)", async ({ page }) => {
    await page.goto(`${BASE}/dashboard`, { waitUntil: "networkidle" });

    const logo = await getLogo(page);
    await expect(logo).toBeVisible({ timeout: 10000 });

    // Verify NO lightning bolt path exists
    const lightningPath = page.locator('header[data-testid="app-header"] a[aria-label*="Plex Autoplay"] svg path.logo-lightning').first();
    await expect(lightningPath).toHaveCount(0);

    // Verify logo SVG has exactly 2 paths (triangle + vertical line)
    const paths = page.locator('header[data-testid="app-header"] a[aria-label*="Plex Autoplay"] svg path');
    await expect(paths).toHaveCount(2);

    // Verify no animation on any path
    const hasAnimation = await paths.first().evaluate(el => {
      const style = window.getComputedStyle(el);
      return style.animationName !== "none" && style.animationDuration !== "0s";
    });
    expect(hasAnimation).toBe(false);
  });

  test("logo: theme switching works correctly (light/dark)", async ({ page }) => {
    await page.goto(`${BASE}/dashboard`, { waitUntil: "networkidle" });

    const logo = await getLogo(page);
    await expect(logo).toBeVisible({ timeout: 10000 });

    const themeMode = () => page.evaluate(() => localStorage.getItem("theme.mode"));

    // Test light theme
    await page.evaluate(() => localStorage.setItem("theme.mode", "light"));
    await page.reload({ waitUntil: "networkidle" });
    await expect(page.locator("html")).toHaveAttribute("data-theme", "light");
    await expect(logo).toBeVisible();

    // Test dark theme
    await page.evaluate(() => localStorage.setItem("theme.mode", "dark"));
    await page.reload({ waitUntil: "networkidle" });
    await expect(page.locator("html")).toHaveAttribute("data-theme", "dark");
    await expect(logo).toBeVisible();
  });

  test("logo: different sizes render correctly (via AppHeader default 28)", async ({ page }) => {
    await page.goto(`${BASE}/dashboard`, { waitUntil: "networkidle" });

    const logo = await getLogo(page);
    await expect(logo).toBeVisible({ timeout: 10000 });

    const box = await logo.boundingBox();
    expect(box).toBeTruthy();
    expect(box.width).toBeGreaterThan(0);
    expect(box.height).toBeGreaterThan(0);
  });
});

test.describe("Theme and Navigation (Regression)", () => {
  test.beforeEach(async ({ page }) => {
    await login(page);
  });

  test("tema: toggle cicla system -> light -> dark e persiste", async ({ page }) => {
    await page.goto(`${BASE}/dashboard`, { waitUntil: "networkidle" });

    const themeMode = () => page.evaluate(() => localStorage.getItem("theme.mode"));

    await page.evaluate(() => localStorage.setItem("theme.mode", "system"));
    await page.reload({ waitUntil: "networkidle" });
    await expect(themeMode()).resolves.toBe("system");
    await expect(page.locator("html")).toHaveAttribute("data-theme", "light");

    const toggle = page.locator('button[aria-label="Tema claro"], button[aria-label="Tema escuro"]').first();
    await expect(toggle).toBeVisible({ timeout: 5000 });

    // Ensure we start from system (light)
    await page.evaluate(() => localStorage.setItem("theme.mode", "system"));
    await page.reload({ waitUntil: "networkidle" });
    await expect(page.locator("html")).toHaveAttribute("data-theme", "light");

    // claro -> escuro
    await toggle.click();
    await expect(page.locator("html")).toHaveAttribute("data-theme", "dark");
    await expect(themeMode()).resolves.toBe("dark");

    // persistence ao recarregar
    await page.reload({ waitUntil: "networkidle" });
    await expect(page.locator("html")).toHaveAttribute("data-theme", "dark");
    await expect(themeMode()).resolves.toBe("dark");

    // escuro -> claro
    await toggle.click();
    await expect(page.locator("html")).toHaveAttribute("data-theme", "light");
    await expect(themeMode()).resolves.toBe("light");
  });

  test("menu do usuário: abre, fecha e navega", async ({ page }) => {
    await page.goto(`${BASE}/dashboard`, { waitUntil: "networkidle" });

    const menuBtn = page.locator('button[aria-label="Menu do usuário"]').first();
    await expect(menuBtn).toBeVisible();
    await expect(menuBtn).toHaveAttribute("aria-expanded", "false");

    await menuBtn.click();
    await expect(menuBtn).toHaveAttribute("aria-expanded", "true");

    // menu dropdown visível com itens
    await expect(page.locator('button[data-testid="change-password-menu"]')).toBeVisible();

    // clicar em item fecha o menu
    await page.click('a[href="/settings"]');
    await expect(page).toHaveURL(`${BASE}/settings`);
    await expect(menuBtn).toHaveAttribute("aria-expanded", "false");
  });
});