# sincronizacao Specification (Delta)

## ADDED Requirements

### Requirement: Sync screen SHALL initiate scan without going black

**The system SHALL** initiate scan without going black: o usuário relatou que, ao clicar em "Iniciar Escaneamento", a tela de `/sync` ficava preta: o status não saía de `idle`, o console de logs não era preenchido e a UI deixava de responder. A causa provável é exceção não tratada no parsing de mensagens SSE (`JSON.parse` sem try/catch), erro 401 silencioso no `apiFetch` redirecionando/descarregando a shell, ou falha do `EventSource` sem feedback.

**Migration**: Nenhuma ação do usuário.

#### Scenario: Starting scan SHALL keep UI rendered and status SHALL leave idle
**- GIVEN** O usuário está autenticado e na tela /sync
**- WHEN** clica em "Iniciar Escaneamento" com status `idle`
**- THEN** o frontend chama `POST /api/plex/scan`
**- THEN** conecta `EventSource` em `/api/sync/log/stream/{jobId}`
**- THEN** o status passa para `running`
**- THEN** a página continua renderizando shell (`AppHeader` + `<main>`)
**- THEN** não há exceção JavaScript não tratada que deixe a tela preta

#### Scenario: SSE error SHALL NOT break UI
**- GIVEN** O `EventSource` recebe erro ou mensagem não-JSON
**- WHEN** o `onerror` é disparado
**- THEN** o `EventSource` é fechado
**- THEN** o status passa para `failed` com feedback visível
**- THEN** a tela continua renderizada (não fica preta)

#### Scenario: 401/HTTP error SHALL NOT unmount shell
**- GIVEN** A chamada à API retorna 401 ou erro de rede
**- WHEN** `apiFetch` é invocada
**- THEN** a UI permanece funcional com mensagem/estado de erro visível
**- THEN** não há redirecionamento silencioso que deixe a tela branca/preta

### Requirement: Sync screen SHALL respect light/dark theme

**The system SHALL** respect light/dark theme: garantir que o fundo, textos e botões da tela de sincronização respeitem o tema configurado (classe `dark` / `data-theme`), evitando fundo preto incorreto em modo claro ou texto ilegível em modo escuro.

**Migration**: Padronizar classes do design system nas telas de sincronização.

#### Scenario: Theme SHALL be applied and toggled correctly
**- GIVEN** O tema configurado é `light`
**- WHEN** O usuário alterna para `dark`
**- THEN** `document.documentElement.dataset.theme` passa para `dark`
**- THEN** A classe `.dark` é aplicada ao elemento raiz
**- THEN** O fundo/texto da tela de sincronização renderizam conforme o tema escuro

#### Scenario: Light theme SHALL persist
**- GIVEN** O tema configurado é `light`
**- THEN** `document.documentElement.dataset.theme` é `light`
**- THEN** O fundo/texto da tela de sincronização renderizam conforme o tema claro

### Requirement: Sync screen menus SHALL work

**The system SHALL** ensure menus work on sync screen: validar que a abertura, navegação, fechamento e estados ativos dos menus/breadcrumb continuam funcionando na tela de sincronização, antes e depois do escaneamento.

**Migration**: Nenhuma.

#### Scenario: Breadcrumb SHALL be visible with Sincronização active
**- GIVEN** O usuário está na tela /sync
**- THEN** O breadcrumb da top bar está visível
**- THEN** O item "Sincronizar" está destacado como ativo
