# Spec — Múltiplos diretórios de mídia na página de Ajustes

## Requirement: Múltiplos diretórios de mídia em /ajustes

### Requirement: Página de Ajustes exibe MediaRootsField

Quando o usuário acessa `/ajustes`, a tela SHALL exibir o componente `MediaRootsField` permitindo adicionar múltiplos diretórios de mídia (com campo de texto + botão "Procurar…" que abre o DirectoryPicker e botão "Remover" para cada item).

#### Scenario: Carrega diretórios salvos

- **WHEN** o usuário acessa `/ajustes` e existem `media_roots` no SQLite
- **THEN** a lista exibe cada diretório salvo como item removível

#### Scenario: Adiciona um novo diretório

- **WHEN** o usuário digita um caminho válido e clica em "Adicionar" (ou confirma no DirectoryPicker)
- **THEN** o diretório aparece na lista de itens do `MediaRootsField`

#### Scenario: Remove um diretório

- **WHEN** o usuário clica em "Remover" em um item da lista
- **THEN** o diretório é removido da lista

### Requirement: Persistência de media_roots no backend

Ao salvar as configurações, a página SHALL enviar `media_roots` (array de strings) via `PUT /api/settings`. O backend SHALL normalizar a lista (ao menos 1, máximo 32, existente, não é `/`, deduplicado) e persistir em `MEDIA_ROOTS`.

#### Scenario: Salva múltiplos diretórios

- **WHEN** o usuário salva a página com N diretórios válidos na lista
- **THEN** `PUT /api/settings` recebe `media_roots` com N itens e retorna `200`

#### Scenario: Lista vazia é rejeitada

- **WHEN** o usuário tenta salvar com a lista de diretórios vazia
- **THEN** o backend retorna `400` com erro prefixado `media_roots`
