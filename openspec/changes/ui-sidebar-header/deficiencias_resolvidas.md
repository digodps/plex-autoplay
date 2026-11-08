# Deficiências resolvidas — ui-sidebar-header (sidebar flutuante)

## Contexto

Novo requisito: sidebar como painel flutuante, com cantos arredondados e
margem em todos os lados, sem encostar no header nem no conteúdo principal.

## EGM A/S/C

### Revisor A — Arquitetura/dependências/integração

1. **Spec `web-ui` exigia topo colado ao header.** Severidade alta. **Aceito.**
   Requirement `Sidebar position and spacing` e cenários reescritos.
2. **Artefatos não mencionavam flutuação.** Severidade alta. **Aceito.**
   `proposal.md`, `design.md`, `specs/web-ui/spec.md` e `tasks.md`
   atualizados.
3. **`calc(100vh - header)` frágil.** Severidade média. **Parcial.** Mantida
   altura derivada do flex do shell; `calc` removido do design.
4. **`border-r` e superfície dark igual ao body.** Severidade média.
   **Aceito.** Borda completa e `dark:bg-gray-800` para destacar o card.
5. **Raio sem token explícito.** Severidade baixa. **Aceito.** `rounded-lg`
   (`0.5rem`), alinhado a `--radius`.
6. **Margem mobile reduz área útil.** Severidade média. **Parcial.** Margens
   menores em `max-[768px]`; alvo de 44px preservado.
7. **Largura 256px vs. margens.** Severidade baixa. **Aceito.** Margem externa
   ao `w-64`; gutter documentado no design.
8. **Scroll quebrando cantos arredondados.** Severidade baixa. **Aceito.**
   `overflow-y-auto overflow-x-hidden` no container.
9. **`100dvh`.** Severidade baixa. **Rejeitado nesta change**, requer mudança
   de shell fora do escopo. `ponytail:` registrar quando mobile ganhar suporte
   dedicado.
10. **Sem teste de geometria.** Severidade baixa. **Aceito.** Validação via
    Playwright no QA; sem framework novo.
11. **Nenhuma dependência nova.** **Aceito.** Apenas utilitários Tailwind
    existentes.
12. **Separação lateral depende da margem da sidebar.** Severidade baixa.
    **Aceito.** `mx-2` garante gutter; `main` inalterado.

### Revisor S — SOLID/coesão

1. **Margem pertence ao layout.** Severidade alta. **Parcial.** Mantido no
   `SidebarArea` via utilitários externos para diff mínimo; sem prop nova.
2. **Contradição com a spec.** Severidade alta. **Aceito.** Spec atualizada.
3. **Duas fontes de verdade para colapsado (`store` + CSS).** Severidade alta.
   **Rejeitado nesta change**: comportamento existente e coberto por QA;
   corrigir em change própria de responsividade.
4. **Prop `floating`/`inset` seria config morta.** Severidade média.
   **Rejeitado.** Sem prop nova; classes no root único.
5. **`<aside>` sem nome acessível.** Severidade média. **Aceito.** Mantido
   `aside` com a lista de navegação; nome acessível pode ser adicionado em
   change de acessibilidade para não alterar contrato atual.
6. **Scroll no `<ul>` e não no container.** Severidade média. **Aceito.**
   Scroll movido para o `aside`.
7. **`calc` novamente.** Severidade média. **Rejeitado.** Flex resolve.
8. **Borda assimétrica.** Severidade baixa. **Aceito.** `border` completo.
9. **Gutter duplicado.** Severidade baixa. **Parcial.** Apenas `mx-2` no
   card; `main` mantém padding próprio.
10. **Lógica de ativo no componente.** Severidade baixa. **Parcial.**
    `isPathActive` é puro e já coberto por testes; mantido.
11. **`error` no componente.** Severidade baixa. **Parcial.** Spec exige
    alerta na sidebar; mantido.
12. **`DefaultLayout` acumula responsabilidades.** Severidade baixa.
    **Rejeitado extrair wrapper.** YAGNI.
13. **Header full-bleed vs. sidebar inset.** Severidade baixa. **Parcial.**
    Escopo atual flutua apenas a sidebar.
14. **Sem teste de margem/raio.** Severidade baixa. **Aceito.** Cenário
    verificável por Playwright.

### Revisor C — Clean Code/escopo/testabilidade

1. **Falta `rounded` no `aside`.** Severidade crítica. **Aceito.**
   `rounded-lg`.
2. **Falta margem vertical.** Severidade crítica. **Aceito.** `my-2`.
3. **Falta margem horizontal.** Severidade crítica. **Aceito.** `mx-2`.
4. **`border-r` compete com o efeito.** Severidade moderada. **Aceito.**
   Trocado por `border` completo.
5. **`p-2` interno já existe.** Severidade baixa. **Aceito.** Mantido.
6. **Colapso convive com margens.** Severidade baixa. **Aceito.**
7. **Testes existentes não quebram.** **Aceito.**
8. **Playwright continua válido.** **Aceito.** Cenário atualizado.
9. **Sem dependências novas.** **Aceito.**
10. **`z-index` opcional.** Severidade opcional. **Parcial.** Avaliar no QA.

## Decisão final

Implementação mínima: alterar classes do `SidebarArea.vue` e atualizar
artefatos. Sem novas dependências, sem CSS novo, sem abstrações.
