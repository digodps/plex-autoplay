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
await page.waitForURL(/\/series/, { timeout: 15000 });
await page.waitForTimeout(2000);

const html = await page.content();
console.log("=== HTML (first 5000) ===");
console.log(html.slice(0, 5000));

// Check for genre-carousel-view
const hasCarousel = await page.locator('.genre-carousel-view').count();
console.log(`genre-carousel-view: ${hasCarousel}`);

await browser.close();
