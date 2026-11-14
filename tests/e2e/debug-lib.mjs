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
await page.waitForTimeout(1000);

// Dump all links
const links = await page.locator('a').all();
for (const link of links) {
  const href = await link.getAttribute('href');
  const text = await link.textContent();
  if (href) console.log(`LINK: ${href} | TEXT: ${text.trim()}`);
}

// Dump all buttons
const buttons = await page.locator('button').all();
for (const btn of buttons) {
  const text = await btn.textContent();
  console.log(`BTN: ${text.trim()}`);
}

await browser.close();
