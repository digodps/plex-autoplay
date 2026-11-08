# QA Report 2

Resultado: FAIL

1. **`npm run lint`** — apenas warnings legados (boundaries/dependencies), sem erros críticos. ✅

2. **`npm run build`** — build concluído com sucesso. ✅

3. **`npm run test:sidebar`** — 1 teste passa (modo compacto persiste no localStorage). ✅

4. **`openspec validate move-theme-logout-to-sidebar --strict`** — passa. ✅

5. **HeaderArea inexistente** — `ls src/components/Header` dá "No such file or directory" e grep não encontrou imports de HeaderArea em nenhum arquivo. ✅

6. **Tema em linha própria abaixo do menu** — SidebarArea.vue usa `flex-col` com gap-1; botão menu primeiro, alternador de tema em linha própria abaixo com label visível quando expandido. ✅

7. **Destaque do item ativo `<router-link>`** — bg-primary text-white aplicado no `<router-link>` (SidebarItem.vue linhas 24-29). Visível no modo compacto. **FAIL** — rota `/series/:seriesKey/markers` não destaca nenhum item da sidebar, violando requisito "exactly one navigation item renders active highlight WHEN any shell route active". O `isActive` computed usa `route.path === props.to || route.path.startsWith(\`${props.to}/\`)`, mas `/series/:seriesKey/markers` não começa com `/libraries/` nem corresponde a nenhum `to` exato.

8. **tasks.md** — todos os itens marcados como concluídos, mas a verificação da rota aninhada `/series/:seriesKey/markers` não foi explicitamente coberta; o critério 7 do especificação continua incompleto para esse caso.

9. **Acessibilidade** — Controles icon-only têm `aria-label`/`aria-expanded`. Foco visível com `focus-ringe`. Contraste do item ativo: light (bg-primary #3C50E0 com texto branco) e dark (bg-gray-900 com texto branco). `aria-current="page"` aplicado exatamente em 1 item quando a rota bate. ⚠️ Falha no ponto 7 acima invalida o requisito "exactly one".

Próximo passo: ajustar `SidebarItem.isActive` para também considerar rotas aninhadas da série (ex: `/series/:seriesKey/markers` deveria destacar "Bibliotecas" como item pai ativo, ou adicionar item "Marcadores" ao menu, ou ajustar a lógica de prefixo.