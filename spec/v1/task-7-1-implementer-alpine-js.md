# Oppgave 7.1: Implementer Alpine.js-app for UI-logikk

## Fase
Fase 7: Frontend – JavaScript og Alpine.js

## Avhengigheter
- Oppgave 6.1 (index.html må eksistere)
- Oppgave 6.2 (CSS må eksistere)

## Formål
Implementere all interaktiv logikk for applikasjonen med Alpine.js, inkludert API-kall, state management og brukerinteraksjoner.

## Oppgavebeskrivelse

### 1. Opprett app.js
Lag fil `src/YouTubeKurator.Api/wwwroot/app.js`:

```javascript
// Alpine.js app state and logic
function appState() {
    return {
        // View state
        currentView: 'list', // 'list' or 'detail'
        playlists: [],
        currentPlaylist: null,
        videos: [],
        currentPage: 1,
        videosPerPage: 12,

        // UI state
        isLoading: false,
        isLoadingVideos: false,
        showCreateDialog: false,
        error: null,
        successMessage: null,

        // Cache info
        lastRefresh: null,

        // New playlist form
        newPlaylist: {
            name: '',
            searchQuery: ''
        },

        // Computed properties
        get totalPages() {
            return Math.ceil(this.videos.length / this.videosPerPage);
        },

        get paginatedVideos() {
            const start = (this.currentPage - 1) * this.videosPerPage;
            const end = start + this.videosPerPage;
            return this.videos.slice(start, end);
        },

        // ============================================================
        // Initialization
        // ============================================================

        async initApp() {
            try {
                await this.loadPlaylists();
            } catch (err) {
                console.error('Failed to initialize app:', err);
                this.error = 'Kunne ikke starte applikasjonen. Sjekk internettforbindelsen.';
            }
        },

        // ============================================================
        // View Management
        // ============================================================

        showView(view) {
            this.currentView = view;
            this.error = null;
            this.successMessage = null;
            if (view === 'list') {
                this.currentPlaylist = null;
                this.videos = [];
                this.currentPage = 1;
            }
        },

        // ============================================================
        // Playlist Operations
        // ============================================================

        async loadPlaylists() {
            this.isLoading = true;
            try {
                const response = await fetch('/api/playlists');
                if (!response.ok) {
                    throw new Error('Failed to load playlists');
                }
                this.playlists = await response.json();
                this.error = null;
            } catch (err) {
                console.error('Error loading playlists:', err);
                this.error = 'Kunne ikke laste spillelister. Sjekk internettforbindelsen.';
            } finally {
                this.isLoading = false;
            }
        },

        async createPlaylist() {
            if (!this.newPlaylist.name.trim() || !this.newPlaylist.searchQuery.trim()) {
                this.error = 'Navn og søkeord er påkrevd.';
                return;
            }

            this.isLoading = true;
            try {
                const response = await fetch('/api/playlists', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify(this.newPlaylist)
                });

                if (!response.ok) {
                    throw new Error('Failed to create playlist');
                }

                this.newPlaylist = { name: '', searchQuery: '' };
                this.showCreateDialog = false;
                await this.loadPlaylists();
                this.successMessage = 'Spillelisten ble opprettet!';
                this.error = null;
            } catch (err) {
                console.error('Error creating playlist:', err);
                this.error = 'Kunne ikke opprett spillelisten. Prøv igjen.';
            } finally {
                this.isLoading = false;
            }
        },

        async selectPlaylist(id) {
            try {
                const response = await fetch(`/api/playlists/${id}`);
                if (!response.ok) {
                    throw new Error('Failed to load playlist');
                }
                this.currentPlaylist = await response.json();
                this.currentView = 'detail';
                this.videos = [];
                this.currentPage = 1;
                this.error = null;
            } catch (err) {
                console.error('Error selecting playlist:', err);
                this.error = 'Kunne ikke laste spillelisten.';
            }
        },

        async savePlaylist() {
            if (!this.currentPlaylist.name.trim() || !this.currentPlaylist.searchQuery.trim()) {
                this.error = 'Navn og søkeord er påkrevd.';
                return;
            }

            this.isLoading = true;
            try {
                const response = await fetch(`/api/playlists/${this.currentPlaylist.id}`, {
                    method: 'PUT',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({
                        name: this.currentPlaylist.name,
                        searchQuery: this.currentPlaylist.searchQuery
                    })
                });

                if (!response.ok) {
                    throw new Error('Failed to update playlist');
                }

                await this.loadPlaylists();
                this.successMessage = 'Spillelisten ble lagret!';
                this.error = null;
            } catch (err) {
                console.error('Error saving playlist:', err);
                this.error = 'Kunne ikke lagre spillelisten. Prøv igjen.';
            } finally {
                this.isLoading = false;
            }
        },

        async deletePlaylist() {
            if (!confirm('Er du sikker på at du vil slette denne spillelisten?')) {
                return;
            }

            this.isLoading = true;
            try {
                const response = await fetch(`/api/playlists/${this.currentPlaylist.id}`, {
                    method: 'DELETE'
                });

                if (!response.ok) {
                    throw new Error('Failed to delete playlist');
                }

                await this.loadPlaylists();
                this.showView('list');
                this.successMessage = 'Spillelisten ble slettet!';
            } catch (err) {
                console.error('Error deleting playlist:', err);
                this.error = 'Kunne ikke slette spillelisten. Prøv igjen.';
            } finally {
                this.isLoading = false;
            }
        },

        // ============================================================
        // Video Operations
        // ============================================================

        async refreshPlaylist() {
            if (!this.currentPlaylist) return;

            this.isLoadingVideos = true;
            this.error = null;
            try {
                const response = await fetch(`/api/playlists/${this.currentPlaylist.id}/refresh`, {
                    method: 'POST'
                });

                if (!response.ok) {
                    throw new Error('Failed to refresh playlist');
                }

                const data = await response.json();
                this.videos = data.videos || [];
                this.lastRefresh = {
                    fromCache: data.fromCache,
                    cacheExpiresUtc: data.cacheExpiresUtc
                };

                if (data.error) {
                    this.error = data.error.message;
                }

                this.currentPage = 1;
            } catch (err) {
                console.error('Error refreshing playlist:', err);
                this.error = 'Kunne ikke hente videoer. Sjekk internettforbindelsen.';
            } finally {
                this.isLoadingVideos = false;
            }
        },

        openVideo(videoId) {
            const url = `https://youtu.be/${videoId}`;
            window.open(url, '_blank', 'noopener,noreferrer');
        },

        // ============================================================
        // Pagination
        // ============================================================

        nextPage() {
            if (this.currentPage < this.totalPages) {
                this.currentPage++;
                window.scrollTo({ top: 0, behavior: 'smooth' });
            }
        },

        previousPage() {
            if (this.currentPage > 1) {
                this.currentPage--;
                window.scrollTo({ top: 0, behavior: 'smooth' });
            }
        },

        // ============================================================
        // Formatters
        // ============================================================

        formatDate(dateString) {
            if (!dateString) return '';
            const date = new Date(dateString);
            const today = new Date();
            const yesterday = new Date(today);
            yesterday.setDate(yesterday.getDate() - 1);

            if (this.isSameDay(date, today)) {
                return 'I dag';
            } else if (this.isSameDay(date, yesterday)) {
                return 'I går';
            } else {
                return date.toLocaleDateString('no-NO');
            }
        },

        formatDateTime(dateString) {
            if (!dateString) return '';
            const date = new Date(dateString);
            return date.toLocaleString('no-NO');
        },

        formatDuration(duration) {
            if (!duration) return '0:00';

            // Parse ISO 8601 duration or TimeSpan format
            // Examples: "PT1H2M30S" or "01:02:30"
            let hours = 0, minutes = 0, seconds = 0;

            if (typeof duration === 'string') {
                if (duration.includes('PT')) {
                    // ISO 8601
                    const hourMatch = duration.match(/(\d+)H/);
                    const minuteMatch = duration.match(/(\d+)M/);
                    const secondMatch = duration.match(/(\d+)S/);
                    hours = hourMatch ? parseInt(hourMatch[1]) : 0;
                    minutes = minuteMatch ? parseInt(minuteMatch[1]) : 0;
                    seconds = secondMatch ? parseInt(secondMatch[1]) : 0;
                } else {
                    // TimeSpan format "HH:MM:SS"
                    const parts = duration.split(':').map(Number);
                    if (parts.length === 3) {
                        [hours, minutes, seconds] = parts;
                    } else if (parts.length === 2) {
                        [minutes, seconds] = parts;
                    }
                }
            }

            if (hours > 0) {
                return `${hours}:${String(minutes).padStart(2, '0')}:${String(seconds).padStart(2, '0')}`;
            }
            return `${minutes}:${String(seconds).padStart(2, '0')}`;
        },

        formatViews(count) {
            if (count >= 1000000) {
                return (count / 1000000).toFixed(1) + 'M';
            } else if (count >= 1000) {
                return (count / 1000).toFixed(1) + 'K';
            }
            return count.toString();
        },

        isSameDay(date1, date2) {
            return date1.getFullYear() === date2.getFullYear() &&
                   date1.getMonth() === date2.getMonth() &&
                   date1.getDate() === date2.getDate();
        }
    };
}

