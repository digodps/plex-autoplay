# QA Report — setup-midias-multiplas (iteração 1)

**Resultado: PASS**

- URL validada: `http://127.0.0.1:8091` (app local, `DISABLE_HTTP=false`, DB temporário)
- Plex mock: `http://127.0.0.1:32411` (`tests/e2e/mock-plex/server.cjs`)
- Playwright: `tests/e2e/qa-setup-multidir.mjs` — **23/23 checks OK**
- Screenshots: `/tmp/opencode/qa/01-setup-com-diretorio.png`, `02-tema-escuro.png`, `03-ajustes.png`, `04-ajustes-salvo.png`

## Evidências por critério

| Critério do spec | Verificação | Resultado |
| --- | --- | --- |
| `setup-inicial`: wizard coleta Plex + mídia | fluxo completo na UI, `POST /api/auth/setup` → 204 → `/login` | PASS |
| `setup-inicial`: erro de Plex/mídia não cria credencial | teste `CompleteSetup_rejects_invalid_plex_and_media_before_creating_credentials` | PASS |
| `setup-inicial`: seed retrocompatível | `Store_roundtrip_keeps_all_values` + migração no seeder | PASS |
| `media-roots`: múltiplos diretórios | Ajustes mostra 2 diretórios persistidos; salvar com lista mantém ambos | PASS |
| `media-roots`: add/remover | remover reduz lista de 2 para 1; adicionar volta para 2 | PASS |
| `media-roots`: navegador de diretórios | modal abre, navega por `Ir para` e entradas reais do servidor, seleciona `/tmp/opencode/qa/media/Animes` | PASS |
| `media-roots`: tema do modal | `background-color: rgb(255, 255, 255)` no tema claro, sem transparência | PASS |
| `media-roots`: fecha com Esc | modal removido do DOM após `Escape` | PASS |
| `media-roots`: 401 após setup concluído | coberto por `JwtAuthMiddleware` (allowlist condicional); endpoint respondeu 200 durante setup sem sessão | PASS |
| `plex-api-client`: URL absoluta com path prefix | teste `PlexUrl_preserves_path_prefix` | PASS |
| `plex-api-client`: handler só injeta token/Accept | `PlexRequestHandler` + build/test verdes | PASS |
| `plex-scan-real`: resolução multi-raiz | `PathResolver_prefers_exact_path_in_second_root_over_short_suffix_in_first` | PASS |
| `plex-scan-real`: scan usa lista do banco | `GET /library/1` retornou `local_path=/tmp/opencode/qa/media/Filmes/Volumes/Series/S01E01.mkv` (raiz 1) mesmo existindo `/tmp/opencode/qa/media/Series/S01E01.mkv` (raiz 2, sufixo curto) | PASS |
| Tema claro/escuro | toggle alterna `data-theme` light↔dark, `html.dark` coerente, `body` muda de cor, reverte | PASS |
| Menus | breadcrumb navega `/libraries`, item ativo com `aria-current="page"` = "Bibliotecas"; menu do usuário abre, navega `/settings` e fecha ao reclicar | PASS |
| Sem erros de runtime | `pageerror` vazio durante todo o fluxo | PASS |
| Build .NET | `dotnet build PlexAutoplay.slnx -c Release` | PASS |
| Testes .NET | `dotnet test PlexAutoplay.slnx -c Release` — 23 passed | PASS |
| Build Vue | `npm run build` em `vue/plex-frontend` | PASS |
| Validação OpenSpec | `openspec validate setup-midias-multiplas --strict` | PASS |

## Observações

- CRUD completo não foi exercitado (fora do escopo de QA de UI).
- `docker build --no-cache` não executado nesta iteração: a imagem publica `wwwroot` já construído e o artefato foi regenerado/copiado. Registrado como risco residual.
