# Tasks

## 1. Correção do achado [10]

- [x] 1.1 Aplicar correção nos arquivos: tests/e2e/tests/markers.spec.js e verificar que o achado desaparece na próxima execução de /code-review

## Resolução

markers.spec.js dividido por responsabilidade: CRUD de API em markers.spec.js (106 linhas) e catálogo/editor de UI em tests/series-editor.spec.js (80 linhas); helpers loginByUi/bearerToken movidos para tests/utils/auth.js. E2E 23 passed + 2 skipped.

## Workflow follow-up

- Validar com `openspec validate --change fix-10-clean-code-/-solid` antes de seguir para archive/apply.
