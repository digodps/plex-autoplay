// Helpers de autenticação compartilhados pelos specs E2E.
// Lê as variáveis de ambiente no momento da chamada (loadEnv() roda no spec).

const DEFAULT_BASE_URL = "https://plex-autoplay.dns.podman:443";

function baseUrl() {
  return process.env.PLAYWRIGHT_BASE_URL || DEFAULT_BASE_URL;
}

async function loginByUi(page) {
  await page.goto(`${baseUrl()}/login`, { waitUntil: "networkidle" });
  await page.waitForSelector('input[type="password"]', {
    state: "visible",
    timeout: 10000,
  });
  await page.fill('input[type="password"]', process.env.ADMIN_PASSWORD);
  await page.click("button");
  await page.waitForURL(`${baseUrl()}/dashboard`);
}

async function bearerToken(context) {
  const cookies = await context.cookies();
  const authCookie = cookies.find((c) => c.name === "auth_token");
  return authCookie?.value ?? "";
}

module.exports = { loginByUi, bearerToken };
