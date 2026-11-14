import { chromium } from "@playwright/test";

const BASE = "http://localhost:8080";
const PASSWORD = "admin123";

const browser = await chromium.launch({ headless: true });
const page = await browser.newPage({ ignoreHTTPSErrors: true });
page.on("response", res => {
  if (res.url().includes("/api/")) console.log("RESP:", res.status(), res.url());
});
page.on("request", req => {
  if (req.url().includes("/api/")) console.log("REQ:", req.method(), req.url());
});
page.on("pageerror", err => console.log("PAGE ERROR:", err.message));
page.setDefaultTimeout(30000);

try {
  await page.goto(`${BASE}/login`, { waitUntil: "domcontentloaded" });
  await page.fill('input[data-testid="password-input"]', PASSWORD);
  await page.click('button[data-testid="login-submit"]');
  await page.waitForURL(`${BASE}/dashboard`, { timeout: 10000 });
  await page.waitForTimeout(1000);
  
  await page.goto(`${BASE}/libraries`, { waitUntil: "domcontentloaded" });
  await page.waitForTimeout(1000);
  
  await page.locator('button:has-text("Séries")').first().click();
  await page.waitForURL(/\/series/, { timeout: 15000 });
  
  // Wait for network idle after series page load
  await page.waitForLoadState('networkidle', { timeout: 10000 });
  console.log("Network idle reached");
  
  const text = await page.textContent('body');
  console.log("Body text:", text.slice(0, 3000));
  
} catch (e) {
  console.log("ERROR:", e.message);
} finally {
  await browser.close();
}
