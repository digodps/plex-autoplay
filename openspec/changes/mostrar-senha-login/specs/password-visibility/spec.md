# Spec Delta — password-visibility

## Purpose

Permitir que o usuário exiba ou oculte a senha digitada em qualquer campo de senha da UI,
reduzindo erros de digitação em login, setup, troca de senha e token do Plex.

## ADDED Requirements

### Requirement: password-visibility-toggle

Todo campo `type="password"` da UI SHALL ter um botão de alternância de visibilidade com
ícone de olho ao lado do input.

#### Scenario: login exibe e oculta senha
- **WHEN** o usuário abre `/login` e clica no ícone ao lado do campo de senha
- **THEN** o campo alterna para `type="text"` (senha visível) e o ícone muda para `eye-off`;
  clicar novamente volta para `type="password"` e ícone `eye`

#### Scenario: setup exibe e oculta senha e token
- **WHEN** o usuário abre `/setup` e clica no ícone ao lado de nova senha, confirmação ou
  token do Plex
- **THEN** o campo alvo alterna entre `type="password"` e `type="text"` e o ícone
  acompanha (`eye`/`eye-off`)

#### Scenario: troca de senha exibe e oculta senhas
- **WHEN** o usuário abre o modal de troca de senha e clica no ícone ao lado de senha atual,
  nova senha ou confirmação
- **THEN** o campo alvo alterna entre `type="password"` e `type="text"` e o ícone
  acompanha (`eye`/`eye-off`)

#### Scenario: ajustes exibe e oculta token do Plex
- **WHEN** o usuário abre Ajustes e clica no ícone ao lado do campo Token do Plex
- **THEN** o campo alterna entre `type="password"` e `type="text"` e o ícone acompanha
  (`eye`/`eye-off`)

#### Scenario: tema light/dark e menus funcionam
- **WHEN** o usuário alterna o tema (light/dark) e navega pelos menus
- **THEN** o ícone de senha herda a cor do tema (`currentColor`) e os menus abrem,
  navegam, fecham e exibem estado ativo

### Requirement: password-visibility-accessibility

O botão de alternância SHALL ser acessível e testável.

#### Scenario: botão acessível e testável
- **WHEN** o botão de alternância é renderizado
- **THEN** ele é um `<button type="button">` com `aria-label` dinâmico ("Mostrar senha" quando
  oculto, "Ocultar senha" quando visível), `aria-pressed` refletindo o estado e um
  `data-testid` único por campo (ex: `toggle-password-<campo>`)

#### Scenario: foco mantido no input
- **WHEN** o usuário clica no botão de alternância
- **THEN** o foco permanece no input de senha correspondente

#### Scenario: campo do token do Plex identificado
- **WHEN** o input Token do Plex em Ajustes é renderizado
- **THEN** ele possui `id="plex-token"` e `data-testid="plex-token"`, permitindo associação
  `label for` e seleção em testes
