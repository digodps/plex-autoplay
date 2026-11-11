// Load environment variables from .env file (mimics the shell 'source .env')
// This is used by E2E acceptance tests to read config values.

const fs = require("fs");
const path = require("path");

// tests/e2e/utils -> raiz do projeto
const projectRoot = path.resolve(__dirname, "..", "..", "..");
const envPath = path.join(projectRoot, ".env");

function loadEnv() {
  if (!fs.existsSync(envPath)) return;

  const envContent = fs.readFileSync(envPath, "utf-8");
  envContent.split("\n").forEach((line) => {
    const trimmed = line.trim();
    if (!trimmed || trimmed.startsWith("#")) return;

    const eqIdx = trimmed.indexOf("=");
    if (eqIdx === -1) return;

    const key = trimmed.substring(0, eqIdx).trim();
    const value = trimmed.substring(eqIdx + 1).trim();
    const cleanValue = value.replace(/^["']|["']$/g, "");

    if (typeof process.env[key] === "undefined") {
      process.env[key] = cleanValue;
    }
  });
}

module.exports = { loadEnv };
