import { chromium } from "@playwright/test";

const BASE = "http://localhost:8080";
const PASSWORD = "admin123";

const browser = await chromium.launch({ headless: true });
const page = await browser.newPage({ ignoreHTTPSErrors: true });
page.on("pageerror", err => console.log("PAGE ERROR:", err.message, err.stack?.slice(0,500)));
page.setDefaultTimeout(30000);

try {
  await page.goto(`${BASE}/login`, { waitUntil: "domcontentloaded" });
  await page.fill('input[data-testid="password-input"]', PASSWORD);
  await page.click('button[data-testid="login-submit"]');
  await page.waitForURL(`${BASE}/dashboard`, { timeout: 10000, waitUntil: "domcontentloaded" });
  
  await page.goto(`${BASE}/libraries/1/series`, { waitUntil: "domcontentloaded" });
  await page.waitForResponse(res => res.url().includes('/api/series/1'), { timeout: 10000 });
  await page.waitForTimeout(3000);
  
  console.log("URL:", page.url());
  
  // Use locators instead of evaluate
  const seriesView = await page.locator('.series-view').count();
  console.log(".series-view:", seriesView);
  
  const genreCarousel = await page.locator('.genre-carousel-view').count();
  console.log(".genre-carousel-view:", genreCarousel);
  
  const skeleton = await page.locator('.genre-carousel-skeleton').count();
  console.log(".genre-carousel-skeleton:", skeleton);
  
  const alert = await page.locator('.alert').count();
  console.log(".alert:", alert);
  
  const empty = await page.locator('.empty').count();
  console.log(".empty:", empty);
  
  const appHtml = await page.locator('#app').innerHTML({ timeout: 5000 });
  console.log("#app innerHTML (first 3000):", appHtml.slice(0, 3000));
  
} catch (e) {
  console.log("ERROR:", e.message);
} finally {
  await browser.close();
}
