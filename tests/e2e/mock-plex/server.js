const http = require("http");

const PORT = Number(process.env.PORT) || 32400;

<<<<<<< HEAD
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
=======
// ---- Gera 60 séries de mock para testar infinite scroll (3 páginas de 24) ----
function generateSeries(count = 60) {
  const series = [];
  const genres = [
    "Action",
    "Drama",
    "Comedy",
    "Sci-Fi",
    "Thriller",
    "Fantasy",
    "Horror",
    "Mystery",
  ];
  const templates = [
    "Uma história épica sobre {topic}",
    "A jornada de {topic} através de mundos desconhecidos",
    "Quando {topic} muda tudo",
    "Os segredos de {topic} revelados",
    "Uma aventura de {topic} sem precedentes",
  ];
  const topics = [
    "heróis improváveis",
    "viagens no tempo",
    "inteligência artificial",
    "mundos paralelos",
    "sociedades distópicas",
    "mistérios antigos",
    "poderes extraordinários",
    "conspirações globais",
  ];

  for (let i = 1; i <= count; i++) {
    const genre = genres[i % genres.length];
    const template = templates[i % templates.length];
    const topic = topics[i % topics.length];
    series.push({
      type: "show",
      ratingKey: String(1000 + i),
      title: `Série ${i}: ${genre} ${i}`,
      summary: template.replace("{topic}", topic),
      thumb: `/library/metadata/${1000 + i}/thumb/${1000 + i}`,
    });
  }
  return series;
}

const series = generateSeries(60);

// Episódios simples para cada série
const episodes = {};
series.forEach((s) => {
  const epCount = (s.ratingKey % 5) + 1; // 1-5 episódios por série
  episodes[s.ratingKey] = Array.from({ length: epCount }, (_, idx) => ({
    ratingKey: String(parseInt(s.ratingKey) * 100 + idx + 1),
    title: `Episódio ${idx + 1}`,
    parentIndex: 1,
    index: idx + 1,
    file: `/media/${s.ratingKey}/S01E${String(idx + 1).padStart(2, "0")}.mkv`,
  }));
});

// ---- Rotas ----
function episodesResponse(list) {
  return {
    MediaContainer: {
      Metadata: list.map((ep) => ({
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
>>>>>>> origin/feature/dashboard-redesign-31-10-2026-13-29-43-856

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
<<<<<<< HEAD
<<<<<<< HEAD
  // Episódios de uma seção (?type=4): usado pelo scan de marcadores.
  if (catalogMatch && url.searchParams.get("type") === "4" && req.method === "GET") {
    res.writeHead(200, { "Content-Type": "application/json" });
    res.end(JSON.stringify(episodesResponse("101")));
    return;
  }

=======
  if (catalogMatch && req.method === "GET" && url.searchParams.get("type") === "4") {
    // Episódios da seção (usado pelo scanner): /library/sections/{key}/all?type=4
    res.writeHead(200, { "Content-Type": "application/json" });
    res.end(JSON.stringify(episodesResponse(Object.values(episodes).flat())));
    return;
  }
>>>>>>> origin/feature/fix-plex-episodes-endpoint-31-10-2026-19-14-11
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
    res.end(JSON.stringify(episodesResponse(episodes[leavesMatch[1]] ?? [])));
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

  // Identidade do servidor (validação de URL/token no setup inicial).
  if (path === "/identity" && req.method === "GET") {
    res.writeHead(200, { "Content-Type": "application/xml" });
    res.end(
      '<?xml version="1.0" encoding="UTF-8"?>\n<MediaContainer machineIdentifier="mock-machine" friendlyName="Plex Mock" version="1.40.0"/>',
    );
    return;
  }

=======
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

>>>>>>> origin/feature/dashboard-redesign-31-10-2026-13-29-43-856
  // Lista de bibliotecas (com key, id e agent)
  if (path === "/library/sections" && req.method === "GET") {
    res.writeHead(200, { "Content-Type": "application/json" });
    res.end(
      JSON.stringify({
        MediaContainer: {
          Directory: [
            { key: "1", id: "1", title: "Séries", type: "show", agent: "tv.plex.agents.series" },
<<<<<<< HEAD
            // Filmes removido para que o mock retorne apenas 1 biblioteca de séries
            // e o front-end faça auto-redirect para /libraries/1/series (esperado pelos testes e2e)
=======
>>>>>>> origin/feature/dashboard-redesign-31-10-2026-13-29-43-856
          ],
        },
      }),
    );
    return;
  }

  // Episódios de uma seção (usado pelo scanner). Sem mídia local no mock:
<<<<<<< HEAD
  // o scanner apenas pula os episódios.
=======
>>>>>>> origin/feature/dashboard-redesign-31-10-2026-13-29-43-856
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
<<<<<<< HEAD
  console.log(`Plex mock rodando na porta ${PORT}`);
});
=======
  console.log(`Plex mock rodando na porta ${PORT} com ${series.length} séries`);
});
>>>>>>> origin/feature/dashboard-redesign-31-10-2026-13-29-43-856
