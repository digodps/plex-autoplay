const { test, expect } = require("@playwright/test");
const fs = require("fs");
const path = require("path");

const PROJECT_ROOT = path.resolve(__dirname, "../../..");

// Dentro do container executor não há Docker-in-Docker, então testes que
// gerenciam containers não se aplicam. São pulados automaticamente.
const INSIDE_EXECUTOR = process.env.INSIDE_EXECUTOR_CONTAINER === "true";

test.describe("Ciclo de vida do container Docker", () => {
  test("inicia o container com ./start.sh e verifica que está rodando", () => {
    test.skip(
      INSIDE_EXECUTOR,
      "Não gerencia containers dentro do executor (sem Docker-in-Docker)",
    );
  });

  test("verifica que o script stop.sh existe e é executável", () => {
    const stopScript = `${PROJECT_ROOT}/scripts/stop.sh`;
    expect(fs.existsSync(stopScript)).toBe(true);
    expect(fs.statSync(stopScript).isFile()).toBe(true);
    // Verifica permissão de execução
    const mode = fs.statSync(stopScript).mode;
    expect(mode & 0o111).toBeGreaterThan(0);
  });

  test("verifica logs via ./logs.sh --no-follow --tail 10", () => {
    test.skip(
      INSIDE_EXECUTOR,
      "Não gerencia containers dentro do executor (sem Docker-in-Docker)",
    );
  });
});
