# Proposal: Marcadores no Formato Religioso (HH:MM:SS.cs)

## Why
Usuários necessitam visualizar e editar os marcadores de intro/créditos usando o formato de horas, minutos, segundos e centésimos (HH:MM:SS.cs), facilitando a precisão na marcação temporal.

## What Changes
- Adicionar utilitário de conversão entre milissegundos e o formato HH:MM:SS.cs (`msToReligious` e `religiousToMs`).
- Atualizar o componente `MarkerRow.vue` para exibir o marcador em formato HH:MM:SS.cs e permitir a edição por input de texto nesse mesmo formato, convertendo para milissegundos antes de persistir.

## Impact
- Frontend: `MarkerRow.vue` e utilitário `timeFormat.js`.
- Sem impacto no backend (continua recebendo/enviando milissegundos).
