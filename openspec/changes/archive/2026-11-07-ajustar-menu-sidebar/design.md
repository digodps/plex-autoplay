# Design: ajustar-menu-sidebar

## Decisão

`SidebarArea.vue` passa a declarar `label` em cada item. `SidebarItem.vue` aceita `label` opcional (default `""`) e renderiza `label || to`, preservando compatibilidade com chamadas antigas. `AppIcon` recebe `class="m-1"` para espaçamento.

## Riscos

- Nenhum risco de contrato de API ou rota.
- Fallback para `to` evita quebra caso um item futuro não defina `label`.
