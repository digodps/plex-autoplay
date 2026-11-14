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
  
  await page.goto(`${BASE}/libraries/1/series`, { waitUntil: "domcontentloaded" });
  await page.waitForTimeout(5000);
  
  console.log("URL:", page.url());
  console.log("Closed:", page.isClosed());
  
  const bodyHtml = await page.content();
  console.log("HTML len:", bodyHtml.length);
  
  // Check for series-view
  const seriesView = await page.locator('.series-view').count();
  console.log(".series-view:", seriesView);
  
  // Check for genre-carousel-view
  const genreCarousel = await page.locator('.genre-carousel-view').count();
  console.log(".genre-carousel-view:", genreCarousel);
  
  // Check for skeleton
  const skeleton = await page.locator('.genre-carousel-skeleton').count();
  console.log(".genre-carousel-skeleton:", skeleton);
  
  // Check for alert
  const alert = await page.locator('.alert').count();
  console.log(".alert:", alert);
  
  // Check for empty
  const empty = await page.locator('.empty').count();
  console.log(".empty:", empty);
  
  // Check the main page div
  const mainHtml = await page.locator('main.main').innerHTML();
  console.log("main.main innerHTML (first 3000):", mainHtml.slice(0, 3000));
  
} catch (e) {
  console.log("ERROR:", e.message);
} finally {
  await browser.close();
}
