# Spec: Marcadores no Formato Religioso

## ADDED Requirements

### Requirement 1: Exibição de Marcadores em Formato Religioso (HH:MM:SS.cs)
O sistema deve exibir os marcadores de intro e créditos no formato `HH:MM:SS.cs`.

#### Scenario: Visualizar marcador salvo
- **Given** um marcador cadastrado com posição inicial de 3661050 ms
- **When** o usuário acessa o editor de marcadores
- **Then** o sistema exibe o marcador no formato `01:01:01.05`

### Requirement 2: Edição e Persistência de Marcadores
O editor de marcadores deve permitir inserir e editar o tempo no formato `HH:MM:SS.cs`, convertendo para milissegundos no momento da persistência.

#### Scenario: Salvar marcador editado em formato religioso
- **Given** que o usuário digita `00:01:30.00` no campo de início
- **When** o usuário clica em Salvar
- **Then** o sistema converte para `90000` ms e persiste no backend
