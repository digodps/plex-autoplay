import { chromium } from "@playwright/test";
const BASE = "http://localhost:8080";
const browser = await chromium.launch({ headless: true });
const page = await browser.newPage({ ignoreHTTPSErrors: true });
page.setDefaultTimeout(30000);
page.on("pageerror", e => console.log("PAGEERR:", e.message, e.stack?.slice(0,500)));
page.on("console", msg => console.log("CONSOLE:", msg.type(), msg.text().slice(0,300)));

await page.goto(`${BASE}/login`, { waitUntil: "domcontentloaded" });
await page.fill('input[data-testid="password-input"]', "admin123");
await page.click('button[data-testid="login-submit"]');
await page.waitForURL(`${BASE}/dashboard`, { timeout: 10000 });
console.log("Dashboard loaded");
await page.goto(`${BASE}/libraries/1/series`, { waitUntil: "domcontentloaded" });
console.log("Navigation done");
console.log("URL:", page.url());
await browser.close();
