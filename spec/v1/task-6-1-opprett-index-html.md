# Oppgave 6.1: Opprett index.html med basissider

## Fase
Fase 6: Frontend – HTML og struktur

## Avhengigheter
- Oppgave 1.1 (wwwroot-mappen må eksistere)

## Formål
Lag grunnlaget for frontend med HTML-struktur for begge visninger: playlist-oversikt og playlist-detalj.

## Oppgavebeskrivelse

### 1. Opprett index.html
Lag fil `src/YouTubeKurator.Api/wwwroot/index.html`:

```html
<!DOCTYPE html>
<html lang="no">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <meta name="theme-color" content="#1f2937">
    <meta name="description" content="Personlig YouTube-kurator for temabaserte spillelister">

    <!-- PWA Manifest -->
    <link rel="manifest" href="/manifest.json">
    <link rel="icon" type="image/svg+xml" href="/icon.svg">

    <!-- Apple touch icon -->
    <link rel="apple-touch-icon" href="/apple-touch-icon.png">

    <!-- Styles -->
    <link rel="stylesheet" href="/styles.css">

    <title>YouTube Kurator</title>
</head>
<body>
    <div id="app" x-data="appState()" x-init="initApp()">
        <!-- Header -->
        <header class="header">
            <div class="header-content">
                <h1 class="header-title">
                    <a href="#" @click.prevent="showView('list')" class="logo-link">
                        📺 YouTube Kurator
                    </a>
                </h1>
                <nav class="breadcrumb" v-show="currentView === 'detail'">
                    <a href="#" @click.prevent="showView('list')">Spillelister</a>
                    <span class="separator">/</span>
                    <span x-text="currentPlaylist?.name || 'Laster...'"></span>
                </nav>
            </div>
        </header>

        <!-- Main Content -->
        <main class="main-content">
            <!-- Playlist List View -->
            <section id="playlist-list" x-show="currentView === 'list'" class="view">
                <div class="view-header">
                    <h2>Mine Spillelister</h2>
                    <button class="btn btn-primary" @click="showCreateDialog = true">
                        + Ny Spilleliste
                    </button>
                </div>

                <!-- Loading State -->
                <div x-show="isLoading" class="loading-spinner">
                    Laster spillelister...
                </div>

                <!-- Playlists Grid -->
                <div x-show="!isLoading && playlists.length > 0" class="playlists-grid">
                    <template x-for="playlist in playlists" :key="playlist.id">
                        <div class="playlist-card" @click="selectPlaylist(playlist.id)">
                            <div class="playlist-card-header">
                                <h3 x-text="playlist.name"></h3>
                            </div>
                            <div class="playlist-card-body">
                                <p class="search-query" x-text="playlist.searchQuery"></p>
                                <p class="playlist-meta">
                                    <span class="updated-date" x-text="formatDate(playlist.updatedUtc)"></span>
                                </p>
                            </div>
                        </div>
                    </template>
                </div>

                <!-- Empty State -->
                <div x-show="!isLoading && playlists.length === 0" class="empty-state">
                    <p>Ingen spillelister ennå. Opprett en ny for å komme i gang!</p>
                </div>
            </section>

            <!-- Playlist Detail View -->
            <section id="playlist-detail" x-show="currentView === 'detail'" class="view">
                <!-- Playlist Header & Controls -->
                <div class="detail-header">
                    <div class="detail-controls">
                        <div class="edit-form">
                            <label>Navn</label>
                            <input type="text" x-model="currentPlaylist.name" class="input" />

                            <label>Søkeord</label>
                            <input type="text" x-model="currentPlaylist.searchQuery" class="input" />

                            <div class="button-group">
                                <button class="btn btn-secondary" @click="savePlaylist()">
                                    💾 Lagre
                                </button>
                                <button class="btn btn-danger" @click="deletePlaylist()">
                                    🗑️ Slett
                                </button>
                            </div>
                        </div>
                    </div>

                    <button class="btn btn-primary btn-large" @click="refreshPlaylist()">
                        🔄 Oppdater Videoer
                    </button>
                </div>

                <!-- Cache Info -->
                <div x-show="lastRefresh && lastRefresh.fromCache" class="cache-info">
                    ℹ️ Videoene er hentet fra cache og er gyldige til
                    <span x-text="formatDateTime(lastRefresh.cacheExpiresUtc)"></span>
                </div>

                <!-- Error Message -->
                <div x-show="error" class="error-toast" @click="error = null">
                    <span x-text="error"></span>
                    <button class="toast-close">✕</button>
                </div>

                <!-- Success Message -->
                <div x-show="successMessage" class="success-toast" @click="successMessage = null">
                    <span x-text="successMessage"></span>
                    <button class="toast-close">✕</button>
                </div>

                <!-- Videos Loading -->
                <div x-show="isLoadingVideos" class="loading-spinner">
                    Henter videoer...
                </div>

                <!-- Videos Grid -->
                <div x-show="!isLoadingVideos && videos.length > 0" class="videos-container">
                    <div class="pagination-top">
                        <button class="btn btn-small" @click="previousPage()" :disabled="currentPage === 1">
                            ← Forrige
                        </button>
                        <span class="page-info">
                            Side <span x-text="currentPage"></span> av <span x-text="totalPages"></span>
                        </span>
                        <button class="btn btn-small" @click="nextPage()" :disabled="currentPage === totalPages">
                            Neste →
                        </button>
                    </div>

                    <div class="videos-grid">
                        <template x-for="video in paginatedVideos" :key="video.videoId">
                            <div class="video-card" @click="openVideo(video.videoId)" role="button" tabindex="0">
                                <div class="video-thumbnail">
                                    <img :src="video.thumbnailUrl" :alt="video.title" loading="lazy" />
                                    <div class="duration-badge" x-text="formatDuration(video.duration)"></div>
                                </div>
                                <div class="video-info">
                                    <h4 class="video-title" x-text="video.title"></h4>
                                    <p class="video-channel" x-text="video.channelName"></p>
                                    <div class="video-meta">
                                        <span class="views" x-text="formatViews(video.viewCount) + ' views'"></span>
                                        <span class="published-date" x-text="formatDate(video.publishedAt)"></span>
                                    </div>
                                </div>
                            </div>
                        </template>
                    </div>

                    <div class="pagination-bottom">
                        <button class="btn btn-small" @click="previousPage()" :disabled="currentPage === 1">
                            ← Forrige
                        </button>
                        <span class="page-info">
                            Side <span x-text="currentPage"></span> av <span x-text="totalPages"></span>
                        </span>
                        <button class="btn btn-small" @click="nextPage()" :disabled="currentPage === totalPages">
                            Neste →
                        </button>
                    </div>
                </div>

                <!-- Empty Videos -->
                <div x-show="!isLoadingVideos && videos.length === 0 && !error" class="empty-state">
                    <p>Ingen videoer funnet. Prøv å oppdatere eller endre søkeordet.</p>
                </div>
            </section>
        </main>

        <!-- Create Playlist Dialog -->
        <div x-show="showCreateDialog" class="modal-overlay" @click.self="showCreateDialog = false">
            <div class="modal">
                <h2>Opprett ny Spilleliste</h2>
                <div class="modal-body">
                    <label>Navn</label>
                    <input type="text" x-model="newPlaylist.name" class="input" placeholder="f.eks. Musikk" />

                    <label>Søkeord</label>
                    <input type="text" x-model="newPlaylist.searchQuery" class="input" placeholder="f.eks. beste sanger 2025" />
                </div>
                <div class="modal-actions">
                    <button class="btn btn-secondary" @click="showCreateDialog = false">Avbryt</button>
                    <button class="btn btn-primary" @click="createPlaylist()">Opprett</button>
                </div>
            </div>
        </div>
    </div>

    <!-- Scripts -->
    <script defer src="https://cdn.jsdelivr.net/npm/alpinejs@3.x.x/dist/cdn.min.js"></script>
    <script src="/app.js"></script>

    <!-- Service Worker -->
    <script>
        if ('serviceWorker' in navigator) {
            navigator.serviceWorker.register('/sw.js').catch(err => console.log('SW registration failed:', err));
        }
    </script>
</body>
</html>
```

