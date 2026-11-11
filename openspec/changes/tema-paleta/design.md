# Design

## Context

A tela inicial (`DashboardView.vue`) exibe o tema amarelo fixo definido em `src/style.css` (`--accent: #e5a00d`), e o modo escuro depende exclusivamente de `prefers-color-scheme` do SO, sem controle do usuário. O backend é .NET com SQLite único (`/data/app.db`), autenticação via JWT em cookie, endpoints organizados em classes estáticas `MapXxxEndpoints`, e repositórios que seguem o padrão `EnsureTableCreated` + migrations aditivas (`ALTER TABLE ... DEFAULT`) no construtor. Não existe entidade de usuário no domínio (single-user fixo "admin"), mas o tema será vinculado ao token autenticado por uma chave de usuário fixa.

## Goals / Non-Goals

**Goals:**
- Permitir que o usuário autenticado escolha a cor de destaque de uma paleta pré-definida.
- Permitir toggle manual claro/escuro/sistema na tela inicial.
- Persistir a preferência no SQLite e reaplicá-la ao recarregar a aplicação.
- Manter fallback amarelo e modo sistema quando nenhuma preferência estiver salva.
- Não quebrar endpoints nem tabelas existentes.

**Non-Goals:**
- Picker de cor livre (RGB/hex) — apenas paleta pré-definida, para proteger contraste e acessibilidade.
- Temas por página ou por componente; o tema é global.
- Sincronização entre dispositivos além do banco compartilhado da instância.
- Alterar cores de fundo/texto/borda; somente a cor de destaque (`--accent`) é variável.

## Decisions

### Persistência no backend vs. localStorage apenas
Decisão: backend (SQLite) vinculado ao usuário autenticado.
Razão: o usuário pediu explicitamente que o tema seja salvo "no banco" para manter ao reabrir. localStorage sozinho não sobrevive a trocas de navegador/dispositivo e não atende ao requisito.
Alternativa considerada: localStorage apenas — rejeitada porque não persiste entre sessões em ambientes diferentes.

### Modelo de usuário single-user
Decisão: uma linha na tabela `UserThemePreferences` com `userId` fixo (ex.: "admin"), sem entidade `User` nova.
Razão: o domínio já é single-user (JWT com claims fixos "admin"); criar entidade de usuário seria over-engineering para este escopo.
Alternativa considerada: entidade `User` + relacionamento — rejeitada por adicionar complexidade de auth/autorização fora do escopo.

### Local do seletor de tema
Decisão: canto superior da tela inicial (`DashboardView.vue`), ao lado do toggle de escuro/claro.
Razão: atendimento direto ao pedido do usuário; não exige nova página de configurações.
Alternativa considerada: página de configurações dedicada — rejeitada por adicionar navegação extra fora do escopo.

### Paleta pré-definida
Decisão: cores fixas (amarelo Plex, azul, verde, roxo, rosa, índigo).
Razão: garante contraste acessível com fundo claro/escuro sem precisar calcular contraste dinamicamente.
Alternativa considerada: picker livre — rejeitada por risco de combinações com baixo contraste.

### Classes CSS para modo escuro
Decisão: classe `.theme-dark` no elemento raiz (`html`/`body`) controlada pelo frontend, em vez de depender apenas de `@media (prefers-color-scheme)`.
Razão: permite override manual da escolha do SO.
Alternativa considerada: apenas media query — rejeitada porque não há controle do usuário.

## Risks / Trade-offs

- [Contraste de cores da paleta] → Mitigação: cores pré-definidas testadas em claro e escuro; documentar valores seguros.
- [Usuário sem autenticação] → Mitigação: endpoints de tema exigem JWT; sem sessão, o tema não é carregado nem gravado (usa fallback CSS).
- [Múltiplas abas do navegador] → Mitigação: leitura no mount; mudanças em outra aba não são refletidas em tempo real (aceitável para preferência de tema).
- [Schema do SQLite] → Mitigação: seguir padrão de migration aditiva (`ALTER TABLE` com try/catch) usado por `SqliteMarkerRepository`.

## Open Questions

- Nenhuma. Todas as decisões de escopo estão resolvidas na proposta.
