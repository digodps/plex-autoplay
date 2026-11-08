# media-roots Specification

## Purpose

Define a lista de raízes de mídia configurável pelo operador, sua validação/persistência e o navegador de diretórios do servidor usado para escolhê-las.

## ADDED Requirements

### Requirement: Lista de diretórios de mídia

O sistema SHALL persistir uma lista ordenada de diretórios de mídia (`MEDIA_ROOTS`), aceitando inclusão e remoção de itens quantas vezes o operador precisar, com limite máximo de 32 itens.

#### Scenario: Vários diretórios salvos
- **WHEN** o operador salva `["/media/Animes","/mnt/nas/Series","/media/Filmes"]`
- **THEN** o sistema persiste e devolve exatamente esses três caminhos, na ordem informada

#### Scenario: Caminho duplicado
- **WHEN** a lista enviada contém o mesmo caminho mais de uma vez, inclusive com barra final diferente
- **THEN** o sistema persiste apenas a primeira ocorrência após canonicalização

#### Scenario: Caminho inválido na lista
- **WHEN** algum item é relativo, é `/` ou não existe em disco
- **THEN** o sistema responde `400` com `errors` contendo `media_roots: <caminho> não é um diretório absoluto existente` e não persiste nada

#### Scenario: Lista vazia
- **WHEN** o operador envia lista vazia em `PUT /api/settings`
- **THEN** o sistema responde `400` exigindo ao menos um diretório

#### Scenario: Lista acima do limite
- **WHEN** a lista enviada tem 33 itens
- **THEN** o sistema responde `400` com erro `media_roots: máximo de 32 diretórios` e não persiste nada

#### Scenario: Campo omitido preserva valor
- **WHEN** `PUT /api/settings` não envia `media_roots`
- **THEN** o sistema mantém a lista já persistida

#### Scenario: Contrato JSON snake_case
- **WHEN** a UI envia ou recebe configurações
- **THEN** o campo se chama `media_roots` em requisição e resposta

### Requirement: Navegador de diretórios do servidor

O sistema SHALL oferecer `GET /api/filesystem/directories` para listar subdiretórios do servidor, permitindo que a UI selecione um caminho existente sem digitação manual.

#### Scenario: Listar subdiretórios
- **WHEN** o operador requisita `path=/media`
- **THEN** o sistema retorna `path` absoluto, `parent` e `entries` com `name` e `path` absoluto de cada subdiretório, ordenados por nome (`OrdinalIgnoreCase`), sem arquivos, sem entradas ocultas (`.*`) e com no máximo 2000 itens

#### Scenario: Caminho raiz
- **WHEN** o operador requisita sem `path` ou com `path=/`
- **THEN** o sistema retorna `path: "/"`, `parent: null` e as entradas de diretório do sistema de arquivos raiz

#### Scenario: Navegação para o pai
- **WHEN** o operador está em `/media/Animes`
- **THEN** o sistema retorna `parent: "/media"`

#### Scenario: Caminho relativo
- **WHEN** `path` não é absoluto
- **THEN** o sistema responde `400` e não lista nada

#### Scenario: Diretório inexistente
- **WHEN** `path` não existe
- **THEN** o sistema responde `404` com mensagem de diretório não encontrado

#### Scenario: Acesso sem sessão após setup concluído
- **WHEN** a requisição não possui sessão autenticada e o setup já foi concluído
- **THEN** o sistema responde `401` e não lista nada

#### Scenario: Acesso durante o setup inicial
- **WHEN** ainda não existe credencial de operador
- **THEN** o sistema permite listar diretórios para o wizard de primeiro acesso, mesmo sem sessão

### Requirement: Seleção de diretórios na interface

O sistema SHALL oferecer, no primeiro acesso e na página de Ajustes, um campo de lista de diretórios com ações de adicionar, remover e "Procurar…" que abre o navegador de diretórios do servidor.

#### Scenario: Adicionar diretório pelo navegador
- **WHEN** o operador navega até `/media/Animes` e confirma "Selecionar este diretório"
- **THEN** o caminho é adicionado à lista e enviado no próximo salvamento

#### Scenario: Remover diretório
- **WHEN** o operador remove um item da lista
- **THEN** o item deixa de ser enviado e deixa de ser usado no processamento após salvar

#### Scenario: Tema e interação do modal
- **WHEN** o modal abre em tema claro ou escuro
- **THEN** cores de fundo, texto e borda seguem o tema ativo e o modal fecha por "Cancelar", tecla Esc ou clique no backdrop
