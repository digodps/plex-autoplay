# Design

## Context

A página de sincronização (`/sync`, `vue/plex-frontend/src/views/SyncView.vue`) consome o SSE de `/api/sync/log/stream/{jobId}` (`SyncEndpoints.cs`) e renderiza apenas três contadores. O backend já armazena o job em `ScanJobStore` (memória) com status, contagens e `ErrorMessage`, mas não armazena nem expõe linhas de log. O tema claro/escuro é controlado pelo frontend via classes CSS e variáveis de `style.css`; a mudança `tema-paleta` adiciona a paleta de acento e modo claro/escuro/sistema, mas não toca o console de log.

## Goals / Non-Goals

**Goals:**
- Exibir linhas de log com timestamp, mensagem e nível (info, sucesso, pular, aviso, erro) no console de sincronização.
- Em caso de erro, mostrar sugestão de ação para o usuário corrigir o problema.
- Fundo translúcido ("console transparente") que segue o tema claro/escuro do app.
- Auto-scroll ao chegar nova linha e limite de linhas para não crescer indefinidamente.
- Permitir que o usuário limpe a exibição do console a qualquer momento, sem afetar o histórico do job no backend.

**Non-Goals:**
- Persistir linhas de log no banco de dados (apenas durante a vida do job em memória).
- Logs de outros serviços que não a varredura de sincronização.
- Filtro ou busca dentro do console de log.
- Download/exportação do log.

## Decisions

### Backend: gravar linhas de log no ScanJobStore durante a varredura
Decisão: adicionar uma lista de `ScanLogEntry` (timestamp, mensagem, nível, sugestão) ao `ScanJob` e expor no endpoint SSE.
Razão: mantém o armazenamento em memória já usado por `ScanJobStore`, sem nova tabela ou dependência; o frontend já consome esse endpoint.
Alternativa considerada: novo endpoint dedicado `/api/sync/log/lines` — rejeitada por duplicar o stream SSE já existente.

### SSE: enviar linhas de log junto aos contadores
Decisão: estender o payload do SSE para incluir `logs` (array de entradas) além de `status`, `processed`, `skipped`, `errors`.
Razão: uma única conexão SSE entrega progresso e log; não exige nova conexão nem polling.
Alternativa considerada: endpoint HTTP separado para log — rejeitada por adicionar complexidade e desacoplar progresso do log.

### Frontend: painel de console com ícones por nível e sugestão de erro
Decisão: reescrever `SyncView.vue` com lista de linhas de log, ícone ao lado de cada linha (▶ info, ✔ sucesso, ⏭ pular, ⚠ aviso, ✖ erro) e sugestão de ação destacada em linhas de erro.
Razão: ícones facilitam a interpretação rápida; sugestão de erro responde ao pedido do usuário de saber o que fazer para corrigir.
Alternativa considerada: tabela simples sem ícones — rejeitada por dificultar a leitura rápida.

### Frontend: botão "Limpar console" (client-side only)
Decisão: adicionar um botão "Limpar console" no cabeçalho do painel de log que esconde as linhas da exibição (define a lista local como vazia), sem enviar comando ao backend.
Razão: é a opção mais simples e de baixo risco; não exige novo endpoint nem altera o contrato SSE; o histórico do job permanece disponível se o usuário recarregar a página ou abrir outra aba.
Alternativa considerada: endpoint server-side para apagar o log do job — rejeitada por adicionar complexidade e risco de apagar um diagnóstico ainda em andamento.

### Visual: fundo vidro/translúcido seguindo o tema
Decisão: `backdrop-filter: blur()` + `background: color-mix(in srgb, var(--surface) 55%, transparent)`, fonte monospace, borda sutil, classe `.theme-dark` já aplicada no elemento raiz pelo tema.
Razão: aparência de console transparente que se adapta ao modo claro/escuro/sistema sem quebrar o design existente.
Alternativa considerada: fundo escuro fixo tipo terminal — rejeitada por não seguir o tema claro do app quando selecionado.

## Risks / Trade-offs

- [Linhas de log perdidas ao recarregar a página] → Mitigação: o job vive em memória e é recriado a cada novo scan; aceitável para um console de acompanhamento em tempo real.
- [Múltiplas abas conectadas ao mesmo SSE] → Mitigação: cada aba abre seu próprio EventSource; não há sincronização de leitura entre abas (aceitável para acompanhamento pontual).
- [Crescimento ilimitado da lista de log] → Mitigação: limitar a 100 linhas e remover as mais antigas.
