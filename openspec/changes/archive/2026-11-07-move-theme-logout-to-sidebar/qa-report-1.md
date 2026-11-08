# QA Report 1

Resultado: PASS

Data: 2026-11-07
Change: move-theme-logout-to-sidebar
Escopo: verificação independente contra spec.md, design.md, proposal.md e tasks.md; inspeção do código real (não do relatório do implementador).

## Comandos obrigatórios (exit codes)

| # | Comando | Exit | Resultado |
|---|---------|------|-----------|
| 1 | `npm run lint` | 0 | ok (apenas warnings de migração do eslint-plugin-boundaries, sem erros) |
| 2 | `npm run build` | 0 | `✓ built in 1.29s`, 53 módulos transformados |
| 3 | `npm run test:sidebar` | 0 | `# pass 1`, `# fail 0` |
| 4 | `openspec validate move-theme-logout-to-sidebar --strict` | 0 | `Change 'move-theme-logout-to-sidebar' is valid` |

Saída relevante:

```
# test:sidebar
ok 1 - compact mode toggles and persists to localStorage
# tests 1
# pass 1
# fail 0
```

```
# openspec validate --strict
Change 'move-theme-logout-to-sidebar' is valid
```

Docker: não foi executado build de imagem. Justificativa: o change é exclusivamente de frontend (`vue/plex-frontend`), nenhum serviço, Dockerfile ou docker-compose foi alterado. `npm run build` cobre a validação de empacotamento necessária.

## Cenários do spec (spec.md) → implementação

1. **Theme toggle lives in the sidebar** — PASS
   - Sidebar topo: `SidebarArea.vue:50-59` (`@click="theme.toggle"`, `AppIcon` sun/moon, `aria-label`/`title`).
   - Header sem tema: `grep -n "theme\|toggle" HeaderArea.vue` retorna apenas `aria-label="Abrir menu"` / `@click="sidebar.toggleSidebar"` (`HeaderArea.vue:21-22`). Nenhum botão de tema no header.

2. **Logout lives at the sidebar bottom** — PASS
   - Botão "Sair" na base: `SidebarArea.vue:82-91` (`aria-label="Sair"`, `@click="handleLogout"`).
   - Ação: `SidebarArea.vue:26-29` → `auth.logout(); router.push({ name: "login" })`.
   - Limpa sessão: `stores/auth.js:18-21` (`this.token = ""; deleteCookie(AUTH_COOKIE)`).
   - Rota `login` existe: `src/router/index.js:7-8`.

3. **Compact mode hides labels** — PASS
   - Largura: `SidebarArea.vue:35` (`sidebar.isCollapsed ? 'w-16' : 'w-64'`).
   - Rótulos condicionais: `SidebarArea.vue:47,58,65,90` (`v-if="!sidebar.isCollapsed"`); `SidebarItem.vue:28` (`v-if="!collapsed"`).
   - Nome acessível no link: `SidebarItem.vue:23-24` (`:aria-label="collapsed ? label : undefined"`, `:title`).
   - Ícone decorativo oculto de AT: `AppIcon.vue:16` (`aria-hidden="true"`).

4. **Compact mode persists** — PASS
   - Leitura: `stores/sidebar.js:8` (`localStorage.getItem("sidebar.collapsed") === "true"`).
   - Escrita: `stores/sidebar.js:18-21` (`toggleCollapsed` + `localStorage.setItem`).
   - Verificação executável: `tests/sidebar-collapsed.test.mjs:13-24` (toggle, persistência e restauração após novo Pinia).

5. **Compact mode control is accessible** — PASS
   - `SidebarArea.vue:41` `:aria-label="sidebar.isCollapsed ? 'Expandir menu' : 'Recolher menu'"`.
   - `SidebarArea.vue:42` `:aria-expanded="!sidebar.isCollapsed"` (reflete estado expandido).
   - `SidebarArea.vue:43` `:title` equivalente.

## Tasks.md

Todas as 11 tarefas conferem com o código e comandos:
- 1-2 (remover tema/logout do header): confirmado, header só tem toggle mobile.
- 3-4 (tema no topo, logout na base): `SidebarArea.vue:50-59`, `82-91`.
- 5 (`isCollapsed` + localStorage, remover membros mortos): `sidebar.js`; `SidebarItem` não usa mais `useSidebarStore` (diff removido).
- 6 (ícone `menu` no AppIcon): `AppIcon.vue:72-74`.
- 7 (esconder rótulo + aria-label no router-link): `SidebarItem.vue:23-24,28`.
- 8 (`w-64`/`w-16`): `SidebarArea.vue:35`.
- 9 (test:sidebar): passa.
- 10 (lint/build): passam.
- 11 (openspec validate --strict): passa.

Nenhuma tarefa marcada indevidamente.

## Observações (não bloqueiam)

- `proposal.md:24` afirma "item de navegação com `aria-current=\"page\"` já existente preservado". O `SidebarItem.vue` original nunca teve `aria-current` (confirmado via `git diff`), e o spec não exige esse atributo. Não há regressão; apenas a descrição do proposal é imprecisa. Se desejado, adicionar `:aria-current="isActive ? 'page' : undefined"` em `SidebarItem.vue`.
- `design.md` sugeria `aria-label` no SVG; a implementação colocou no `<router-link>` (`SidebarItem.vue:23`), o que atende melhor ao cenário ("accessible name equal to its label").

## Conclusão

Todos os 5 cenários do spec possuem implementação verificável, acessibilidade (aria-label icon-only + aria-expanded) está presente, tasks.md corresponde ao código, e os 4 comandos obrigatórios passam com exit 0.
