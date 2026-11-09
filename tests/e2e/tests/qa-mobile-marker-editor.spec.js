const { test, expect } = require("@playwright/test");

const BASE_URL = process.env.PLAYWRIGHT_BASE_URL || "http://localhost:8090";
const ADMIN_PASSWORD = process.env.ADMIN_PASSWORD || "PlexAdmin2026";

test.describe("tela de maker mobile — QA", () => {
  test("tema light/dark toggle aplica corretamente", async ({ page }) => {
    await page.goto(`${BASE_URL}/login`);
    await login(page);

    const before = await page.evaluate(() => ({
      classes: document.documentElement.classList.contains("dark"),
      aria: document.documentElement.dataset.theme,
    }));

    await page.click("button[aria-label='Tema escuro']");

    await expect(page).toHaveURL(/\/dashboard/);

    const after = await page.evaluate(() => ({
      classes: document.documentElement.classList.contains("dark"),
      aria: document.documentElement.dataset.theme,
      stored: localStorage.getItem("theme.mode"),
    }));

    expect(after.stored).toBe("dark");
    expect(after.classes).toBe(!before.classes);
  });

  test("menus abrem, navegam, fecham e mostram estado ativo", async ({ page }) => {
    await login(page);
    await expect(page).toHaveURL(/\/dashboard/);

    // Abre menu do usuário
    await page.click('button[aria-label="Menu do usuário"]');
    const opened = await page.evaluate(() =>
      document.querySelector('button[aria-label="Menu do usuário"]').getAttribute("aria-expanded"),
    );
    expect(opened).toBe("true");

    // Navega para /settings via link no dropdown (fecha o menu automaticamente)
    await page.click('a[href="/settings"]', { timeout: 5000 });
    await expect(page).toHaveURL(/\/settings/);

    // Menu fecha após navegação (showProfile reseta para false)
    const closed = await page.evaluate(() =>
      document.querySelector('button[aria-label="Menu do usuário"]').getAttribute("aria-expanded"),
    );
    expect(closed).toBe("false");

    // Breadcrumb reflete estado ativo
    const active = await page.locator('nav[aria-label="Breadcrumb"] ol li').first().textContent();
    expect(active).toContain("Ajustes");
  });
});

async function login(page) {
  await page.goto(`${BASE_URL}/login`);
  await page.waitForSelector('input[type="password"]', { state: "visible", timeout: 10000 });
  await page.fill('input[type="password"]', ADMIN_PASSWORD);
  await page.click("button");
  await page.waitForURL(/\/dashboard/, { timeout: 10000 });
}