import { chromium } from "@playwright/test";

const BASE = "http://localhost:8080";
const PASSWORD = "admin123";

const browser = await chromium.launch({ headless: true });
const page = await browser.newPage({ ignoreHTTPSErrors: true });
page.on("response", res => {
  if (res.url().includes("/api/status") || res.url().includes("/setup/status") || res.url().includes("/series")) {
    console.log("RESP:", res.status(), res.url());
  }
});
page.on("request", req => {
  if (req.url().includes("/api/status") || req.url().includes("/setup/status") || req.url().includes("/series")) {
    console.log("REQ:", req.method(), req.url());
  }
});
page.on("pageerror", err => console.log("PAGE ERROR:", err.message));
page.setDefaultTimeout(30000);

try {
  await page.goto(`${BASE}/login`, { waitUntil: "domcontentloaded" });
  await page.fill('input[data-testid="password-input"]', PASSWORD);
  await page.click('button[data-testid="login-submit"]');
  await page.waitForURL(`${BASE}/dashboard`, { timeout: 10000, waitUntil: "domcontentloaded" });
  
  await page.goto(`${BASE}/libraries`, { waitUntil: "domcontentloaded" });
  await page.waitForTimeout(1000);
  
  console.log("---CLICKING SERIES---");
  await page.locator('button:has-text("Séries")').first().click();
  
  // Wait for navigation and network
  await page.waitForLoadState('domcontentloaded');
  await page.waitForTimeout(3000);
  
  console.log("URL:", page.url());
  console.log("Closed:", page.isClosed());
  
  const html = await page.content();
  console.log("HTML len:", html.length);
  console.log("HTML:", html.slice(0, 3000));
} catch (e) {
  console.log("ERROR:", e.message);
} finally {
  await browser.close();
}
