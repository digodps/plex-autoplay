import { chromium } from "@playwright/test";

const BASE = "http://localhost:8080";
const PASSWORD = "admin123";

const browser = await chromium.launch({ headless: true });
const page = await browser.newPage({ ignoreHTTPSErrors: true });
page.on("console", msg => console.log("CONSOLE:", msg.type(), msg.text().slice(0,200)));
page.on("pageerror", err => console.log("PAGE ERROR:", err.message, err.stack?.slice(0,500)));
page.setDefaultTimeout(30000);

try {
  await page.goto(`${BASE}/login`, { waitUntil: "domcontentloaded" });
  await page.fill('input[data-testid="password-input"]', PASSWORD);
  await page.click('button[data-testid="login-submit"]');
  await page.waitForURL(`${BASE}/dashboard`, { timeout: 10000, waitUntil: "domcontentloaded" });
  
  await page.goto(`${BASE}/libraries/1/series`, { waitUntil: "domcontentloaded" });
  await page.waitForTimeout(5000);
  
  console.log("URL:", page.url());
  
  // Check if SeriesView component mounted by looking for its root element
  const seriesView = await page.locator('.series-view').count();
  console.log(".series-view count:", seriesView);
  
  const genreCarousel = await page.locator('.genre-carousel-view').count();
  console.log(".genre-carousel-view count:", genreCarousel);
  
  const skeleton = await page.locator('.genre-carousel-skeleton').count();
  console.log(".genre-carousel-skeleton count:", skeleton);
  
  const error = await page.locator('.alert').count();
  console.log(".alert count:", error);
  
  // Check if useSeriesByGenre data loaded by looking at network
  // Actually just get the full HTML
  const html = await page.content();
  console.log("HTML length:", html.length);
  console.log("Has genre-carousel-view:", html.includes("genre-carousel-view"));
  console.log("Has carousel-card:", html.includes("carousel-card"));
  console.log("Has genre-title:", html.includes("genre-title"));
  
  // Look for "Nenhuma série" or similar
  console.log("Has 'Nenhuma série':", html.includes("Nenhuma série"));
  
} catch (e) {
  console.log("ERROR:", e.message);
} finally {
  await browser.close();
}
