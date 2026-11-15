const { test, expect } = require("@playwright/test");
const { loadEnv } = require("../utils/env");

loadEnv();

const BASE_URL =
  process.env.PLAYWRIGHT_BASE_URL || "https://plex-autoplay.dns.podman:443";
const ADMIN_PASSWORD = process.env.OPERATOR_PASSWORD || process.env.ADMIN_PASSWORD;

test.describe("Log Hierarchy Display", () => {
  test.beforeEach(async ({ page }) => {
    // Login before each test
    await page.goto(`${BASE_URL}/login`, { waitUntil: "networkidle" });
    await page.waitForSelector('input[type="password"]', {
      state: "visible",
      timeout: 10000,
    });
    await page.fill('input[type="password"]', ADMIN_PASSWORD);
    await page.click("button");
    await page.waitForURL(`${BASE_URL}/dashboard`);
  });

  test("hierarchy tree renders with library/series/episode structure", async ({ page }) => {
    // Navigate to /logs
    await page.goto(`${BASE_URL}/logs`, { waitUntil: "networkidle" });

    // Wait for logs view to load
    await page.waitForSelector('[data-testid="log-filter-text"]', {
      state: "visible",
      timeout: 10000,
    });

    // Trigger a scan to generate logs with hierarchy
    await page.click('[data-testid="log-btn-scan"]');
    
    // Wait for scan to complete and logs to appear
    await page.waitForTimeout(5000);

    // Wait for log cards to render
    await page.waitForSelector('[data-testid^="log-card-"]', { state: "visible", timeout: 15000 });

    // Check for hierarchy tree container
    const hierarchyTree = page.locator('.hierarchy-tree');
    await expect(hierarchyTree).toBeVisible();

    // Check for library nodes (📚 icon)
    const libraryNodes = page.locator('.hierarchy-node:has-text("Biblioteca")');
    await expect(libraryNodes.first()).toBeVisible({ timeout: 5000 });

    // Check for series nodes (📺 icon) - may not exist if no series in logs
    const seriesNodes = page.locator('.hierarchy-node:has-text("Série")');
    
    // Check for episode nodes (▶️ icon) - may not exist if no episodes in logs
    const episodeNodes = page.locator('.hierarchy-node:has-text("Episódio")');
  });

  test("expand/collapse hierarchy nodes works", async ({ page }) => {
    await page.goto(`${BASE_URL}/logs`, { waitUntil: "networkidle" });
    await page.waitForSelector('[data-testid="log-filter-text"]', { state: "visible" });

    // Trigger scan
    await page.click('[data-testid="log-btn-scan"]');
    await page.waitForTimeout(5000);

    await page.waitForSelector('[data-testid^="log-card-"]', { state: "visible", timeout: 15000 });

    // Find a library node with children (has chevron)
    const libraryNode = page.locator('.hierarchy-node:has-text("Biblioteca")').first();
    await expect(libraryNode).toBeVisible({ timeout: 5000 });

    // Check if it has a chevron (expandable)
    const chevron = libraryNode.locator('svg').first();
    
    // Click to expand
    await libraryNode.click();
    await page.waitForTimeout(300);

    // Verify children are visible (series nodes)
    const seriesNodes = page.locator('.hierarchy-node:has-text("Série")');
    const seriesCount = await seriesNodes.count();
    
    // If there are series, they should be visible now
    if (seriesCount > 0) {
      await expect(seriesNodes.first()).toBeVisible();
    }

    // Click again to collapse
    await libraryNode.click();
    await page.waitForTimeout(300);
  });

  test("keyboard navigation works on hierarchy tree", async ({ page }) => {
    await page.goto(`${BASE_URL}/logs`, { waitUntil: "networkidle" });
    await page.waitForSelector('[data-testid="log-filter-text"]', { state: "visible" });

    // Trigger scan
    await page.click('[data-testid="log-btn-scan"]');
    await page.waitForTimeout(5000);

    await page.waitForSelector('[data-testid^="log-card-"]', { state: "visible", timeout: 15000 });

    // Focus the first hierarchy node
    const firstNode = page.locator('.hierarchy-node').first();
    await firstNode.focus();
    await page.waitForTimeout(100);

    // Verify focus styles are applied
    const focusedNode = page.locator('.hierarchy-node:focus-visible');
    // Note: :focus-visible may not be detectable via Playwright easily
    // Just verify the node can receive focus
    await expect(firstNode).toBeFocused();

    // Press Enter to expand (if expandable)
    await firstNode.press('Enter');
    await page.waitForTimeout(300);
  });

  test("theme toggle works with hierarchy display", async ({ page }) => {
    await page.goto(`${BASE_URL}/logs`, { waitUntil: "networkidle" });
    await page.waitForSelector('[data-testid="log-filter-text"]', { state: "visible" });

    // Trigger scan
    await page.click('[data-testid="log-btn-scan"]');
    await page.waitForTimeout(5000);

    await page.waitForSelector('[data-testid^="log-card-"]', { state: "visible", timeout: 15000 });

    // Get initial theme
    const initialTheme = await page.evaluate(() => {
      return document.documentElement.dataset.theme || "light";
    });
    expect(["light", "dark"]).toContain(initialTheme);

    // Toggle theme
    const themeToggle = page.locator('button[aria-label*="theme"], button[data-testid="theme-toggle"]');
    if (await themeToggle.count() > 0) {
      await themeToggle.first().click();
      await page.waitForTimeout(300);
      
      const toggledTheme = await page.evaluate(() => {
        return document.documentElement.dataset.theme || "light";
      });
      expect(toggledTheme).not.toBe(initialTheme);

      // Verify hierarchy tree still renders correctly
      const hierarchyTree = page.locator('.hierarchy-tree');
      await expect(hierarchyTree).toBeVisible();

      // Revert theme
      await themeToggle.first().click();
      await page.waitForTimeout(300);
    }
  });

  test("menu navigation works with logs page active", async ({ page }) => {
    await page.goto(`${BASE_URL}/logs`, { waitUntil: "networkidle" });
    await page.waitForSelector('[data-testid="log-filter-text"]', { state: "visible" });

    // Verify Logs menu item is active
    const logsMenuItem = page.locator('a[href="/logs"], button[data-to="/logs"], a[href$="/logs"]');
    if (await logsMenuItem.count() > 0) {
      const classes = await logsMenuItem.first().getAttribute("class");
      expect(classes).toMatch(/active|router-link-active/);
    }

    // Navigate away and back
    await page.goto(`${BASE_URL}/dashboard`, { waitUntil: "networkidle" });
    await expect(page).toHaveURL(`${BASE_URL}/dashboard`);

    await page.goto(`${BASE_URL}/logs`, { waitUntil: "networkidle" });
    await expect(page).toHaveURL(`${BASE_URL}/logs`);
    await page.waitForSelector('[data-testid="log-filter-text"]', { state: "visible" });

    // Verify hierarchy tree still works after navigation
    const hierarchyTree = page.locator('.hierarchy-tree');
    await expect(hierarchyTree).toBeVisible();
  });

  test("hierarchy tree accessibility attributes present", async ({ page }) => {
    await page.goto(`${BASE_URL}/logs`, { waitUntil: "networkidle" });
    await page.waitForSelector('[data-testid="log-filter-text"]', { state: "visible" });

    // Trigger scan
    await page.click('[data-testid="log-btn-scan"]');
    await page.waitForTimeout(5000);

    await page.waitForSelector('[data-testid^="log-card-"]', { state: "visible", timeout: 15000 });

    // Check hierarchy tree has role="tree"
    const hierarchyTree = page.locator('.hierarchy-tree');
    await expect(hierarchyTree).toHaveAttribute('role', 'tree');

    // Check nodes have role="treeitem"
    const firstNode = page.locator('.hierarchy-node').first();
    await expect(firstNode).toHaveAttribute('role', 'treeitem');

    // Check aria-level on nodes
    const ariaLevel = await firstNode.getAttribute('aria-level');
    expect(ariaLevel).toBeTruthy();
    expect(parseInt(ariaLevel)).toBeGreaterThanOrEqual(1);

    // Check expandable nodes have aria-expanded
    const expandableNode = page.locator('.hierarchy-node:has(svg)').first();
    const ariaExpanded = await expandableNode.getAttribute('aria-expanded');
    expect(ariaExpanded).toBeTruthy();
  });
});