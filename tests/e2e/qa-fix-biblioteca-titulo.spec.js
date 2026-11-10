const { test, expect } = require("@playwright/test");

const BASE = "http://localhost:8080";

async function authenticate(page) {
  const res = await fetch(`${BASE}/auth/login`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ password: "PlexAdmin2026" }),
  });
  if (!res.ok) throw new Error(`login falhou HTTP ${res.status}`);
  const { token } = await res.json();
  await page.context().addCookies([
    { name: "auth_token", value: token, domain: "localhost", path: "/", httpOnly: false, secure: false, sameSite: "Lax" },
  ]);
  await page.goto(`${BASE}/dashboard`, { waitUntil: "networkidle" });
}

test("QA fix-biblioteca-titulo: tema e menu funcionando", async ({ page }) => {
  await authenticate(page);

  // 1. Tema: toggle cicla
  const html = page.locator("html");
  const themeBtn = page.locator("header [aria-label*='Tema']");
  await expect(themeBtn).toBeVisible();

  const temaInicial = await page.evaluate(() => document.documentElement.dataset.theme);
  await themeBtn.click();
  await page.waitForTimeout(300);
  const temaDepois = await page.evaluate(() => document.documentElement.dataset.theme);
  expect(temaDepois).not.toBe(temaInicial);

  // 2. Menu do usuário: abre e fecha
  const menuBtn = page.locator("header [aria-label='Menu do usuário']");
  await expect(menuBtn).toBeVisible();
  await menuBtn.click();

  const settingsLink = page.locator("header a[href='/settings']");
  await expect(settingsLink).toBeVisible();
  await settingsLink.click();
  await expect(page).toHaveURL(`${BASE}/settings`);
  await expect(page.locator("header")).toContainText("Ajustes");
});
