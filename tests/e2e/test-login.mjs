import { chromium } from "@playwright/test";

const BASE = "http://localhost:8080";

async function tryLogin(page, pwd) {
  await page.goto(`${BASE}/login`, { waitUntil: "networkidle" });
  await page.waitForSelector('input[data-testid="password-input"]');
  await page.fill('input[data-testid="password-input"]', pwd);
  await page.click('button[data-testid="login-submit"]');
  await page.waitForTimeout(3000);
  return page.url();
}

const browser = await chromium.launch({ headless: true });
const page = await browser.newPage({ ignoreHTTPSErrors: true });

for (const pwd of ["admin", "admin123", "admin1234", "plexautoplay", "password", "test", ""]) {
  const url = await tryLogin(page, pwd);
  console.log(`pwd="${pwd}" -> ${url}`);
}

await browser.close();
