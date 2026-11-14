# Change: Refatorar layout da página de sessões para dispositivos móveis

## Contexto e Motivação
A página de Sessões (`SessionsView.vue`) possui tabelas rígidas (`<table>`) que quebram ou exigem rolagem horizontal em telas de celulares estreitas (ex: iPhone), conforme evidenciado no print fornecido. O objetivo é redesenhar o layout para ser responsivo, exibindo os dados em formato de cards empilhados ou lista adaptativa em telas pequenas, preservando a tabela em telas maiores (desktop).

## Proposta de Design
- Em telas pequenas (`< md` / `sm`), substituir a tabela de "Sessões ativas" por um design de cartões (cards) onde cada sessão é um bloco separado com labels claros (Cliente, Posição / Duração, Próximo skip, Estado).
- Fazer o mesmo para o "Histórico de skips" se necessário, ou garantir que a tabela seja scrollável horizontalmente com indicadores visuais ou transformada em lista de cards em mobile.
- Manter o tema dark/light funcionando perfeitamente.
