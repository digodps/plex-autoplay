const { test, expect } = require("@playwright/test");

const BASE = process.env.PLAYWRIGHT_BASE_URL || "http://localhost:8090";
const PASSWORD = process.env.OPERATOR_PASSWORD || process.env.ADMIN_PASSWORD || "PlexAdmin2026";

async function login(page) {
  await page.goto(`${BASE}/login`, { waitUntil: "networkidle" });
  await page.waitForSelector("[data-testid=login-form]", { state: "visible", timeout: 15000 });
  await page.fill("[data-testid=password-input]", PASSWORD);
  await page.click("[data-testid=login-submit]");
  await page.waitForURL(`${BASE}/dashboard`, { timeout: 15000 });
}

test("tema: toggle cicla system -> light -> dark -> system e persiste", async ({ page }) => {
  await login(page);

  const themeMode = () => page.evaluate(() => localStorage.getItem("theme.mode"));

  // sistema -> claro (primeiro clique)
  await page.evaluate(() => localStorage.setItem("theme.mode", "system"));
  await page.reload({ waitUntil: "networkidle" });
  await expect(themeMode()).resolves.toBe("system");
  await expect(page.locator("html")).toHaveAttribute("data-theme", "light");

  const toggle = page.locator("button.btn-ghost").first();
  await toggle.click();
  await expect(page.locator("html")).toHaveAttribute("data-theme", "light");
  await expect(themeMode()).resolves.toBe("light");

  // claro -> escuro
  await toggle.click();
  await expect(page.locator("html")).toHaveAttribute("data-theme", "dark");
  await expect(themeMode()).resolves.toBe("dark");

  // persistence recarregando
  await page.reload({ waitUntil: "networkidle" });
  await expect(page.locator("html")).toHaveAttribute("data-theme", "dark");

  // escuro -> sistema
  await toggle.click();
  await expect(page.locator("html")).toHaveAttribute("data-theme", "light");
  await expect(themeMode()).resolves.toBe("system");

  // sistema -> claro
  await toggle.click();
  await expect(page.locator("html")).toHaveAttribute("data-theme", "light");
  await expect(themeMode()).resolves.toBe("light");
});

test("navegacao: breadcrumb navega e marca item ativo", async ({ page }) => {
  await login(page);

  await page.goto(`${BASE}/libraries`, { waitUntil: "networkidle" });
  const items = page.locator(".breadcrumb-item");
  await expect(items.first()).toBeVisible();
  await expect(items.first()).toContainText("Início");

  // último item = current
  await expect(items.last()).toHaveAttribute("aria-current", "page");

  // Ancestral clicável navega de volta
  await items.first().click();
  await expect(page).toHaveURL(`${BASE}/dashboard`);

  // menu/topbar presente (brand + breadcrumb)
  await expect(page.locator(".topbar")).toBeVisible();
  await expect(page.locator(".breadcrumb-bar, .breadcrumb-item").first()).toBeVisible();
});
