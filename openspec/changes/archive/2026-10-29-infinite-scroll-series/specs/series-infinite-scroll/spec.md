# Spec Delta

## Purpose

Infinite scroll contínuo no catálogo de séries estilo Netflix, com lazy loading de imagens e sem barra de rolagem visível. Permite descoberta contínua de séries sem paginação explícita.

## ADDED Requirements

### Requirement: Infinite scroll contínuo no catálogo
O sistema SHALL exibir séries em um container com rolagem infinita: ao rolar até próximo do fim, carrega automaticamente a próxima página sem ação do usuário, mantendo o estado visual e sem recarregar a página.

#### Scenario: Carregamento automático da próxima página
- **WHEN** usuário rola até 80% da altura do container de séries
- **THEN** sistema carrega a próxima página via API e anexa os resultados ao grid existente

#### Scenario: Indicador de carregamento durante fetch
- **WHEN** próxima página está sendo buscada
- **THEN** exibe skeleton/spinner discreto no final do grid

#### Scenario: Fim da lista
- **WHEN** todas as séries foram carregadas (página final)
- **THEN** para de tentar carregar mais e exibe indicador sutil "Fim do catálogo"

### Requirement: Ocultar barra de rolagem nativa
O sistema SHALL ocultar a barra de rolagem nativa do container de séries, mantendo a rolagem funcional via touch/mouse/wheel, criando experiência visual limpa estilo Netflix.

#### Scenario: Scrollbar oculta mas funcional
- **WHEN** usuário interage com o container de séries
- **THEN** rolagem funciona via wheel/touch/drag mas nenhuma barra de rolagem é visível

#### Scenario: Compatibilidade cross-browser
- **WHEN** acessado em Chrome, Firefox, Safari, Edge
- **THEN** scrollbar oculta em todos (`scrollbar-width: none`, `-ms-overflow-style: none`, `::-webkit-scrollbar { display: none }`)

### Requirement: Lazy loading de imagens de capa
O sistema SHALL carregar imagens de capa das séries apenas quando entram na viewport (IntersectionObserver), reduzindo banda e tempo de carregamento inicial.

#### Scenario: Imagem carrega ao entrar na viewport
- **WHEN** card de série entra na área visível (+50px margem)
- **THEN** imagem começa a carregar (`loading="lazy"` nativo + IntersectionObserver fallback)

#### Scenario: Placeholder durante carregamento
- **WHEN** imagem ainda não carregou
- **THEN** exibe placeholder com cor de fundo e animação shimmer

