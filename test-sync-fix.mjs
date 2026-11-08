import { chromium } from "@playwright/test";

const BASE = process.env.PLAYWRIGHT_BASE_URL || "http://192.168.18.5:8080";
const PASS = process.env.ADMIN_PASSWORD || "admin";
const results = [];
const check = (name, ok, extra = "") => {
  results.push({ name, ok, extra });
  console.log(`${ok ? "PASS" : "FAIL"} | ${name}${extra ? ` | ${extra}` : ""}`);
};

const browser = await chromium.launch({ headless: true });
const ctx = await browser.newContext({ viewport: { width: 1366, height: 900 } });
const page = await ctx.newPage();
page.on("pageerror", (e) => console.log("PAGEERROR:", e.message));

// login
await page.goto(`${BASE}/login`, { waitUntil: "networkidle" });
await page.fill('[data-testid="password-input"]', PASS);
await page.click('[data-testid="login-submit"]');
await page.waitForURL("**/dashboard", { timeout: 15000 });
check("login", page.url().includes("/dashboard"));

// go to sync page
await page.goto(`${BASE}/sync`, { waitUntil: "networkidle" });
await page.waitForSelector('button:has-text("Iniciar Escaneamento")', { state: "visible", timeout: 10000 });

// click start scan
await page.click('button:has-text("Iniciar Escaneamento")');
console.log("Scan iniciado, aguardando conclusão...");

// wait for status "completed"
await page.waitForSelector('.rounded-full:has-text("completed")', { timeout: 60000 });
console.log("Status completed detectado");

// verify no "Invalid Date" in logs
await page.waitForTimeout(500);
const logLines = await page.locator('div[class*="flex"]:has(span.flex-none)').allTextContents();
let invalidDates = 0;
let hasTimestamp = false;
let hasMessages = false;
let hasSkipIcon = false;

for (const line of logLines) {
  if (line.includes("Invalid Date")) invalidDates++;
  if (/\d{2}\/\d{2}\/\d{4}, \d{2}:\d{2}:\d{2}/.test(line)) hasTimestamp = true;
  if (line.includes("biblioteca") || line.includes("pulad") || line.includes("processad")) hasMessages = true;
  if (line.includes("⏭") || line.includes("⏭️") || line.includes("skip") || line.includes("info")) hasSkipIcon = true;
}

check("sem Invalid Date nos logs", invalidDates === 0, invalidDates > 0 ? `${invalidDates} ocorrências` : "");
check("timestamps formatados pt-BR", hasTimestamp);
check("mensagens de log visíveis", hasMessages);
check("ícones de nível aparecem", hasSkipIcon);

// verify status badge
const statusText = await page.locator('.rounded-full:has-text("completed")').first().textContent();
check("status final completed (não failed)", statusText?.includes("completed") ?? false, statusText?.trim());

// screenshot
await page.screenshot({ path: "/tmp/opencode/sync-fix.png", fullPage: true });
console.log("Screenshot: /tmp/opencode/sync-fix.png");

await browser.close();
const failed = results.filter(r => !r.ok).length;
process.exit(failed > 0 ? 1 : 0);