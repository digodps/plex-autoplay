# Tasks

## 1. Remover custom.json do fluxo de autoskip

- [ ] 1.1 Reescrever PlexAutoSkipService para receber IMarkerRepository via DI, consultar GetMarkersForEpisode(ratingKey) a cada mensagem de play do WebSocket e remover customJsonPath, FileSystemWatcher e LoadMarkers do JSON. Verificar: arquivo compila e não faz leitura de arquivo.
- [ ] 1.2 Remover CustomJsonWriterService.cs e a porta ICustomJsonWriter.cs. Verificar: solução compila sem referências pendentes a ICustomJsonWriter.
- [ ] 1.3 Atualizar ServiceCollectionExtensions.cs: retirar registro de ICustomJsonWriter e CUSTOM_JSON_PATH; injetar IMarkerRepository no PlexAutoSkipService. Verificar: `dotnet build` bem-sucedido.

## 2. Remover gravação de custom.json nos endpoints de gravação

- [ ] 2.1 Remover customJsonWriter.UpsertMarkers do POST em MarkerEndpoints.cs. Verificar: POST grava apenas no SQLite.
- [ ] 2.2 Remover customJsonWriter.UpsertMarkers do PUT em MarkerEndpoints.cs. Verificar: PUT atualiza apenas o SQLite; DELETE continua removendo apenas do SQLite.
- [ ] 2.3 Remover customJsonWriter.UpsertMarkers do scan em PlexEndpoints.cs. Verificar: scan grava apenas no SQLite.
- [ ] 2.4 Remover customJsonWriter.UpsertMarkers do SyncBackgroundService.cs. Verificar: sync grava apenas no SQLite.

## 3. Remover custom.json de config e documentação

- [ ] 3.1 Remover referências a custom.json / CUSTOM_JSON_PATH de README.md, package.json, docker-compose.yml e stop.sh. Verificar: documentos não citam mais o arquivo.
- [ ] 3.2 Remover exposição de CUSTOM_JSON_PATH em ConfigEndpoints.cs. Verificar: endpoint de config não expõe mais o caminho do JSON.

## 4. Atualizar testes e verificar build

- [ ] 4.1 Reescrever MarkerEndpointsFixture.cs e MarkerEndpointsTests.cs para SQL-only (sem custom.json temporário e sem asserções sobre o JSON). Verificar: testes de unidade passam.
- [ ] 4.2 Executar build da solução e suíte de testes. Verificar: `dotnet build` e `dotnet test` passam sem falhas.

## Workflow follow-up

- Executar `/opsx-apply` para implementar a mudança a partir destes artefatos de planejamento.
- Arquivar a mudança após conclusão da implementação e revisão do projeto.
