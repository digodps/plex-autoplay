# Design — setup-midias-multiplas

## Decisões

### 1. Armazenamento das raízes

Nova chave `MEDIA_ROOTS` em `AppSetting`, valor JSON array (`["/media/Animes","/mnt/nas"]`). Sem tabela nova: a lista é pequena, sempre lida em bloco e não precisa de query por item.

- `AppSettingsSnapshot.MediaRoots` (`IReadOnlyList<string>`) substitui `MediaRoot`.
- `SqliteAppSettingsStore.GetAsync`: `MEDIA_ROOTS` ausente/inválido → `MEDIA_ROOT` legado (banco) → default `["/media"]`.
- `SaveAsync`: serializa a lista **verbatim** (já normalizada pela Application).
- `AppSettingsSeeder`: tabela vazia → grava `MEDIA_ROOTS` do ambiente (JSON ou separado por `\n`, `,` ou `:`) → senão `MEDIA_ROOT` do ambiente → senão default. Passo adicional de migração, independente do contador da tabela: se `MEDIA_ROOTS` não existir, derivar de `MEDIA_ROOTS`/`MEDIA_ROOT` do ambiente ou da linha `MEDIA_ROOT` existente e gravar. Um único parser.
- `AppSettingKeys.MediaRoot` permanece apenas como chave legada de leitura.

### 2. Validação e canonicalização

Função única em `MediaRoots` (Application), usada por setup e settings:

```
MediaRoots.Normalize(IEnumerable<string>?) -> (ok: IReadOnlyList<string>, errors: IReadOnlyList<string>)
```

Regras: `Path.GetFullPath`, remover barra final (exceto `/`), rejeitar `/` e caminho não absoluto, exigir `Directory.Exists`, dedupe preservando ordem, máximo 32 itens (excedente → erro `media_roots: máximo de 32 diretórios`). Erro por item: `media_roots: <caminho> não é um diretório absoluto existente`.

Normalização acontece **somente** aqui; `SqliteAppSettingsStore` não corrige entrada.

IO de filesystem na Application é mantido de propósito: `UpdateAppSettingsUseCase` já usa `Directory.Exists` e a regra é validável em teste unitário com diretórios temporários. Não criar porta só para embrulhar `Directory.Exists`.

### 3. Resolução de caminho com várias raízes

`MediaPathResolver.BuildCandidates(roots, plexFilePath)` ordena por **especificidade**, não por raiz: do caminho completo ao sufixo mais curto; dentro de cada sufixo, raízes na ordem configurada. Assim o caminho exato em qualquer raiz vence um sufixo curto homônimo em outra raiz. `Fallback` = primeira raiz + caminho completo normalizado. Lista de uma raiz = comportamento atual.

`IPathResolver.Resolve(IReadOnlyList<string> mediaRoots, string plexFilePath)`; `PathResolver` mantém `File.Exists` como critério. A lista continua explícita no parâmetro (sem injetar `IAppSettingsStore` no resolver) para manter o resolver testável e sem estado.

### 4. Navegador de diretórios

`GET /api/filesystem/directories?path=/media`:

```json
{ "path": "/media", "parent": "/", "entries": [{ "name": "Animes", "path": "/media/Animes" }] }
```

- Autenticação: sessão obrigatória, **exceto** quando `setupRequired` (mesma regra de `/api/auth/setup/status`), pois o wizard roda antes de existir credencial. Liberação condicional no `JwtAuthMiddleware`.
- `path` omitido ou `/` → `path: "/"`, `parent: null`, entradas do mount raiz.
- Só diretórios, ordenados `OrdinalIgnoreCase` por nome, ocultos (`.*`) filtrados, cap de 2000 entradas.
- `path` relativo → `400`; inexistente → `404`; sem permissão → `403`.
- Sem jail em `MEDIA_ROOTS`: o objetivo é justamente descobrir caminhos novos montados no container. O app roda com o usuário do operador; exigir sessão é o controle de acesso.
- `..` são resolvidos por `Path.GetFullPath`; symlinks não são expandidos (resposta mostra o caminho navegável).

