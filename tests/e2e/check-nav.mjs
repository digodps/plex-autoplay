import { chromium } from "@playwright/test";

const BASE = "http://localhost:8080";
const PASSWORD = "admin123";

const browser = await chromium.launch({ headless: true });
const page = await browser.newPage({ ignoreHTTPSErrors: true });
page.on("pageerror", err => console.log("PAGE ERROR:", err.message, err.stack?.slice(0,500)));
page.on("framenavigated", frame => console.log("FRAME NAVIGATED:", frame.url()));
page.on("close", () => console.log("PAGE CLOSED"));
page.on("crash", () => console.log("PAGE CRASHED"));
page.setDefaultTimeout(30000);

try {
  await page.goto(`${BASE}/login`, { waitUntil: "domcontentloaded" });
  await page.fill('input[data-testid="password-input"]', PASSWORD);
  await page.click('button[data-testid="login-submit"]');
  await page.waitForURL(`${BASE}/dashboard`, { timeout: 10000, waitUntil: "domcontentloaded" });
  console.log("Dashboard loaded");
  
  await page.goto(`${BASE}/libraries/1/series`, { waitUntil: "domcontentloaded" });
  await page.waitForResponse(res => res.url().includes('/api/series/1'), { timeout: 10000 });
  await page.waitForTimeout(2000);
  
  console.log("Series page loaded, URL:", page.url());
  console.log("Page closed?", page.isClosed());
  
} catch (e) {
  console.log("ERROR:", e.message);
} finally {
  await browser.close();
}
