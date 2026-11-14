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
  await page.waitForTimeout(3000);  // Wait for Vue to render
  
  console.log("URL:", page.url());
  
  const counts = await page.evaluate(() => {
    return {
      seriesView: document.querySelectorAll('.series-view').length,
      genreCarousel: document.querySelectorAll('.genre-carousel-view').length,
      skeleton: document.querySelectorAll('.genre-carousel-skeleton').length,
      alert: document.querySelectorAll('.alert').length,
      empty: document.querySelectorAll('.empty').length,
      appInnerHTML: document.querySelector('#app')?.innerHTML?.slice(0, 3000) || ''
    };
  });
  console.log("Counts:", JSON.stringify(counts, null, 2));
  
} catch (e) {
  console.log("ERROR:", e.message);
} finally {
  await browser.close();
}
