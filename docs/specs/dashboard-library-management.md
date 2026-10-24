# Gerenciar bibliotecas pelo dashboard

O dashboard cadastra bibliotecas como subpastas de uma única raiz de mídia montada no container em modo somente leitura. No formulário de biblioteca, navegue pelas pastas disponíveis, selecione uma pasta e classifique-a como `Anime` ou `Série`. A classificação escolhe se o enriquecimento de marcadores Anime pode ser executado; ambas as opções entram no inventário genérico de episódios.

A remoção desativa a biblioteca em varreduras futuras e não apaga nem modifica arquivos. Caminhos absolutos, traversal, arquivos e links simbólicos que apontem para fora da raiz são rejeitados. Para administrar mídias em outro local do host, altere a raiz na implantação do container.
