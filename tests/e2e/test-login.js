const { chromium } = require("@playwright/test");

(async () => {
  const browser = await chromium.launch({ headless: true });
  const context = await browser.newContext({
    ignoreHTTPSErrors: true,
  });
  const page = await context.newPage();

  // Enable console logging
  page.on("console", (msg) => console.log("BROWSER:", msg.text()));
  page.on("pageerror", (err) => console.log("ERROR:", err.message));

  try {
    // Step 1: Go to login page
    console.log("Going to login page...");
    await page.goto("https://localhost:8443/login", { waitUntil: "networkidle" });
    console.log("Current URL:", page.url());

    // Step 2: Fill password and submit
    console.log("Filling password...");
    await page.fill('input[type="password"]', "admin");
    await page.click("button");

    // Step 3: Wait for redirect
    await page.waitForURL("https://localhost:8443/dashboard", { timeout: 10000 });
    console.log("Redirected to:", page.url());

    // Step 4: Check cookies
    const cookies = await context.cookies();
    console.log(
      "Cookies:",
      cookies.filter((c) => c.name === "auth_token"),
    );

    // Step 5: Try to access protected page
    await page.goto("https://localhost:8443/libraries", { waitUntil: "networkidle" });
    console.log("Libraries page URL:", page.url());
    const content = await page.content();
    console.log("Page has libraries:", content.includes("Bibliotecas"));
  } catch (err) {
    console.log("ERROR:", err.message);
    // Take screenshot
    await page.screenshot({ path: "/tmp/login-error.png" });
    console.log("Screenshot saved to /tmp/login-error.png");
  }

  await browser.close();
})();
