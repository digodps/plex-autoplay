const http = require('http');
const fs = require('fs');
const path = require('path');

const PORT = 4173;
const DIST_DIR = '/home/hermes/plex-autoplay-refactor-session-mobile-layout/vue/plex-frontend/dist';

const mimeTypes = {
  '.html': 'text/html',
  '.js': 'application/javascript',
  '.css': 'text/css',
  '.json': 'application/json',
  '.svg': 'image/svg+xml',
  '.png': 'image/png',
  '.ico': 'image/x-icon',
  '.woff': 'font/woff',
  '.woff2': 'font/woff2',
  '.ttf': 'font/ttf',
  '.eot': 'application/vnd.ms-fontobject',
};

const mockApi = {
  '/api/auth/setup/status': { setupRequired: false },
  '/api/auth/login': { token: 'mock-jwt-token' },
  '/api/status': { ok: true },
  '/api/theme': { mode: 'system' },
  '/api/sessions/skip-status': {
    activeSessions: [
      {
        product: 'Plex Web (Chrome)',
        platform: 'Chrome',
        machineIdentifier: 'w3bx8dyoj4qm3nzgfh7bxy',
        ratingKey: 12345,
        state: 'playing',
        positionMs: 1200000,
        durationMs: 5400000,
        nextMarker: { kind: 'intro', msUntilSkip: 30000 }
      },
      {
        product: 'Plex for Windows',
        platform: 'Windows',
        machineIdentifier: 'abc123def456',
        ratingKey: 67890,
        state: 'paused',
        positionMs: 300000,
        durationMs: 3600000,
        nextMarker: { kind: 'credits', msUntilSkip: 120000 }
      }
    ],
    recentSkips: [
      {
        timestamp: '2026-11-14T15:30:00.000Z',
        ratingKey: 111,
        product: 'Plex Web (Chrome)',
        markerKind: 'intro',
        skippedFromMs: 0,
        skippedToMs: 120000
      },
      {
        timestamp: '2026-11-14T15:25:00.000Z',
        ratingKey: 222,
        product: 'Plex for iOS',
        markerKind: 'credits',
        skippedFromMs: 4500000,
        skippedToMs: 4620000
      }
    ]
  },
  '/api/libraries': [],
  '/api/media-roots': [],
};

function serveFile(res, filePath) {
  const ext = path.extname(filePath);
  const contentType = mimeTypes[ext] || 'application/octet-stream';
  fs.readFile(filePath, (err, data) => {
    if (err) {
      res.writeHead(404);
      res.end('Not found');
      return;
    }
    res.writeHead(200, { 'Content-Type': contentType });
    res.end(data);
  });
}

const server = http.createServer((req, res) => {
  // Enable CORS for dev
  res.setHeader('Access-Control-Allow-Origin', '*');
  res.setHeader('Access-Control-Allow-Methods', 'GET, POST, PUT, DELETE, OPTIONS');
  res.setHeader('Access-Control-Allow-Headers', 'Content-Type, Authorization');
  if (req.method === 'OPTIONS') {
    res.writeHead(204);
    res.end();
    return;
  }

  const url = new URL(req.url, `http://localhost:${PORT}`);
  const pathname = url.pathname;

  // Mock API endpoints
  if (pathname.startsWith('/api/')) {
    // Find matching mock
    let mockKey = null;
    for (const key of Object.keys(mockApi)) {
      if (pathname === key || pathname.startsWith(key + '?')) {
        mockKey = key;
        break;
      }
    }
    if (mockKey) {
      res.writeHead(200, { 'Content-Type': 'application/json' });
      res.end(JSON.stringify(mockApi[mockKey]));
      return;
    }
    // Fallback: 404
    res.writeHead(404, { 'Content-Type': 'application/json' });
    res.end(JSON.stringify({ error: 'Not found' }));
    return;
  }

  // Serve static files from dist
  let filePath = path.join(DIST_DIR, pathname === '/' ? 'index.html' : pathname);
  // If file doesn't exist, serve index.html (SPA fallback)
  if (!fs.existsSync(filePath)) {
    filePath = path.join(DIST_DIR, 'index.html');
  }
  serveFile(res, filePath);
});

server.listen(PORT, () => {
  console.log(`Static server running at http://localhost:${PORT}`);
  console.log(`Serving ${DIST_DIR}`);
});