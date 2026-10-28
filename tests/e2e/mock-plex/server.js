const http = require("http");

const PORT = 32400;

// ---- Dados de mock: séries e episódios ----
const series = [
  {
    type: "show",
    ratingKey: "101",
    title: "Arrow",
    summary: "Um bilionário arma para combater o crime em Star City.",
    thumb: "/library/metadata/101/thumb/169",
  },
  {
    type: "show",
    ratingKey: "102",
    title: "Mercy",
    summary: "Drama sobre um hospital.",
    thumb: "/library/metadata/102/thumb/170",
  },
];

const episodes = {
  101: [
    {
      ratingKey: "5001",
      title: "Pilot",
      parentIndex: 1,
      index: 1,
      file: "/Volumes/Series/S01E01.mkv",
    },
    {
      ratingKey: "5002",
      title: "Honor Thy Father",
      parentIndex: 1,
      index: 2,
      file: "/Volumes/Series/S01E02.mkv",
    },
  ],
  102: [
    {
      ratingKey: "6001",
      title: "Pilot",
      parentIndex: 1,
      index: 1,
      file: "/Volumes/Series/Mercy S01E01.mkv",
    },
  ],
};

// ---- Rotas ----
function episodesResponse(seriesKey) {
  return {
    MediaContainer: {
      Metadata: (episodes[seriesKey] ?? []).map((ep) => ({
        type: "episode",
        ratingKey: ep.ratingKey,
        title: ep.title,
        parentIndex: ep.parentIndex,
        index: ep.index,
        Media: [{ Part: [{ file: ep.file }] }],
      })),
    },
  };
}

const server = http.createServer((req, res) => {
  const url = new URL(req.url, `http://localhost:${PORT}`);
  const path = url.pathname;

  // Catalogo paginado de séries: /library/sections/{key}/all?type=show
  const catalogMatch = path.match(/^\/library\/sections\/(\d+)\/all$/);
  if (catalogMatch && req.method === "GET") {
    const start = parseInt(req.headers["x-plex-container-start"] ?? "0", 10);
    const size = req.headers["x-plex-container-size"];

    let items = series;
    if (url.searchParams.get("title.contains")) {
      const q = url.searchParams.get("title.contains").toLowerCase();
      items = items.filter((s) => s.title.toLowerCase().includes(q));
    }

    const totalSize = items.length;
    if (size !== undefined) {
      const sizeInt = parseInt(size, 10);
      items = items.slice(start, start + (Number.isNaN(sizeInt) ? 0 : sizeInt));
    }

    res.writeHead(200, { "Content-Type": "application/json" });
    res.end(
      JSON.stringify({
        MediaContainer: { totalSize, Metadata: items },
      }),
    );
    return;
  }

  // Episódios de uma série: /library/metadata/{seriesKey}/allLeaves
  const leavesMatch = path.match(/^\/library\/metadata\/(\d+)\/allLeaves$/);
  if (leavesMatch && req.method === "GET") {
    res.writeHead(200, { "Content-Type": "application/json" });
    res.end(JSON.stringify(episodesResponse(leavesMatch[1])));
    return;
  }

  // Capas (thumb): retorna PNG de 1x1
  const thumbMatch = path.match(/^\/library\/metadata\/(\d+)\/thumb/);
  if (thumbMatch && req.method === "GET") {
    // Pixel transparente PNG de 1x1
    const png = Buffer.from(
      "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNkYPhfDwAChwGA60e6kgAAAABJRU5ErkJggg==",
      "base64",
    );
    res.writeHead(200, { "Content-Type": "image/png", "Content-Length": png.length });
    res.end(png);
    return;
  }

  // Lista de bibliotecas (com key, id e agent)
  if (path === "/library/sections" && req.method === "GET") {
    res.writeHead(200, { "Content-Type": "application/json" });
    res.end(
      JSON.stringify({
        MediaContainer: {
          Directory: [
            { key: "1", id: "1", title: "Séries", type: "show", agent: "tv.plex.agents.series" },
            // Filmes removido para que o mock retorne apenas 1 biblioteca de séries
            // e o front-end faça auto-redirect para /libraries/1/series (esperado pelos testes e2e)
          ],
        },
      }),
    );
    return;
  }

  // Episódios de uma seção (usado pelo scanner). Sem mídia local no mock:
  // o scanner apenas pula os episódios.
  const sectionItemsMatch = path.match(/^\/library\/sections\/(\d+)\/items$/);
  if (sectionItemsMatch && req.method === "GET") {
    res.writeHead(200, { "Content-Type": "application/json" });
    res.end(JSON.stringify({ MediaContainer: { Metadata: [] } }));
    return;
  }

  // Health/raiz
  if (path === "/" && req.method === "GET") {
    res.writeHead(200, { "Content-Type": "text/plain" });
    res.end("OK");
    return;
  }

  res.writeHead(404, { "Content-Type": "text/plain" });
  res.end("Not found");
});

server.listen(PORT, "0.0.0.0", () => {
  console.log(`Plex mock rodando na porta ${PORT}`);
});
