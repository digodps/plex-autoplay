# Design: Refatoração Mobile da Página de Sessões

## Arquitetura de Componentes UI
- Modificar `SessionsView.vue`.
- Usar classes Tailwind responsivas (`block md:hidden` para versão mobile de cards, `hidden md:block` para versão desktop em tabela).
- **Sessões Ativas (Mobile Card View)**:
  - Card com bordas arredondadas, padding, fundo adequado para dark/light mode.
  - Seção superior: Nome do cliente (`s.clientIdentifier` / `s.platform`) + Badge de Estado (`playing`, etc.).
  - Seção intermediária: Posição / Duração com barra de progresso.
  - Seção inferior: Próximo skip (tipo e tempo estimado).
- **Histórico de Skips (Mobile Card View / Scroll)**:
  - Lista de itens compactos em formato de card ou tabela com overflow-x-auto suave.

## Critérios de Aceitação
1. Em telas mobile, a tabela de sessões ativas renderiza como cards verticais sem quebrar o layout nem exigir scroll horizontal forçado.
2. O tema Light/Dark continua funcional e sem perda de contraste.
3. O build e os testes passam com sucesso.
