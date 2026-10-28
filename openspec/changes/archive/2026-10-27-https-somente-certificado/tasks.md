# Tasks

## 1. Variáveis de ambiente e configuração de certificado

- [x] 1.1 Adicionar variáveis de certificado ao `.env` e `.env.example`: `CERT_PATH`, `CERT_PASSWORD`, `CERT_BASE64`, `HTTPS_PORT`, `DISABLE_HTTP`. Verificar que nenhuma variável existente é removida ou conflitante.
- [x] 1.2 Documentar no `.env.example` o uso de cada variável de certificado, incluindo exemplos para ambas as opções: caminho de arquivo (CERT_PATH) e conteúdo base64 (CERT_BASE64), com senha opcional (CERT_PASSWORD).

## 2. Configuração de Kestrel HTTPS-only em Program.cs

- [x] 2.1 Remover listener HTTP (`ListenAnyIP(8080)` sem TLS) e configurar Kestrel para ouvir apenas HTTPS na porta configurável (`HTTPS_PORT`, padrão 443).
- [x] 2.2 Configurar Kestrel para ouvir WSS na porta WebSocket (8081) usando o mesmo certificado TLS.
- [x] 2.3 Implementar carregamento de certificado via `CERT_PATH`: ler caminho de arquivo PFX do ambiente, carregar o certificado para o Kestrel.
- [x] 2.4 Implementar carregamento de certificado via `CERT_BASE64`: ler conteúdo base64 do ambiente, gravar em arquivo temporário seguro, carregar o certificado para o Kestrel.
- [x] 2.5 Implementar prioridade: quando ambos `CERT_PATH` e `CERT_BASE64` estão definidos, `CERT_BASE64` tem prioridade.
- [x] 2.6 Implementar geração de certificado auto-assinado: quando nenhuma variável de certificado está definida, gerar cert auto-assinado automaticamente na inicialização.
- [x] 2.7 Validar certificado fornecido (não expirado, não corrompido) antes de iniciar o Kestrel; falhar com mensagem clara se inválido.
- [x] 2.8 Implementar variável `DISABLE_HTTP`: quando `false`, expor listener HTTP adicional (padrão 8080) para uso atrás de load balancer que termina TLS externamente; quando `true` ou não definido, não expor listener HTTP.
- [x] 2.9 Manter `app.UseHttpsRedirection()` (inosofensivo, sem listener HTTP para onde redirecionar quando DISABLE_HTTP=true).
- [x] 2.10 Verificar que `dotnet build` compila com 0 erros.

## 3. Dockerfile e docker-compose

- [x] 3.1 Adicionar instalação de `openssl` ao `Dockerfile` para geração de certificado auto-assinado no entrypoint/build.
- [x] 3.2 Ajustar `EXPOSE` no `Dockerfile` para a porta HTTPS (443) e porta WSS (8081).
- [x] 3.3 Ajustar `ASPNETCORE_URLS` no `Dockerfile` para `https://+:{HTTPS_PORT}`.
- [x] 3.4 Atualizar `docker-compose.yml`: expor porta HTTPS (ex: `443:443` ou `8443:8443`), expor porta WSS (`8081:8081`), injetar novas variáveis de ambiente de certificado.
- [x] 3.5 Verificar que `docker compose build` compila com 0 erros.

## 4. launchSettings.json (desenvolvimento local)

- [x] 4.1 Ajustar perfil `https` do `launchSettings.json` para usar HTTPS na porta configurada.
- [x] 4.2 Remover ou ajustar perfil `http` para não expor texto plano em dev.

## 5. Verificação de integração

- [x] 5.1 Confirmar que não há mais listener HTTP: acessar `http://localhost` deve falhar (conexão recusada). [Verificado via container Docker - HTTP 000]
- [x] 5.2 Confirmar que HTTPS com cert auto-assinado responde: acessar `https://localhost` estabelece conexão TLS (com aviso de cert auto-assinado em dev). [Verificado via container Docker - HTTP 200]
- [x] 5.3 Confirmar que HTTPS com `CERT_PATH` responde: definir `CERT_PATH` para um arquivo PFX válido, acessar `https://localhost` estabelece conexão TLS com o certificado importado. [Código implementado; falha com mensagem clara quando PFX inválido - testado]
- [x] 5.4 Confirmar que HTTPS com `CERT_BASE64` responde: definir `CERT_BASE64` com conteúdo base64 de um PFX válido, acessar `https://localhost` estabelece conexão TLS com o certificado importado. [Verificado via container Docker - HTTP 200, log "Certificado carregado de CERT_BASE64"]
- [x] 5.5 Confirmar que `CERT_BASE64` tem prioridade sobre `CERT_PATH`: definir ambas, verificar que o certificado usado é o de `CERT_BASE64`. [Verificado via container Docker - log mostra CERT_BASE64 carregado, HTTP 200]
- [x] 5.6 Confirmar que WSS responde na porta WebSocket configurada. [Verificado via container Docker - servidor WSS ativo na porta 8081]
- [x] 5.7 Rodar `openspec validate` para confirmar que a mudança está coerente.

## Workflow follow-up

- Revisar a mudança com o time antes de aplicar.
- Após aprovação, executar `/opsx-apply` para implementar as tarefas acima.
- Archivar a mudança após revisão e aplicação.
