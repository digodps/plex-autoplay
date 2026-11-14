import { chromium } from "@playwright/test";

const BASE = "http://localhost:8080";
const PASSWORD = "admin123";

const browser = await chromium.launch({ headless: true });
const page = await browser.newPage({ ignoreHTTPSErrors: true });
page.on("pageerror", err => console.log("PAGE ERROR:", err.message));
page.on("framenavigated", frame => console.log("FRAME NAVIGATED:", frame.url()));
page.on("close", () => console.log("PAGE CLOSED at", Date.now()));
page.on("crash", () => console.log("PAGE CRASHED"));
page.setDefaultTimeout(30000);

console.log("Start at", Date.now());

try {
  await page.goto(`${BASE}/login`, { waitUntil: "domcontentloaded" });
  await page.fill('input[data-testid="password-input"]', PASSWORD);
  await page.click('button[data-testid="login-submit"]');
  await page.waitForURL(`${BASE}/dashboard`, { timeout: 10000, waitUntil: "domcontentloaded" });
  console.log("Dashboard loaded at", Date.now());
  
  await page.goto(`${BASE}/libraries/1/series`, { waitUntil: "domcontentloaded" });
  console.log("Navigation started at", Date.now());
  
  await page.waitForResponse(res => res.url().includes('/api/series/1'), { timeout: 10000 });
  console.log("API response received at", Date.now());
  
  await page.waitForTimeout(500);
  console.log("After 500ms wait at", Date.now());
  
  console.log("Series page loaded, URL:", page.url());
  console.log("Page closed?", page.isClosed());
  
  await page.waitForTimeout(2000);
  console.log("After 2000ms wait at", Date.now(), "closed?", page.isClosed());
  
} catch (e) {
  console.log("ERROR:", e.message);
} finally {
  console.log("Finally block at", Date.now());
  await browser.close();
}
