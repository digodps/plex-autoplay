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
  await page.fill('input[data-testid="password-input"]', PASSWORD);
  await page.click('button[data-testid="login-submit"]');
  await page.waitForURL(`${BASE}/dashboard`, { timeout: 10000, waitUntil: "domcontentloaded" });
  
  await page.goto(`${BASE}/libraries`, { waitUntil: "domcontentloaded" });
  await page.waitForTimeout(1000);
  
  await page.locator('button:has-text("Séries")').first().click();
  await page.waitForURL(/\/series/, { timeout: 15000, waitUntil: "domcontentloaded" });
  await page.waitForTimeout(5000);
  
  console.log("URL:", page.url());
  console.log("Closed:", page.isClosed());
  const html = await page.content();
  console.log("genre-carousel-view:", (html.match(/genre-carousel-view/g) || []).length);
  console.log("carousel-card:", (html.match(/carousel-card/g) || []).length);
  console.log("genre-title:", (html.match(/genre-title/g) || []).length);
  console.log("HTML (body):", html.slice(html.indexOf('<body'), html.indexOf('<body') + 3000));
} catch (e) {
  console.log("ERROR:", e.message);
} finally {
  await browser.close();
}
