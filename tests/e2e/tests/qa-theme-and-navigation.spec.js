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

test("tema: toggle cicla system -> light -> dark -> system e persiste", async ({ page }) => {
  await login(page);

  const themeMode = () => page.evaluate(() => localStorage.getItem("theme.mode"));

  await page.evaluate(() => localStorage.setItem("theme.mode", "system"));
  await page.reload({ waitUntil: "networkidle" });
  await expect(themeMode()).resolves.toBe("system");
  await expect(page.locator("html")).toHaveAttribute("data-theme", "light");

  const toggle = page.locator('button[aria-label="Tema claro"], button[aria-label="Tema escuro"]').first();
  await expect(toggle).toBeVisible({ timeout: 5000 });
  await toggle.click();
  await expect(page.locator("html")).toHaveAttribute("data-theme", "light");
  await expect(themeMode()).resolves.toBe("light");

  await toggle.click();
  await expect(page.locator("html")).toHaveAttribute("data-theme", "dark");
  await expect(themeMode()).resolves.toBe("dark");

  await page.reload({ waitUntil: "networkidle" });
  await expect(page.locator("html")).toHaveAttribute("data-theme", "dark");

  await toggle.click();
  await expect(page.locator("html")).toHaveAttribute("data-theme", "light");
  await expect(themeMode()).resolves.toBe("system");

  await toggle.click();
  await expect(page.locator("html")).toHaveAttribute("data-theme", "light");
  await expect(themeMode()).resolves.toBe("light");
});

test("breadcrumb: exibe trilha e marca item ativo", async ({ page }) => {
  await login(page);

  await page.goto(`${BASE}/libraries`, { waitUntil: "networkidle" });

  const breadcrumb = page.locator("nav[aria-label=Breadcrumb]");
  await expect(breadcrumb).toBeVisible();

  // item atual (últimop da trilha) marcado como ativo
  const current = breadcrumb.locator('[aria-current="page"]');
  await expect(current.first()).toBeVisible();
  await expect(current.first()).toHaveText(/Bibliotecas/);

  // navegação via breadcrumb: em /series/:key existe link "Bibliotecas" clicável
  const seriesLink = page.locator('nav[aria-label=Breadcrumb] a:has-text("Bibliotecas")').first();
  if ((await seriesLink.count()) > 0) {
    await seriesLink.click();
    await expect(page).toHaveURL(`${BASE}/libraries`);
  }
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
