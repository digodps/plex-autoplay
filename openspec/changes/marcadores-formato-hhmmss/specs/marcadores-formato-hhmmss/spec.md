## ADDED Requirements

### Requirement: Exibição e Edição de Marcadores em Formato Religioso
The system SHALL allow viewing and editing markers using the `HH:MM:SS.cs` format.

#### Scenario: Visualizar e editar no formato HH:MM:SS.cs
- **Given** um marcador cadastrado
- **When** o usuário visualiza ou edita o marcador
- **Then** o formato utilizado é `HH:MM:SS.cs` e é convertido corretamente para milissegundos ao salvar.
