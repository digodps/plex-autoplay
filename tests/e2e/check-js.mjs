import { chromium } from "@playwright/test";

const BASE = "http://localhost:8080";
const PASSWORD = "admin123";

const browser = await chromium.launch({ headless: true });
const page = await browser.newPage({ ignoreHTTPSErrors: true });
page.on("console", msg => console.log("CONSOLE:", msg.type(), msg.text().slice(0,500)));
page.on("pageerror", err => console.log("PAGE ERROR:", err.message, err.stack?.slice(0,500)));
page.setDefaultTimeout(30000);

try {
  await page.goto(`${BASE}/login`, { waitUntil: "domcontentloaded" });
  await page.fill('input[data-testid="password-input"]', PASSWORD);
  await page.click('button[data-testid="login-submit"]');
  await page.waitForURL(`${BASE}/dashboard`, { timeout: 10000, waitUntil: "domcontentloaded" });
  console.log("Dashboard loaded");
  
  await page.goto(`${BASE}/libraries/1/series`, { waitUntil: "domcontentloaded" });
  await page.waitForTimeout(3000);
  console.log("Series page loaded, URL:", page.url());
  
  // Check if Vue app is mounted
  const vueApp = await page.evaluate(() => {
    const app = document.querySelector('#app');
    return {
      hasApp: !!app,
      innerHTML: app?.innerHTML?.slice(0, 500)
    };
  });
  console.log("Vue app:", JSON.stringify(vueApp, null, 2));
  
  // Check route
  const route = await page.evaluate(() => {
    return window.__VUE_ROUTER_CURRENT_ROUTE__ || "unknown";
  });
  console.log("Route:", route);
  
} catch (e) {
  console.log("ERROR:", e.message);
} finally {
  await browser.close();
}
