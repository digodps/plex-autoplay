# Spec Delta

## ADDED Requirements

### Requirement: Marca da aplicação discreta com fundo transparente
O `AppHeader` SHALL renderizar a marca da aplicação como um traço fino com
fundo transparente, nunca como um bloco sólido preenchido com gradiente.

#### Scenario: Marca é um traço fino com fundo transparente
- **WHEN** o cabeçalho autenticado é renderizado
- **THEN** a marca (`AppLogo`) é um SVG de contorno (traço) sem `rect` de
  fundo preenchido e sem cor de fundo sólida.
