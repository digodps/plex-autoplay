import { chromium } from "@playwright/test";

const BASE = "http://localhost:8080";
const PASSWORD = "admin123";

const browser = await chromium.launch({ headless: true });
const page = await browser.newPage({ ignoreHTTPSErrors: true });
page.on("console", msg => console.log("CONSOLE:", msg.type(), msg.text()));
page.on("pageerror", err => console.log("PAGE ERROR:", err.message));
page.setDefaultTimeout(30000);

try {
  await page.goto(`${BASE}/login`, { waitUntil: "domcontentloaded" });
  await page.waitForTimeout(1000);
  console.log("URL:", page.url());
  await page.fill('input[data-testid="password-input"]', PASSWORD);
  await page.click('button[data-testid="login-submit"]');
  await page.waitForURL(`${BASE}/dashboard`, { timeout: 10000 });
  await page.waitForTimeout(1000);
  console.log("Dashboard URL:", page.url());
  
  await page.goto(`${BASE}/libraries`, { waitUntil: "domcontentloaded" });
  await page.waitForTimeout(1000);
  console.log("Libraries URL:", page.url());
  
  await page.locator('button:has-text("Séries")').first().click();
  await page.waitForURL(/\/series/, { timeout: 15000 });
  await page.waitForTimeout(2000);
  console.log("Series URL:", page.url());
  
  const html = await page.content();
  console.log("HTML len:", html.length);
} catch (e) {
  console.log("ERROR:", e.message);
} finally {
  await browser.close();
}