### 2. Opprett placeholder-ikon
Lag fil `src/YouTubeKurator.Api/wwwroot/icon.svg`:

```svg
<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 192 192">
  <rect width="192" height="192" fill="#1f2937" rx="20"/>
  <text x="96" y="120" font-size="80" fill="white" text-anchor="middle" font-family="Arial">📺</text>
</svg>
```

Lag også en placeholder for Apple touch icon. Du kan bruke samme SVG eller en 180x180 PNG:

**For senere**: Opprett `src/YouTubeKurator.Api/wwwroot/apple-touch-icon.png` (180x180 PNG)

### 3. Strukturkrav
HTML-filen skal inneholde:

- [ ] `<!DOCTYPE html>` og proper meta-tags
- [ ] PWA meta-tags (manifest, theme-color, apple-touch-icon)
- [ ] Header med logo og breadcrumb-navigasjon
- [ ] To visninger: `playlist-list` og `playlist-detail`
- [ ] Playlist-oversikt med grid av playlist-kort
- [ ] Knapp for å opprette ny playlist
- [ ] Playlist-detalj med navn/søkeord-editor
- [ ] Refresh-knapp og slett-knapp
- [ ] Video-grid med miniatyrbilder og info
- [ ] Paginering (øverst og nederst)
- [ ] Modal/dialog for å opprette ny playlist
- [ ] Error- og success-toast-meldinger
- [ ] Alpine.js-direktiver for interaktivitet
- [ ] Service worker-registrering

## Akseptansekriterier
- [ ] `index.html` finnes i `wwwroot/`
- [ ] Filen inneholder korrekt HTML5-struktur
- [ ] PWA meta-tags er satt
- [ ] Begge visninger (list og detail) er definert
- [ ] Alpine.js er lenket korrekt
- [ ] `app.js` linkes nederst i dokumentet
- [ ] SVG-ikon finnes
- [ ] Filen kan åpnes i nettleser uten JS-feil (struturen skal være synlig, men uten data)

## Referanser
- [Spesifikasjon: Frontend – Sider/visninger](youtube-kurator-v1-spec.md#syderoversikt)
- [Spesifikasjon: Frontend – PWA](youtube-kurator-v1-spec.md#pwa)
- [Alpine.js Documentation](https://alpinejs.dev/)
