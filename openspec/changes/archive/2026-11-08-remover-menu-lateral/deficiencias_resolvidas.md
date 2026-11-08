# Deficiências resolvidas — EGM (A/S/C)

Change: `remover-menu-lateral`.

| # | Achado | Classificação | Resolução |
|---|---|---|---|
| A1 | Breadcrumb dependia de query volátil para rótulos/links | Parcial | `Bibliotecas` e link por `params.key` independem de query; título/query continuam fallback. |
| A2 | Task não exigia ancestral `Bibliotecas` | Aceito | Task 1.4 e design especificam a trilha completa. |
| A3 | `error` do layout perde consumidor e quebra lint | Aceito | Task 1.1 remove `error`; erro de bibliotecas fica em `LibrariesView`. |
| A4 | Testes existentes contradizem a nova trilha | Aceito | Task 1.5 reescreve `breadcrumb.test.mjs` com trilhas e fallback. |
| A5 | Remoção do requisito de Material Tailwind era overreach | Aceito | Requisito mantido em `navegacao-lateral`. |
| A6 | Biblioteca em `/libraries/:key/series` era link e atual ao mesmo tempo | Aceito | Design: biblioteca é atual nessa rota; link só em marcadores. |
| A7 | Purpose de `navegacao-lateral` ficaria obsoleto após archive | Rejeitado | Responsabilidade do `openspec archive`, fora deste apply. |
| A8 | Ícones `library`/`menu` ficariam mortos | Aceito | Task 1.3 remove se sem uso. |
| S1 | Mesma contradição de trilha/link | Aceito | Coberto por A2/A6. |
| S2 | Remoção de contrato global de estilo | Aceito | Coberto por A5. |
| S3 | `error` morto | Aceito | Coberto por A3. |
| S4 | `buildBreadcrumb` concentra rota, lookup e links | Rejeitado | Escopo mínimo; split em modelo de navegação não é necessário para remover o menu. |
| S5 | Contrato de item duplicado entre lib e `AppHeader` | Rejeitado | Contrato existente; mudança exigiria refactor sem ganho para o pedido. |
| S6 | Loading removia ancestral ao filtrar item vazio | Aceito | Task 1.4 garante `Bibliotecas` sempre presente e rótulos genéricos. |
| S7 | `isCurrent` derivado armazenado | Rejeitado | Contrato já consumido por `AppHeader` e testes; manter. |
| S8 | `AppHeader` acumula responsabilidades | Rejeitado | Extrair breadcrumb component é refactor fora de escopo. |
| C1 | Trilhas sem `Bibliotecas` não atendem spec | Aceito | Coberto por A2. |
| C2 | Nenhum teste impede reintroduzir sidebar | Aceito | Task 1.5 adiciona `tests/no-sidebar.test.mjs`. |
| C3 | `error` morto quebra lint | Aceito | Coberto por A3. |
| C4 | `package.json` com chaves `private`/`prettier` duplicadas | Aceito | Task 1.5 limpa duplicatas ao remover `test:sidebar`. |
| C5 | Testes sem novas asserções | Aceito | Coberto por A4. |
| C6 | Remoção de requisito não-lateral | Aceito | Coberto por A5. |
| C7 | Ramos `libraries`/`meta.title`/marcadores sem biblioteca sem teste | Aceito | Task 1.5 exige cobertura desses ramos. |
| C8 | Design prometia fallback que o código não entregava | Aceito | Coberto por S6/A1. |

Nenhum achado alterou o requisito central: remover o menu lateral e manter a top bar com breadcrumb, tema e menu do usuário.
