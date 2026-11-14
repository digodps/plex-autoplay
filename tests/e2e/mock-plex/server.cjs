const http = require("http");

const PORT = Number(process.env.PORT) || 32400;

// ---- Dados de mock: séries e episódios ----
const series = [
  {
    type: "show",
    ratingKey: "101",
    title: "Arrow",
    summary: "Um bilionário arma para combater o crime em Star City.",
    thumb: "/library/metadata/101/thumb/169",
    Genre: [{ tag: "Action" }, { tag: "Adventure" }],
  },
  {
    type: "show",
    ratingKey: "110",
    title: "The Flash",
    summary: "O homem mais rápido do mundo.",
    thumb: "/library/metadata/110/thumb/180",
    Genre: [{ tag: "Action" }, { tag: "Adventure" }, { tag: "Superhero" }],
  },
  {
    type: "show",
    ratingKey: "111",
    title: "Daredevil",
    summary: "Advogado cego combate o crime à noite.",
    thumb: "/library/metadata/111/thumb/181",
    Genre: [{ tag: "Action" }, { tag: "Crime" }, { tag: "Superhero" }],
  },
  {
    type: "show",
    ratingKey: "112",
    title: "Jack Ryan",
    summary: "Analista da CIA vira agente de campo.",
    thumb: "/library/metadata/112/thumb/182",
    Genre: [{ tag: "Action" }, { tag: "Thriller" }, { tag: "Political" }],
  },
  {
    type: "show",
    ratingKey: "113",
    title: "24",
    summary: "Agente contra-terrorismo em tempo real.",
    thumb: "/library/metadata/113/thumb/183",
    Genre: [{ tag: "Action" }, { tag: "Thriller" }],
  },
  {
    type: "show",
    ratingKey: "114",
    title: "The Boys",
    summary: "Supers perigosos vs vigilantes.",
    thumb: "/library/metadata/114/thumb/184",
    Genre: [{ tag: "Action" }, { tag: "Dark Comedy" }, { tag: "Superhero" }],
  },
  {
    type: "show",
    ratingKey: "115",
    title: "Reacher",
    summary: "Ex-militar investiga conspirações.",
    thumb: "/library/metadata/115/thumb/185",
    Genre: [{ tag: "Action" }, { tag: "Crime" }, { tag: "Thriller" }],
  },
  {
    type: "show",
    ratingKey: "102",
    title: "Mercy",
    summary: "Drama sobre um hospital.",
    thumb: "/library/metadata/102/thumb/170",
    Genre: [{ tag: "Drama" }, { tag: "Medical" }],
  },
  {
    type: "show",
    ratingKey: "103",
    title: "Attack on Titan",
    summary: "Humanidade luta contra titãs.",
    thumb: "/library/metadata/103/thumb/171",
    Genre: [{ tag: "Animation" }, { tag: "Action" }, { tag: "Dark Fantasy" }],
  },
  {
    type: "show",
    ratingKey: "104",
    title: "My Hero Academia",
    summary: "Academia de heróis em um mundo de quirks.",
    thumb: "/library/metadata/104/thumb/172",
    Genre: [{ tag: "Animation" }, { tag: "Shonen" }, { tag: "Superhero" }],
  },
  {
    type: "show",
    ratingKey: "105",
    title: "Sword Art Online",
    summary: "Jogadores presos em MMORPG mortal.",
    thumb: "/library/metadata/105/thumb/173",
    Genre: [{ tag: "Animation" }, { tag: "Isekai" }, { tag: "Fantasy" }],
  },
  {
    type: "show",
    ratingKey: "106",
    title: "Breaking Bad",
    summary: "Professor de química vira fabricante de metanfetamina.",
    thumb: "/library/metadata/106/thumb/174",
    Genre: [{ tag: "Crime" }, { tag: "Drama" }, { tag: "Thriller" }],
  },
  {
    type: "show",
    ratingKey: "107",
    title: "The Office",
    summary: "Comédia sobre escritório americano.",
    thumb: "/library/metadata/107/thumb/175",
    Genre: [{ tag: "Comedy" }],
  },
  {
    type: "show",
    ratingKey: "108",
    title: "Stranger Things",
    summary: "Crianças enfrentam forças sobrenaturais.",
    thumb: "/library/metadata/108/thumb/176",
    Genre: [{ tag: "Drama" }, { tag: "Fantasy" }, { tag: "Horror" }, { tag: "Sci-Fi" }],
  },
  {
    type: "show",
    ratingKey: "109",
    title: "Untitled Series",
    summary: "Série sem gênero definido.",
    thumb: "/library/metadata/109/thumb/177",
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
  // Episódios de uma seção (?type=4): usado pelo scan de marcadores.
  if (catalogMatch && url.searchParams.get("type") === "4" && req.method === "GET") {
    res.writeHead(200, { "Content-Type": "application/json" });
    res.end(JSON.stringify(episodesResponse("101")));
    return;
  }

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

  // Identidade do servidor (validação de URL/token no setup inicial).
  if (path === "/identity" && req.method === "GET") {
    res.writeHead(200, { "Content-Type": "application/xml" });
    res.end(
      '<?xml version="1.0" encoding="UTF-8"?>\n<MediaContainer machineIdentifier="mock-machine" friendlyName="Plex Mock" version="1.40.0"/>',
    );
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

  // Sessões ativas (usado pelo autoskip). Retorna vazio (sem sessões ativas no mock).
  if (path === "/status/sessions" && req.method === "GET") {
    res.writeHead(200, { "Content-Type": "application/xml" });
    res.end(
      '<?xml version="1.0" encoding="UTF-8"?>\n<MediaContainer size="0"/>',
    );
    return;
  }

  res.writeHead(404, { "Content-Type": "text/plain" });
  res.end("Not found");
});

server.listen(PORT, "0.0.0.0", () => {
  console.log(`Plex mock rodando na porta ${PORT}`);
});