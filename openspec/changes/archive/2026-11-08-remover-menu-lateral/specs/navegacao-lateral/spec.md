# navegacao-lateral Specification (Delta)

## REMOVED Requirements

### Requirement: Canva-style rail structure

**Reason**: O menu lateral foi removido; a navegação principal passa a ser o breadcrumb da top bar.

**Migration**: Nenhuma ação do usuário. Itens de biblioteca continuam acessíveis pelo card do Dashboard e pelo breadcrumb.

### Requirement: Navigation stays outside the component

**Reason**: Não existe mais componente de navegação lateral; o contrato de props/eventos do rail deixou de existir.

**Migration**: Nenhuma. O breadcrumb é presentacional e recebe `items` do shell.

### Requirement: Rail accessibility and responsivity

**Reason**: Sem rail, não há alvos, foco ou strip horizontal do menu lateral a validar.

**Migration**: Nenhuma. Acessibilidade da top bar e do breadcrumb permanece coberta por `web-ui`.

### Requirement: Sidebar owns theme, logout and compact mode

**Reason**: Tema, logout e modo compacto do menu lateral foram removidos. Tema e logout ficam na top bar.

**Migration**: A preferência `sidebar.collapsed.v2` deixa de ser lida; pode ser descartada pelo navegador.

### Requirement: Sidebar stacking, header removal and active highlight

**Reason**: O shell volta a ter top bar e não tem item lateral com destaque ativo.

**Migration**: Nenhuma.
