const { test, expect } = require("@playwright/test");

const BASE = process.env.PLAYWRIGHT_BASE_URL || "http://localhost:8080";
const PASSWORD = process.env.OPERATOR_PASSWORD || "admin123";

async function login(page) {
  await page.goto(`${BASE}/login`, { waitUntil: "networkidle" });
  await page.waitForSelector("[data-testid=login-form]", { state: "visible", timeout: 15000 });
  await page.fill("[data-testid=password-input]", PASSWORD);
  await page.click("[data-testid=login-submit]");
  await page.waitForURL(`${BASE}/dashboard`, { timeout: 15000 });
}

async function cycleTheme(page) {
  const toggle = page.locator('button[aria-label^="Tema "]').first();
  await expect(toggle).toBeVisible({ timeout: 10000 });
  return toggle;
}

test("tema: light/dark toggle aplicado na página /sync", async ({ page }) => {
  await login(page);
  await page.goto(`${BASE}/sync`, { waitUntil: "networkidle" });

  const html = page.locator("html");
  const toggle = await cycleTheme(page);

  // inicial light
  await expect(html).toHaveAttribute("data-theme", "light");

  // light -> dark
  await toggle.click();
  await expect(html).toHaveAttribute("data-theme", "dark");

  // persiste ao recarregar
  await page.reload({ waitUntil: "networkidle" });
  await expect(html).toHaveAttribute("data-theme", "dark");

  // dark -> light
  await (await cycleTheme(page)).click();
  await expect(html).toHaveAttribute("data-theme", "light");
});

test("menus: breadcrumb visível, item ativo marcado; logo navega para home", async ({ page }) => {
  await login(page);
  await page.goto(`${BASE}/libraries`, { waitUntil: "networkidle" });

  const breadcrumb = page.locator('nav[aria-label="Breadcrumb"]');
  await expect(breadcrumb).toBeVisible();

  const items = breadcrumb.locator("ol li");
  await expect(items.first()).toBeVisible();
  const last = items.last();
  await expect(last.locator("[aria-current='page']")).toBeVisible();
  await expect(last).toContainText("Bibliotecas");

  // "home" via logo do AppHeader navega para dashboard
  await page.locator('a[href="/"]').first().click();
  await expect(page).toHaveURL(`${BASE}/dashboard`);
});

test("sync: iniciar escaneamento não deixa tela preta (shell + status running)", async ({ page }) => {
  await login(page);
  await page.goto(`${BASE}/sync`, { waitUntil: "networkidle" });

  // shell continua renderizada
  await expect(page.locator("header")).toBeVisible();
  await expect(page.locator("main, [role=main]")).toBeVisible();

  // inicia escaneamento
  const start = page.getByRole("button", { name: /Iniciar Escaneamento/ });
  await expect(start).toBeVisible();
  await start.click();

  // status sai de idle
  await page.waitForFunction(() => {
    const el = document.querySelector(".page");
    return el && el.textContent.includes("running");
  }, { timeout: 10000 });

  // shell continua visível durante o fluxo (não fica preta)
  await expect(page.locator("header")).toBeVisible();
  await expect(page.locator("h1")).toContainText("Sincronização");
});
