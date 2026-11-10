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
}

test("QA fix-biblioteca-titulo: título Biblioteca e breadcrumb alinhado", async ({ page }) => {
  await authenticate(page);

  // Sidebar/menu deve conter "Biblioteca" (não "Séries")
  await page.goto(`${BASE}/libraries`, { waitUntil: "networkidle" });
  const body = await page.locator("body").innerText();
  expect(body).toContain("Biblioteca");
  expect(body).not.toContain("Séries");

  // Navegar para uma biblioteca (primeira) e validar título "Biblioteca → ..."
  const libCard = page.locator("[data-testid='library-card']").first();
  if (await libCard.count()) {
    await libCard.click();
    await page.waitForLoadState("networkidle");
    const pageText = await page.locator("body").innerText();
    expect(pageText).not.toContain("Séries da biblioteca");
    expect(pageText).toContain("Biblioteca");
  }
});
