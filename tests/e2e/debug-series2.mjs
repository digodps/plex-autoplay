import { chromium } from "@playwright/test";

const BASE = "http://localhost:8080";
const PASSWORD = "admin123";

const browser = await chromium.launch({ headless: true });
const page = await browser.newPage({ ignoreHTTPSErrors: true });
page.setDefaultTimeout(30000);

await page.goto(`${BASE}/login`, { waitUntil: "networkidle" });
await page.fill('input[data-testid="password-input"]', PASSWORD);
await page.click('button[data-testid="login-submit"]');
await page.waitForURL(`${BASE}/dashboard`, { timeout: 10000 });

await page.goto(`${BASE}/libraries`, { waitUntil: "networkidle" });
await page.waitForTimeout(500);
await page.locator('button:has-text("Séries")').first().click();
await page.waitForURL(/\/series/, { timeout: 10000 });
await page.waitForTimeout(3000);

// Check all Vue components data
const vueData = await page.evaluate(() => {
  const app = document.querySelector('#app');
  if (!app) return null;
  const instance = app.__vue_app__;
  if (!instance) return null;
  // Find SeriesView instance
  function findComponent(node, name) {
    if (node.$options && node.$options.name === name) return node;
    if (node.$children) {
      for (const child of node.$children) {
        const found = findComponent(child, name);
        if (found) return found;
      }
    }
    return null;
  }
  // Not easy from outside. Let's just check for genre-carousel-view in DOM
  return {
    genreCarouselView: document.querySelector('.genre-carousel-view') !== null,
    seriesView: document.querySelector('.series-view') !== null,
    mainContent: document.querySelector('.page')?.innerHTML?.slice(0, 2000)
  };
});

console.log("Vue data:", JSON.stringify(vueData, null, 2));

await browser.close();
