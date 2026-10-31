# Tasks

## 1. Backend — modelo e armazenamento de linhas de log

- [x] 1.1 Adicionar lista de `ScanLogEntry` (timestamp, mensagem, nível, sugestão) ao `ScanJob` em `PlexAutoplay.Domain.Ports.IScanJobStore` e verificar que compila
- [x] 1.2 Implementar armazenamento e leitura de linhas de log em `ScanJobStore` e verificar que as linhas são retidas por job
- [x] 1.3 Adicionar helper para gravar uma linha de log com nível e sugestão de correção, e verificar que é exposto pelo store

## 2. Backend — endpoint SSE com linhas de log

- [x] 2.1 Estender `/api/sync/log/stream/{jobId}` em `SyncEndpoints.cs` para enviar `logs` junto aos contadores via SSE e verificar que o cliente recebe o array de linhas
- [x] 2.2 Garantir que o stream fecha quando o job chega a completed/failed/cancelled e verificar que não há vazamento de conexão

## 3. Backend — gravar linhas durante a varredura

- [x] 3.1 Gravar linha de log no início da varredura (job iniciado, bibliotecas encontradas) e verificar que aparece no stream
- [x] 3.2 Gravar linha de log para episódios processados, pulados e erros, com sugestão de correção em erro, e verificar que aparece no stream com timestamp e nível
- [x] 3.3 Inicializar o log do job ao iniciar scan em `PlexEndpoints.cs` e verificar que o job começa com log vazio

## 4. Frontend — console de log no SyncView.vue

- [x] 4.1 Reescrever `SyncView.vue` para renderizar lista de linhas de log (timestamp + mensagem) recebidas via SSE, além dos contadores, e verificar que as linhas aparecem ao conectar
- [x] 4.2 Adicionar ícone por nível ao lado de cada linha (▶ info, ✔ sucesso, ⏭ pular, ⚠ aviso, ✖ erro) e verificar que o ícone corresponde ao nível da linha
- [x] 4.3 Exibir sugestão de ação destacada em linhas de erro e verificar que a sugestão é legível
- [x] 4.4 Adicionar auto-scroll ao chegar nova linha e limite de linhas (ex.: 100), e verificar que o scroll acompanha e a lista não cresce indefinidamente
- [x] 4.5 Adicionar botão "Limpar console" no cabeçalho do painel que esconde as linhas da exibição (apenas frontend; histórico do backend é mantido) e verificar que clicar limpa a lista sem interromper o stream

## 5. Frontend — estilo do console (segue o tema)

- [x] 5.1 Adicionar variáveis/estilos do painel de log em `style.css` (fundo vidro/translúcido com `backdrop-filter: blur()`, fonte monospace, borda sutil) e verificar que o painel segue o tema claro/escuro/sistema
- [x] 5.2 Verificar que o painel se adapta ao toggle de tema da mudança `tema-paleta` quando ambas estão ativas

## 6. Verificação de integração

- [x] 6.1 Iniciar uma varredura na tela inicial, abrir `/sync` e verificar que as linhas de log aparecem em tempo real com ícones e sugestão de erro quando houver falha
- [x] 6.2 Cancelar a varredura e verificar que o stream encerra e o console mostra o estado final
- [x] 6.3 Clicar em "Limpar console" durante uma varredura e verificar que as linhas somem da tela, o stream continua ativo e novas linhas continuam a aparecer

## Workflow follow-up

- Quando a implementação estiver concluída e revisada, executar `/opsx-apply` para aplicar a mudança.
- Após a aplicação, arquivar a mudança com `/opsx-archive` se os requisitos de revisão do projeto forem atendidos.
