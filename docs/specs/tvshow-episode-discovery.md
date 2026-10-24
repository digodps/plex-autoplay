# Descoberta de episódios de TV shows

A descoberta de episódios é genérica e não consulta provedores de Anime. Ela percorre apenas a biblioteca cadastrada e reconhece arquivos de vídeo com estes formatos:

- `S01E02` e faixas como `S01E02-E03`;
- `1x02`;
- `E02` dentro de uma pasta `Season 1` ou `Temporada 1`;
- o padrão legado `<série>/Temporada n/<arquivo>-NN`.
- `<série>/<título> n -NN.ext`, usando `n` como temporada e `NN` como episódio (por exemplo, `<série>/Ao no Hako 2 -01.mp4`); esse formato também pode aparecer dentro de uma pasta de temporada, e o número do arquivo prevalece sobre o da pasta.

Uma faixa associa o mesmo arquivo a cada número de episódio, sem repetir um episódio no inventário. Quando a temporada do nome do arquivo diverge da pasta de temporada, a descoberta conserva a temporada indicada no arquivo e registra a divergência como ambiguidade. Arquivos de vídeo sem padrão reconhecido são listados como não reconhecidos; falhas de leitura ficam separadas e não interrompem as demais pastas.

As chamadas Jikan/AniSkip são um enriquecimento separado, habilitado somente para bibliotecas classificadas como Anime. O inventário e a cobertura de marcadores funcionam para qualquer biblioteca de TV shows.
