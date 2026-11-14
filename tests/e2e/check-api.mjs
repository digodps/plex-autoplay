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
  await page.waitForURL(`${BASE}/dashboard`, { timeout: 10000, waitUntil: "domcontentloaded" });
  console.log("Dashboard loaded");
  
  await page.goto(`${BASE}/libraries/1/series`, { waitUntil: "domcontentloaded" });
  await page.waitForTimeout(5000);
  console.log("Series page loaded, URL:", page.url());
  
} catch (e) {
  console.log("ERROR:", e.message);
} finally {
  await browser.close();
}
