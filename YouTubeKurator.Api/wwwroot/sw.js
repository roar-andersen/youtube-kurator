// Service Worker for YouTube Kurator
const CACHE_VERSION = 'v1-app-shell';
const STATIC_CACHE = `${CACHE_VERSION}-static`;
const DYNAMIC_CACHE = `${CACHE_VERSION}-dynamic`;

const STATIC_ASSETS = [
    '/',
    '/index.html',
    '/app.js',
    '/styles.css',
    '/manifest.json',
    '/icon-192.png',
    '/icon-512.png'
];

// ============================================================
// Install Event - Cache static assets
// ============================================================

self.addEventListener('install', event => {
    console.log('Service Worker installing...');
    event.waitUntil(
        caches.open(STATIC_CACHE).then(cache => {
            console.log(`Caching static assets to ${STATIC_CACHE}`);
            return cache.addAll(STATIC_ASSETS);
        }).catch(err => {
            console.error('Failed to cache static assets:', err);
        })
    );
    // Activate immediately instead of waiting for clients to close
    self.skipWaiting();
});

// ============================================================
// Activate Event - Clean up old caches
// ============================================================

self.addEventListener('activate', event => {
    console.log('Service Worker activating...');
    event.waitUntil(
        caches.keys().then(cacheNames => {
            return Promise.all(
                cacheNames
                    .filter(name => name !== STATIC_CACHE && name !== DYNAMIC_CACHE)
                    .map(name => {
                        console.log(`Deleting old cache: ${name}`);
                        return caches.delete(name);
                    })
            );
        })
    );
    // Claim all clients immediately
    self.clients.claim();
});

// ============================================================
// Fetch Event - Network first for API, cache first for assets
// ============================================================

self.addEventListener('fetch', event => {
    const { request } = event;
    const url = new URL(request.url);

    // Skip non-GET requests
    if (request.method !== 'GET') {
        return;
    }

    // API requests: Network first, fallback to cache
    if (url.pathname.startsWith('/api/')) {
        event.respondWith(
            fetch(request)
                .then(response => {
                    // Clone response to cache it
                    if (response.ok) {
                        const cache = caches.open(DYNAMIC_CACHE).then(c => {
                            c.put(request, response.clone());
                            return c;
                        });
                    }
                    return response;
                })
                .catch(() => {
                    // Network failed, try cache
                    return caches.match(request).then(response => {
                        if (response) {
                            return response;
                        }
                        // No cached response for API
                        return new Response(
                            JSON.stringify({ error: 'Kunne ikke koble til. Sjekk internettforbindelsen.' }),
                            { status: 503, headers: { 'Content-Type': 'application/json' } }
                        );
                    });
                })
        );
        return;
    }

    // Static assets: Cache first, fallback to network
    if (request.destination === 'style' ||
        request.destination === 'script' ||
        request.destination === 'image' ||
        request.destination === 'font') {
        event.respondWith(
            caches.match(request).then(response => {
                if (response) {
                    return response;
                }
                return fetch(request).then(response => {
                    // Only cache successful responses
                    if (response.ok) {
                        caches.open(DYNAMIC_CACHE).then(cache => {
                            cache.put(request, response.clone());
                        });
                    }
                    return response;
                }).catch(() => {
                    // Return placeholder if asset not found
                    if (request.destination === 'image') {
                        return new Response(
                            '<svg xmlns="http://www.w3.org/2000/svg" width="100" height="100"><rect fill="#ddd" width="100" height="100"/></svg>',
                            { headers: { 'Content-Type': 'image/svg+xml' } }
                        );
                    }
                    return new Response('Not Found', { status: 404 });
                });
            })
        );
        return;
    }

    // HTML pages: Network first, fallback to cache
    if (request.destination === 'document') {
        event.respondWith(
            fetch(request)
                .then(response => {
                    if (response.ok) {
                        caches.open(DYNAMIC_CACHE).then(cache => {
                            cache.put(request, response.clone());
                        });
                    }
                    return response;
                })
                .catch(() => {
                    return caches.match(request).then(response => {
                        if (response) {
                            return response;
                        }
                        // Offline page
                        return new Response(
                            '<html><body><h1>Appen krever internettforbindelse</h1><p>Vennligst sjekk din tilkobling og prøv igjen.</p></body></html>',
                            { headers: { 'Content-Type': 'text/html' }, status: 503 }
                        );
                    });
                })
        );
        return;
    }

    // Default: Network first
    event.respondWith(
        fetch(request).catch(() => {
            return caches.match(request);
        })
    );
});

// ============================================================
// Message Event - Handle cache updates from client
// ============================================================

self.addEventListener('message', event => {
    if (event.data && event.data.type === 'SKIP_WAITING') {
        self.skipWaiting();
    }
});
