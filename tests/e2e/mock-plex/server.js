const http = require('http');

const PORT = 32400;

const routes = {
  // Plex API: lista de seções de biblioteca
  '/library/sections': {
    method: 'GET',
    body: {
      MediaContainer: {
        Directory: [
          { id: '1', title: 'Séries', type: 'show' },
          { id: '2', title: 'Filmes', type: 'movie' }
        ]
      }
    }
  },
  // Plex API: token de autenticação (simplificado)
  '/': {
    method: 'GET',
    body: 'OK'
  }
};

const server = http.createServer((req, res) => {
  const url = req.url.split('?')[0];
  const route = routes[url];

  if (!route) {
    res.writeHead(404, { 'Content-Type': 'text/plain' });
    res.end('Not found');
    return;
  }

  if (req.method !== route.method) {
    res.writeHead(405, { 'Content-Type': 'text/plain' });
    res.end('Method not allowed');
    return;
  }

  if (route.method === 'GET' && typeof route.body === 'string') {
    res.writeHead(200, { 'Content-Type': 'text/plain' });
    res.end(route.body);
  } else {
    res.writeHead(200, { 'Content-Type': 'application/json' });
    res.end(JSON.stringify(route.body));
  }
});

server.listen(PORT, '0.0.0.0', () => {
  console.log(`Plex mock rodando na porta ${PORT}`);
});
