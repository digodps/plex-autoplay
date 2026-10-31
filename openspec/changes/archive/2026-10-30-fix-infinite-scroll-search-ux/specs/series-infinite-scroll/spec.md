# Spec Delta

## MODIFIED Requirements

### Requirement: Infinite scroll contínuo no catálogo
O sistema SHALL exibir séries em um container com rolagem infinita: ao rolar até próximo do fim, carrega automaticamente a próxima página sem ação do usuário, mantendo o estado visual e sem recarregar a página. **O container observado deve ser o elemento rolável (overflow: auto/scroll) mais próximo da sentinela, não o viewport.**

#### Scenario: Carrega próxima página ao rolar até o fim em container rolável
- **WHEN** usuário rola o container de séries até 200px do fim
- **THEN** sistema dispara carregamento da próxima página via API
- **AND** novas séries são anexadas à lista existente
- **AND** sentinela permanece no fim da lista atualizada

#### Scenario: Não carrega quando não há mais páginas
- **WHEN** usuário rola até o fim e `hasMore` é false
- **THEN** sistema NÃO dispara carregamento adicional
- **AND** indicador "Fim do catálogo" é exibido

#### Scenario: Sentinela detecta interseção no container correto
- **WHEN** sentinela entra na viewport do container rolável (não do viewport da página)
- **THEN** IntersectionObserver dispara callback `onLoadMore`
- **AND** `loadMore` é executado uma única vez por interseção

### Requirement: Ocultar barra de rolagem nativa
O sistema SHALL ocultar a barra de rolagem nativa do container de séries, mantendo a rolagem funcional via touch/mouse/wheel, criando experiência visual limpa estilo Netflix.

#### Scenario: Barra de rolagem oculta mas rolagem funcional
- **WHEN** usuário visualiza o container de séries
- **THEN** barra de rolagem nativa não é visível (scrollbar-width: none, ::-webkit-scrollbar { display: none })
- **AND** rolagem via mouse wheel, touch e teclado funciona normalmente

### Requirement: Lazy loading de imagens de capa
O sistema SHALL carregar imagens de capa das séries apenas quando entram na viewport (IntersectionObserver), reduzindo banda e tempo de carregamento inicial.

#### Scenario: Imagem carrega apenas quando visível
- **WHEN** card de série entra na viewport do container rolável
- **THEN** atributo `loading="lazy"` da imagem é respeitado
- **AND** imagem começa a carregar apenas quando próxima da viewport