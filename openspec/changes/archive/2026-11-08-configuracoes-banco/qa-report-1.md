# QA Report 1 — configuracoes-banco

**Resultado: PASS**

Data: 2026-11-08
URL de validação acessada com Playwright: `http://127.0.0.1:8086` (container
isolado) e `http://127.0.0.1:8085` (execução local).

## Evidências

- `dotnet build PlexAutoplay.slnx` — sucesso, 0 erros.
- `dotnet test tests/PlexAutoplay.ArchitectureTests` — 16/16 testes passando.
- `npm run lint` — 0 erros (8 warnings pré-existentes em outras views).
- `npm run build` — sucesso; bundle `SettingsView-B9eiEQ7L.js`.
- `openspec validate configuracoes-banco --strict` — válido.
- `docker build --no-cache -t plex-autoplay:configuracoes-banco .` — sucesso.
- Container isolado `plex-qa` em `http://127.0.0.1:8086`:
  - `GET /api/settings` sem JWT → 401.
  - `GET /api/settings` com JWT → 200, sem `plex_token`.
  - `PUT /api/settings` inválido → 400 com 4 erros e banco inalterado.
- Playwright (`qa-evidence/qa-settings.mjs`, 10/10 checks):
  - página carrega valores do banco;
  - token não é exibido e estado "configurado" aparece;
  - salvar persiste após reload;
  - recarregar restaura valores;
  - tema alterna light/dark sem erro JS.
- Playwright de menus/tema (`qa-ui-check.mjs`, 18/19 checks): login, toggle
  de tema, abrir/fechar menu de usuário, navegação para `/settings`, sidebar
  responsiva e ausência de overflow. O único FAIL foi "sidebar lista
  bibliotecas" porque o banco de QA está vazio (sem bibliotecas Plex
  sincronizadas), não por regressão desta change.

## Achados

1. Sem falhas funcionais.
2. Observação: o teste genérico de sidebar espera bibliotecas sincronizadas;
   em banco limpo não há itens. Coberto fora do escopo desta change.
3. Limite registrado: token do Plex permanece em texto puro no SQLite local,
   como já ocorria no `.env`; não é retornado pela API.

## Arquivos de evidência

- `openspec/changes/configuracoes-banco/qa-evidence/qa-settings.mjs`
- `openspec/changes/configuracoes-banco/qa-evidence/settings.png`