// Initialize Alpine.js app (script tag in HTML will call this via x-init)
```

### 2. Verifiser Alpine.js er lastet
I `index.html`, sikre at Alpine.js CDN-lenken er korrekt:

```html
<script defer src="https://cdn.jsdelivr.net/npm/alpinejs@3.x.x/dist/cdn.min.js"></script>
```

### 3. Test applikasjonen
Start `dotnet run` og åpne nettleseren på `http://localhost:5000`. Verifiser at:

- Spillelister lastes når siden åpnes
- Du kan opprett ny spilleliste
- Du kan redigere spilleliste
- Du kan slette spilleliste
- Du kan trykke refresh og få videoer
- Paginering fungerer
- Feilmeldinger vises ved problemer

## Akseptansekriterier
- [ ] `app.js` eksisterer i `wwwroot/`
- [ ] appState()-funksjon eksisterer og returnerer objekt med all state
- [ ] loadPlaylists() henter fra GET /api/playlists
- [ ] createPlaylist() sender POST til /api/playlists
- [ ] selectPlaylist(id) henter enkelt playlist
- [ ] savePlaylist() sender PUT til /api/playlists/{id}
- [ ] deletePlaylist() sender DELETE til /api/playlists/{id}
- [ ] refreshPlaylist() sender POST til /api/playlists/{id}/refresh
- [ ] openVideo(videoId) åpner YouTube-lenke
- [ ] Paginering fungerer (nextPage/previousPage)
- [ ] Alle formatter-funksjoner (date, duration, views) fungerer
- [ ] Error-håndtering fungerer
- [ ] Ingen JavaScript-feil i console

## Referanser
- [Spesifikasjon: Frontend – JavaScript](youtube-kurator-v1-spec.md#8-frontend)
- [Spesifikasjon: API-endepunkter](youtube-kurator-v1-spec.md#7-api-endepunkter)
- [Alpine.js Documentation](https://alpinejs.dev/)
- [Fetch API](https://developer.mozilla.org/en-US/docs/Web/API/Fetch_API)