### 5. Setup inicial

`POST /api/auth/setup` recebe `{ token, newPassword, plexUrl, plexToken, mediaRoots[] }`.

Ordem obrigatória (falha em qualquer passo anterior a `CreateAsync` permite repetir o setup):

1. token de setup válido
2. política de senha
3. `plexUrl` `http`/`https` absoluta (mesmo validador de `UpdateAppSettingsUseCase`)
4. `MediaRoots.Normalize` (diretórios locais validados antes da chamada de rede)
5. `GetIdentityAsync(plexUrl, plexToken)` com timeout de 10s
6. `IAppSettingsStore.SaveAsync` (Plex + raízes)
7. `IAuthCredentialStore.CreateAsync`
8. `Invalidate` do token de setup

Se o passo 7 falhar, configuração já está gravada e o setup permanece repetível (credencial inexistente). O `CompleteSetupUseCase` passa a ser `Transient` no DI (depende do typed client do Plex).

Erros: `401 { message }` token de setup; `400 { errors: string[] }` (prefixo `plex_url:`, `plex_token:`, `media_roots:`); `409` já configurado; `204` sucesso. Plex inacessível **não** cria credencial — o formulário continua disponível com os valores preenchidos, então não há lockout.

### 6. Fix do transporte Plex

- `PlexApiClient` recebe `IAppSettingsStore` e monta URL absoluta via helper único `BuildUrl(settings.PlexUrl, path)`: `PlexUrl.TrimEnd('/') + "/" + path.TrimStart('/')`. Preserva path prefix (`https://host/plex/library/sections`).
- Aplicar a **todos** os métodos: `GetLibrariesAsync`, `GetEpisodesAsync`, `GetSeriesAsync`, `GetSeriesEpisodesAsync`, `GetIdentityAsync` (este recebe URL/token explícitos).
- `GetIdentityAsync` envia `Accept: application/xml` (parser usa `XDocument`).
- `PlexRequestHandler`: remove a reescrita de `RequestUri`; injeta `X-Plex-Token` (se ausente) e `Accept: application/json` (se ausente). Requisição sem URL absoluta → `InvalidOperationException` explícita.
- Verificação final: nenhum `GetAsync("/...")` sobra em `src/PlexAutoplay.Infrastructure/PlexProxy`.

### 7. UI

- `MediaRootsField.vue`: v-model `string[]`; linhas com valor + remover, input + "Adicionar", "Procurar…" abrindo `DirectoryPicker.vue`. `data-testid`: `media-root-add`, `media-root-remove`, `media-root-open-picker`, `media-root-value`.
- `DirectoryPicker.vue`: modal com caminho atual, lista, "Selecionar este diretório", "Cancelar"; fecha com Esc/backdrop. `data-testid`: `directory-picker`, `directory-picker-entry`, `directory-picker-select`, `directory-picker-cancel`.
- `SetupView.vue`: wizard de 3 passos (senha → Plex → mídia) com "Voltar/Continuar", "Testar conexão" no passo do Plex via `POST /api/plex/identity` (liberado sem sessão apenas enquanto `setupRequired`); erros de `errors[]` exibidos como lista, sem limpar campos. Ao receber `204`, faz login com a senha criada (`POST /auth/login`) e entra no dashboard já autenticado.
- `SettingsView.vue`: `media_roots` via `MediaRootsField`.
- `api.js`: helper `extractErrors(res) -> string[]` usado por setup e settings.

### Riscos / mitigação

| Risco | Mitigação |
| --- | --- |
| Endpoint de filesystem exposto sem sessão | liberado só quando `setupRequired`; depois exige sessão |
| Setup quebrar instalações existentes com `MEDIA_ROOT` | fallback de leitura + migração no seeder |
| Muitas raízes degradarem scan | limite 32; sonda para no primeiro `File.Exists` |
| Plex atrás de proxy com path prefix | `BuildUrl` + teste com `/plex` |
| Credencial criada sem configuração | settings gravadas antes da credencial |
| E2E existente quebrar com novo payload | `tests/e2e/global-setup.js` e mock `/identity` atualizados na mesma change |
