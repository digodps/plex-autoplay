import { chromium } from "@playwright/test";

const BASE = process.argv[2] || "http://localhost:8080";
const PASSWORD = process.env.ADMIN_PASSWORD || "admin123";

const results = [];
function check(name, ok, detail = "") {
  results.push({ name, ok, detail });
  console.log(`${ok ? "PASS" : "FAIL"}  ${name}${detail ? ` | ${detail}` : ""}`);
}

try {
  const browser = await chromium.launch({ headless: true });
  const page = await browser.newPage({ ignoreHTTPSErrors: true });
  page.setDefaultTimeout(30000);

  // Auth
  await page.goto(`${BASE}/login`, { waitUntil: "domcontentloaded" });
  await page.waitForSelector('[data-testid="login-form"]', { timeout: 10000 });
  check("Login page loads", true);
  await page.fill('input[data-testid="password-input"]', PASSWORD);
  await page.click('button[data-testid="login-submit"]');
  await page.waitForURL(`${BASE}/dashboard`, { timeout: 10000, waitUntil: "domcontentloaded" });
  check("Login redirects", page.url().includes("/dashboard"));

  // Go straight to the series page
  await page.goto(`${BASE}/libraries/1/series`, { waitUntil: "domcontentloaded" });
  await page.waitForFunction(() => document.querySelector('.genre-carousel-view') !== null, { timeout: 20000 });
  await page.waitForTimeout(1000);
  check("Series page renders genre carousel", true, page.url());

  // Genre sections
  const genreTitles = page.locator('.genre-title');
  const titles = [];
  for (let i = 0; i < await genreTitles.count(); i++) titles.push((await genreTitles.nth(i).textContent()).trim());
  check("Genre sections rendered", titles.length > 0, `${titles.length} sections: ${titles.join(", ")}`);
  check("Expected genres present (Action/Animation/Comedy/Crime/Drama)",
    ["Action","Animation","Comedy","Crime","Drama"].every(g => titles.includes(g)), titles.join(", "));
  check("'Outros' section last", titles[titles.length - 1] === "Outros", `last=${titles[titles.length-1]}`);

  // Cards / tracks
  const cards = page.locator('.carousel-card');
  const cardCount = await cards.count();
  check("Carousel cards rendered", cardCount > 0, `${cardCount} cards`);
  const tracks = page.locator('.carousel-track');
  check("Carousel tracks present", await tracks.count() > 0, `${await tracks.count()} tracks`);

  // Scroll buttons
  const wrapper = page.locator('.carousel-wrapper').first();
  await wrapper.hover();
  await page.waitForTimeout(300);
  const rightBtn = page.locator('.carousel-btn-right').first();
  const rightVisible = await rightBtn.isVisible().catch(() => false);
  check("Right scroll button appears on hover", rightVisible, `visible=${rightVisible}`);

  // Hover scale
  const firstCard = cards.first();
  await firstCard.hover();
  await page.waitForTimeout(300);
  const transform = await firstCard.evaluate(el => getComputedStyle(el).transform);
  check("Card hover triggers scale transform", transform.includes("scale") || transform.includes("matrix"), `transform=${transform}`);

  // Mouse wheel horizontal scroll
  const track = tracks.first();
  const before = await track.evaluate(el => el.scrollLeft);
  await track.hover();
  await page.mouse.wheel(300, 0);
  await page.waitForTimeout(300);
  const after = await track.evaluate(el => el.scrollLeft);
  check("Mouse wheel scrolls track horizontally", after > before, `scrollLeft ${before} -> ${after}`);

  // Click a card navigates to markers
  await firstCard.click();
  await page.waitForURL(/\/markers/, { timeout: 10000, waitUntil: "domcontentloaded" }).catch(() => {});
  check("Card click navigates to markers", page.url().includes("/markers"), page.url());

  // Theme toggle on series page
  await page.goBack({ waitUntil: "domcontentloaded" }).catch(() => {});
  await page.waitForTimeout(500);
  const themeBtn = page.locator('button[aria-label="Tema claro"], button[aria-label="Tema escuro"]').first();
  const darkBefore = await page.locator("html").evaluate(el => el.classList.contains("dark"));
  await themeBtn.click();
  await page.waitForTimeout(300);
  const darkAfter = await page.locator("html").evaluate(el => el.classList.contains("dark"));
  check("Theme toggle works on series page", darkBefore !== darkAfter, `${darkBefore} -> ${darkAfter}`);

  await browser.close();

  const failed = results.filter((r) => !r.ok);
  console.log("\n=== RESUMO QA GENRE CAROUSEL ===");
  console.log(`Total: ${results.length}, Passed: ${results.length - failed.length}, Failed: ${failed.length}`);
  if (failed.length > 0) {
    console.log("Achados:");
    failed.forEach((f) => console.log(`  - ${f.name} ${f.detail}`));
    process.exitCode = 1;
  } else {
    console.log("RESULT: PASS");
    process.exitCode = 0;
  }
} catch (err) {
  check("Execução da QA", false, err.message);
  console.error(err);
  process.exitCode = 1;
}