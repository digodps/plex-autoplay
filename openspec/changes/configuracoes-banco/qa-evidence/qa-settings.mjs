import { chromium } from "@playwright/test";

const BASE = process.env.PLAYWRIGHT_BASE_URL || "http://127.0.0.1:8085";
const PASS = process.env.ADMIN_PASSWORD || "operator123";
const results = [];
const check = (name, ok, extra = "") => {
  results.push({ name, ok });
  console.log(`${ok ? "PASS" : "FAIL"} | ${name}${extra ? ` | ${extra}` : ""}`);
};

const browser = await chromium.launch();
const page = await browser.newPage({ viewport: { width: 1366, height: 900 } });
const errors = [];
page.on("pageerror", (e) => errors.push(e.message));

await page.goto(`${BASE}/login`, { waitUntil: "networkidle" });
await page.fill('[data-testid="password-input"]', PASS);
await page.click('[data-testid="login-submit"]');
await page.waitForURL("**/dashboard", { timeout: 15000 });

await page.goto(`${BASE}/settings`, { waitUntil: "networkidle" });
check("settings carrega do banco", (await page.inputValue("#sync-interval")) === "9", await page.inputValue("#sync-interval"));
check("catalog interval carrega do banco", (await page.inputValue("#catalog-interval")) === "33", await page.inputValue("#catalog-interval"));
check("media root carrega do banco", (await page.inputValue("#media-root")) === "/home/hermes/media", await page.inputValue("#media-root"));
check("token nao e exibido", (await page.inputValue("#plex-token")) === "");
check("token configurado sinalizado", (await page.locator("text=Token já configurado").count()) === 1);

await page.fill("#sync-interval", "11");
await page.click('button[type="submit"]');
await page.waitForSelector("text=Configurações salvas com sucesso", { timeout: 15000 });
check("salvamento exibe sucesso", true);

await page.reload({ waitUntil: "networkidle" });
check("valor persiste apos reload", (await page.inputValue("#sync-interval")) === "11", await page.inputValue("#sync-interval"));

await page.fill("#sync-interval", "9");
await page.click('button[type="submit"]');
await page.waitForSelector("text=Configurações salvas com sucesso", { timeout: 15000 });
check("restaura valor original", (await page.inputValue("#sync-interval")) === "9");

await page.getByRole("button", { name: /Tema (claro|escuro)/ }).click();
await page.waitForTimeout(200);
const theme = await page.evaluate(() => document.documentElement.dataset.theme);
check("tema alterna", theme === "dark" || theme === "light", theme);

await page.screenshot({ path: "/tmp/opencode/plex-qa/settings.png", fullPage: true });
check("sem erros JS", errors.length === 0, errors.join("; "));
await browser.close();

const failed = results.filter((r) => !r.ok);
console.log(`\n${results.length - failed.length}/${results.length} checks passed`);
process.exit(failed.length ? 1 : 0);
