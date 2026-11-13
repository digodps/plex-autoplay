# Design: Marcadores no Formato Religioso

## Technical Approach
1. Utilitário `timeFormat.js`:
   - `msToReligious(ms)`: Converte milissegundos para string `HH:MM:SS.cs`.
   - `religiousToMs(str)`: Converte string `HH:MM:SS.cs` (ou subformatos) de volta para milissegundos inteiros.
2. `MarkerRow.vue`:
   - Substitui inputs numéricos por inputs de texto formatados.
   - Aplica `msToReligious` na exibição e `religiousToMs` na persistência.
