import { chromium } from "@playwright/test";

const BASE = "http://localhost:8080";
const PASSWORD = "admin";

const browser = await chromium.launch({ headless: true });
const page = await browser.newPage({ ignoreHTTPSErrors: true });
page.setDefaultTimeout(60000);

await page.goto(`${BASE}/login`, { waitUntil: "domcontentloaded" });
await page.waitForTimeout(2000);
console.log("URL:", page.url());

// Try to find any input
const inputs = await page.locator('input').all();
console.log(`Found ${inputs.length} inputs`);
for (const inp of inputs) {
  const type = await inp.getAttribute('type');
  const testid = await inp.getAttribute('data-testid');
  console.log(`  input: type=${type} testid=${testid}`);
}

// Try password input
await page.waitForSelector('input[type="password"]', { timeout: 10000 });
await page.fill('input[type="password"]', PASSWORD);
await page.click('button[type="submit"]');
await page.waitForTimeout(3000);
console.log("URL after:", page.url());

await browser.close();
