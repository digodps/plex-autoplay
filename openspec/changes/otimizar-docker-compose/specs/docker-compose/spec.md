# docker-compose Specification (Delta)

## ADDED Requirements
### Requirement: Apenas variáveis utilizadas
O compose e o arquivo `.env` devem conter exclusivamente as variáveis de ambiente necessárias e consumidas pela aplicação ou pelos serviços Docker.

## MODIFIED Requirements
### Requirement: Validação de start simplificada
O script `start.sh` deve validar apenas as variáveis obrigatórias para o funcionamento inicial da aplicação, ignorando parâmetros opcionais ou legados.
