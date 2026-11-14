import { chromium } from "@playwright/test";

const BASE = "http://localhost:8080";
const PASSWORD = "admin123";

const browser = await chromium.launch({ headless: true });
const page = await browser.newPage({ ignoreHTTPSErrors: true });
page.on("pageerror", err => console.log("PAGE ERROR:", err.message, err.stack?.slice(0,500)));
page.on("console", msg => console.log("CONSOLE:", msg.type(), msg.text().slice(0,300)));
page.setDefaultTimeout(30000);

try {
  await page.goto(`${BASE}/login`, { waitUntil: "domcontentloaded" });
  await page.fill('input[data-testid="password-input"]', PASSWORD);
  await page.click('button[data-testid="login-submit"]');
  await page.waitForURL(`${BASE}/dashboard`, { timeout: 10000, waitUntil: "domcontentloaded" });
  
  await page.goto(`${BASE}/libraries/1/series`, { waitUntil: "domcontentloaded" });
  await page.waitForResponse(res => res.url().includes('/api/series/1'), { timeout: 10000 });
  await page.waitForTimeout(5000);  // Wait longer for any JS to complete
  
  console.log("URL:", page.url());
  console.log("Trying simple locator...");
  
  // Try a very simple locator first
  const bodyExists = await page.locator('body').count();
  console.log("body count:", bodyExists);
  
  console.log("Trying #app...");
  const appExists = await page.locator('#app').count();
  console.log("#app count:", appExists);
  
} catch (e) {
  console.log("ERROR:", e.message);
} finally {
  await browser.close();
}
