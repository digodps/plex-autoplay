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
  await page.waitForTimeout(8000);
  
  console.log("URL:", page.url());
  
  // Check all elements in body
  const bodyText = await page.locator('body').textContent({ timeout: 10000 });
  console.log("Body text length:", bodyText.length);
  console.log("Body text (first 2000):", bodyText.slice(0, 2000));
  
  // Check if SeriesView component mounted by looking for its root element
  const seriesView = await page.locator('.series-view').count();
  console.log(".series-view count:", seriesView);
  
  const genreCarousel = await page.locator('.genre-carousel-view').count();
  console.log(".genre-carousel-view count:", genreCarousel);
  
  const skeleton = await page.locator('.genre-carousel-skeleton').count();
  console.log(".genre-carousel-skeleton count:", skeleton);
  
  const error = await page.locator('.alert').count();
  console.log(".alert count:", error);
  
} catch (e) {
  console.log("ERROR:", e.message);
} finally {
  await browser.close();
}
