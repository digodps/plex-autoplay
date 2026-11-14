import { chromium } from "@playwright/test";

const BASE = "http://localhost:8080";
const PASSWORD = "admin123";

const browser = await chromium.launch({ headless: true });
const page = await browser.newPage({ ignoreHTTPSErrors: true });
page.on("console", msg => console.log("CONSOLE:", msg.type(), msg.text().slice(0,300)));
page.on("pageerror", err => console.log("PAGE ERROR:", err.message, err.stack?.slice(0,500)));
page.setDefaultTimeout(30000);

try {
  await page.goto(`${BASE}/login`, { waitUntil: "domcontentloaded" });
  await page.fill('input[data-testid="password-input"]', PASSWORD);
  await page.click('button[data-testid="login-submit"]');
  await page.waitForURL(`${BASE}/dashboard`, { timeout: 10000, waitUntil: "domcontentloaded" });
  console.log("Dashboard loaded");
  console.log("URL:", page.url());
  
  // Check dashboard content
  const text = await page.locator('body').textContent({ timeout: 5000 });
  console.log("Dashboard text:", text.slice(0, 500));
  
} catch (e) {
  console.log("ERROR:", e.message);
} finally {
  await browser.close();
}
