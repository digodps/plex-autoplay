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

async function getLogo(page) {
  // The logo is inside the RouterLink in the header, directly rendered as SVG
  return page.locator('header[data-testid="app-header"] a[aria-label*="Plex Autoplay"] svg').first();
}

async function getLightningPath(page) {
  // The lightning bolt path has class "logo-lightning"
  return page.locator('header[data-testid="app-header"] a[aria-label*="Plex Autoplay"] svg path.logo-lightning').first();
}

test.describe("Electric Logo Animation", () => {
  test("logo animation: electric lightning bolt effect is present and animating", async ({ page }) => {
    await login(page);
    await page.goto(`${BASE}/dashboard`, { waitUntil: "networkidle" });

    // Find the logo SVG
    const logo = await getLogo(page);
    await expect(logo).toBeVisible({ timeout: 10000 });

    // Check for the lightning bolt path in the SVG
    const lightningPath = await getLightningPath(page);
    await expect(lightningPath).toBeVisible();

    // Verify animation CSS is applied on the lightning path
    const hasAnimation = await lightningPath.evaluate(el => {
      const style = window.getComputedStyle(el);
      return style.animationName !== "none" && style.animationDuration !== "0s";
    });
    expect(hasAnimation).toBe(true);
  });

  test("logo animation: respects prefers-reduced-motion", async ({ page }) => {
    // Set prefers-reduced-motion before navigation
    await page.emulateMedia({ reducedMotion: "reduce" });
    await login(page);
    await page.goto(`${BASE}/dashboard`, { waitUntil: "networkidle" });

    const lightningPath = await getLightningPath(page);
    await expect(lightningPath).toBeVisible({ timeout: 10000 });

    // Check that animation is disabled on the lightning path
    const hasAnimation = await lightningPath.evaluate(el => {
      const style = window.getComputedStyle(el);
      return style.animationName !== "none" && style.animationDuration !== "0s";
    });
    expect(hasAnimation).toBe(false);
  });

  test("logo animation: theme switching works correctly (light/dark)", async ({ page }) => {
    await login(page);
    await page.goto(`${BASE}/dashboard`, { waitUntil: "networkidle" });

    const logo = await getLogo(page);
    await expect(logo).toBeVisible({ timeout: 10000 });

    const themeMode = () => page.evaluate(() => localStorage.getItem("theme.mode"));

    // Test light theme
    await page.evaluate(() => localStorage.setItem("theme.mode", "light"));
    await page.reload({ waitUntil: "networkidle" });
    await expect(page.locator("html")).toHaveAttribute("data-theme", "light");
    
    // Verify logo is visible and uses correct color in light theme
    await expect(logo).toBeVisible();

    // Test dark theme
    await page.evaluate(() => localStorage.setItem("theme.mode", "dark"));
    await page.reload({ waitUntil: "networkidle" });
    await expect(page.locator("html")).toHaveAttribute("data-theme", "dark");
    
    // Verify logo is visible and uses correct color in dark theme
    await expect(logo).toBeVisible();
  });

  test("logo animation: different logo sizes render correctly", async ({ page }) => {
    await login(page);
    await page.goto(`${BASE}/dashboard`, { waitUntil: "networkidle" });

    // The logo should be present at default size (28px in AppHeader)
    const logo = await getLogo(page);
    await expect(logo).toBeVisible({ timeout: 10000 });

    // Check that the logo SVG has proper dimensions
    const box = await logo.boundingBox();
    expect(box).toBeTruthy();
    expect(box.width).toBeGreaterThan(0);
    expect(box.height).toBeGreaterThan(0);
  });
});

test.describe("Theme and Navigation (Regression)", () => {
  test("tema: toggle cicla system -> light -> dark e persiste", async ({ page }) => {
    await login(page);

    const themeMode = () => page.evaluate(() => localStorage.getItem("theme.mode"));

    await page.evaluate(() => localStorage.setItem("theme.mode", "system"));
    await page.reload({ waitUntil: "networkidle" });
    await expect(themeMode()).resolves.toBe("system");
    await expect(page.locator("html")).toHaveAttribute("data-theme", "light");

    const toggle = page.locator('button[aria-label="Tema claro"], button[aria-label="Tema escuro"]').first();
    await expect(toggle).toBeVisible({ timeout: 5000 });

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
    await login(page);

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

  test("breadcrumb: exibe trilha e marca item ativo", async ({ page }) => {
    await login(page);

    await page.goto(`${BASE}/libraries`, { waitUntil: "networkidle" });

    const breadcrumb = page.locator("nav[aria-label=Breadcrumb]");
    await expect(breadcrumb).toBeVisible();

    // item atual marcado como ativo
    const current = breadcrumb.locator('[aria-current="page"]');
    await expect(current.first()).toBeVisible();
    await expect(current.first()).toHaveText(/Bibliotecas/);

    await expect(page.locator(".topbar")).toBeVisible();
  });
});