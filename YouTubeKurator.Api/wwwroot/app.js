// Authentication check - redirect to login if not authenticated
window.addEventListener('load', () => {
    const authService = new AuthService();
    if (!authService.isLoggedIn()) {
        window.location.href = '/login.html';
    }
});

// Error message catalog
const ERROR_MESSAGES = {
    NETWORK_ERROR: 'Kunne ikke koble til. Sjekk internettforbindelsen.',
    QUOTA_EXCEEDED: 'YouTube-grensen er nådd for i dag. Du kan fortsatt se lagrede videoer. Prøv å oppdatere i morgen.',
    GENERIC_ERROR: 'Noe gikk galt. Prøv igjen senere.',
    OFFLINE: 'Appen krever internettforbindelse. Sjekk forbindelsen din.',
    PLAYLIST_NOT_FOUND: 'Spillelisten ble ikke funnet.',
    INVALID_INPUT: 'Navn og søkeord er påkrevd.',
    CREATE_FAILED: 'Kunne ikke opprett spillelisten. Prøv igjen.',
    UPDATE_FAILED: 'Kunne ikke lagre endringene. Prøv igjen.',
    DELETE_FAILED: 'Kunne ikke slette spillelisten. Prøv igjen.',
    LOAD_VIDEOS_FAILED: 'Kunne ikke hente videoer. Sjekk internettforbindelsen.',
    LOAD_PLAYLISTS_FAILED: 'Kunne ikke laste spillelister. Sjekk internettforbindelsen.',
    INIT_FAILED: 'Kunne ikke starte applikasjonen. Sjekk internettforbindelsen.'
};

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

        // Sorting
        sortBy: 'relevance',

        // New playlist form
        newPlaylist: {
            name: '',
            searchQuery: ''
        },

        // Filter Editor State
        showFilterEditor: false,
        filterValidationError: null,
        filterSaveMessage: null,
        currentFilterTab: 'themes',

        // Filter Data Structure
        filters: {
            themes: [],
            keywords: {
                include: [],
                exclude: []
            },
            duration: {
                min: 0,
                max: 10800 // 3 hours
            },
            publishedTime: {
                type: 'relative', // 'relative' or 'absolute'
                days: 30,
                fromDate: null,
                toDate: null
            },
            language: {
                preferred: 'en',
                region: 'GB'
            },
            contentType: {
                videos: true,
                livestreams: false,
                shorts: false
            },
            popularity: {
                minViews: 0,
                minLikes: 0,
                minLikeRatio: 0
            },
            channels: {
                include: [],
                exclude: []
            }
        },

        // UI input buffers for filter editor
        themeInput: '',
        includeKeywordInput: '',
        excludeKeywordInput: '',
        channelInput: '',
        channelExcludeInput: '',

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
        // Toast Messages
        // ============================================================

        showError(message) {
            this.error = message;
            // Auto-dismiss after 5 seconds
            setTimeout(() => {
                this.error = null;
            }, 5000);
        },

        showSuccess(message) {
            this.successMessage = message;
            // Auto-dismiss after 3 seconds
            setTimeout(() => {
                this.successMessage = null;
            }, 3000);
        },

        // ============================================================
        // Initialization
        // ============================================================

        async initApp() {
            try {
                await this.loadPlaylists();

                // Restore last playlist if available
                const lastPlaylistId = localStorage.getItem('lastPlaylistId');
                if (lastPlaylistId) {
                    await this.selectPlaylist(lastPlaylistId);
                }
            } catch (err) {
                console.error('Failed to initialize app:', err);
                this.showError(ERROR_MESSAGES.INIT_FAILED);
            }
        },

        logout() {
            const authService = new AuthService();
            authService.logout();
            window.location.href = '/login.html';
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
                // Clear localStorage when returning to list
                localStorage.removeItem('lastPlaylistId');
            }
        },

        // ============================================================
        // Playlist Operations
        // ============================================================

        async loadPlaylists() {
            this.isLoading = true;
            this.error = null;
            try {
                const response = await httpClient.get('/api/playlists');
                if (!response.ok) {
                    throw new Error('Failed to load playlists');
                }
                this.playlists = await response.json();
            } catch (err) {
                console.error('Error loading playlists:', err);
                this.showError(ERROR_MESSAGES.LOAD_PLAYLISTS_FAILED);
            } finally {
                this.isLoading = false;
            }
        },

        async createPlaylist() {
            if (!this.newPlaylist.name.trim() || !this.newPlaylist.searchQuery.trim()) {
                this.showError(ERROR_MESSAGES.INVALID_INPUT);
                return;
            }

            this.isLoading = true;
            this.error = null;
            try {
                const response = await httpClient.post('/api/playlists', this.newPlaylist);

                if (!response.ok) {
                    throw new Error('Failed to create playlist');
                }

                this.newPlaylist = { name: '', searchQuery: '' };
                this.showCreateDialog = false;
                await this.loadPlaylists();
                this.showSuccess('Spillelisten ble opprettet!');
            } catch (err) {
                console.error('Error creating playlist:', err);
                this.showError(ERROR_MESSAGES.CREATE_FAILED);
            } finally {
                this.isLoading = false;
            }
        },

        async selectPlaylist(id) {
            this.error = null;
            try {
                const response = await httpClient.get(`/api/playlists/${id}`);
                if (!response.ok) {
                    throw new Error('Failed to load playlist');
                }
                this.currentPlaylist = await response.json();
                this.currentView = 'detail';
                this.videos = [];
                this.currentPage = 1;
                this.sortBy = 'relevance'; // Reset sort order when selecting a new playlist

                // Save to localStorage for session persistence
                localStorage.setItem('lastPlaylistId', id);
            } catch (err) {
                console.error('Error selecting playlist:', err);
                this.showError(ERROR_MESSAGES.PLAYLIST_NOT_FOUND);
            }
        },

        async savePlaylist() {
            if (!this.currentPlaylist.name.trim() || !this.currentPlaylist.searchQuery.trim()) {
                this.showError(ERROR_MESSAGES.INVALID_INPUT);
                return;
            }

            this.isLoading = true;
            this.error = null;
            try {
                const response = await httpClient.put(`/api/playlists/${this.currentPlaylist.id}`, {
                    name: this.currentPlaylist.name,
                    searchQuery: this.currentPlaylist.searchQuery,
                    enableDiscovery: this.currentPlaylist.enableDiscovery || false
                });

                if (!response.ok) {
                    throw new Error('Failed to update playlist');
                }

                await this.loadPlaylists();
                this.showSuccess('Spillelisten ble lagret!');
            } catch (err) {
                console.error('Error saving playlist:', err);
                this.showError(ERROR_MESSAGES.UPDATE_FAILED);
            } finally {
                this.isLoading = false;
            }
        },

        async deletePlaylist() {
            if (!confirm('Er du sikker på at du vil slette denne spillelisten?')) {
                return;
            }

            this.isLoading = true;
            this.error = null;
            try {
                const response = await httpClient.delete(`/api/playlists/${this.currentPlaylist.id}`);

                if (!response.ok) {
                    throw new Error('Failed to delete playlist');
                }

                await this.loadPlaylists();
                this.showView('list');
                this.showSuccess('Spillelisten ble slettet!');
            } catch (err) {
                console.error('Error deleting playlist:', err);
                this.showError(ERROR_MESSAGES.DELETE_FAILED);
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
                const response = await httpClient.post(`/api/playlists/${this.currentPlaylist.id}/refresh`, {
                    sortBy: this.sortBy
                });

                if (!response.ok) {
                    this.showError(ERROR_MESSAGES.LOAD_VIDEOS_FAILED);
                    throw new Error('Failed to refresh playlist');
                }

                const data = await response.json();
                this.videos = data.videos || [];
                this.lastRefresh = {
                    fromCache: data.fromCache,
                    cacheExpiresUtc: data.cacheExpiresUtc
                };

                // Handle backend error object (e.g., quota exceeded)
                if (data.error) {
                    this.showError(data.error.message || ERROR_MESSAGES.GENERIC_ERROR);
                }

                this.currentPage = 1;
            } catch (err) {
                console.error('Error refreshing playlist:', err);
                if (!this.error) {
                    this.showError(ERROR_MESSAGES.LOAD_VIDEOS_FAILED);
                }
            } finally {
                this.isLoadingVideos = false;
            }
        },

        openVideo(videoId) {
            const url = `https://youtu.be/${videoId}`;
            window.open(url, '_blank', 'noopener,noreferrer');
        },

        async toggleWatchStatus(videoId) {
            try {
                // Find the video in the current list
                const video = this.videos.find(v => v.videoId === videoId);
                if (!video || !this.currentPlaylist) return;

                // Toggle the watch status locally
                video.isWatched = !video.isWatched;

                // Send update to backend
                const response = await httpClient.post(`/api/videos/${videoId}/status`, {
                    playlistId: this.currentPlaylist.id,
                    status: video.isWatched ? 'Seen' : 'New'
                });

                if (!response.ok) {
                    // Revert on error
                    video.isWatched = !video.isWatched;
                    console.error('Failed to update watch status');
                }
            } catch (err) {
                console.error('Error updating watch status:', err);
            }
        },

        async toggleWatchLater(videoId) {
            try {
                // Find the video in the current list
                const video = this.videos.find(v => v.videoId === videoId);
                if (!video || !this.currentPlaylist) return;

                // Toggle the watch later status locally
                const wasInWatchLater = video.isInWatchLater;
                video.isInWatchLater = !video.isInWatchLater;

                // Send update to backend
                let response;
                if (video.isInWatchLater) {
                    // Add to watch later
                    response = await httpClient.post('/api/watchlater', {
                        videoId: videoId,
                        playlistId: this.currentPlaylist.id
                    });
                } else {
                    // Remove from watch later
                    response = await httpClient.delete(`/api/watchlater/${videoId}?playlistId=${this.currentPlaylist.id}`);
                }

                if (!response.ok) {
                    // Revert on error
                    video.isInWatchLater = wasInWatchLater;
                    console.error('Failed to update watch later status');
                }
            } catch (err) {
                console.error('Error updating watch later status:', err);
            }
        },

        // ============================================================
        // Filter Operations
        // ============================================================

        openFilterEditor() {
            this.currentFilterTab = 'themes';
            this.filterValidationError = null;
            this.filterSaveMessage = null;
            this.loadFiltersFromPlaylist();
            this.showFilterEditor = true;
        },

        closeFilterEditor() {
            this.showFilterEditor = false;
            this.filterValidationError = null;
            this.filterSaveMessage = null;
        },

        loadFiltersFromPlaylist() {
            if (this.currentPlaylist && this.currentPlaylist.filters) {
                try {
                    const parsed = JSON.parse(this.currentPlaylist.filters);
                    this.filters = this.mergeFilters(this.filters, parsed);
                } catch (err) {
                    console.error('Failed to parse filters:', err);
                }
            }
        },

        mergeFilters(defaults, loaded) {
            return {
                themes: loaded.themes || defaults.themes,
                keywords: {
                    include: loaded.keywords?.include || defaults.keywords.include,
                    exclude: loaded.keywords?.exclude || defaults.keywords.exclude
                },
                duration: {
                    min: loaded.duration?.min ?? defaults.duration.min,
                    max: loaded.duration?.max ?? defaults.duration.max
                },
                publishedTime: {
                    type: loaded.publishedTime?.type || defaults.publishedTime.type,
                    days: loaded.publishedTime?.days || defaults.publishedTime.days,
                    fromDate: loaded.publishedTime?.fromDate || defaults.publishedTime.fromDate,
                    toDate: loaded.publishedTime?.toDate || defaults.publishedTime.toDate
                },
                language: {
                    preferred: loaded.language?.preferred || defaults.language.preferred,
                    region: loaded.language?.region || defaults.language.region
                },
                contentType: {
                    videos: loaded.contentType?.videos ?? defaults.contentType.videos,
                    livestreams: loaded.contentType?.livestreams ?? defaults.contentType.livestreams,
                    shorts: loaded.contentType?.shorts ?? defaults.contentType.shorts
                },
                popularity: {
                    minViews: loaded.popularity?.minViews ?? defaults.popularity.minViews,
                    minLikes: loaded.popularity?.minLikes ?? defaults.popularity.minLikes,
                    minLikeRatio: loaded.popularity?.minLikeRatio ?? defaults.popularity.minLikeRatio
                },
                channels: {
                    include: loaded.channels?.include || defaults.channels.include,
                    exclude: loaded.channels?.exclude || defaults.channels.exclude
                }
            };
        },

        saveFilters() {
            // Validate
            if (!this.validateFilters()) {
                return;
            }

            // Save to playlist
            this.currentPlaylist.filters = JSON.stringify(this.filters);

            // Show success message
            this.filterSaveMessage = 'Filtre lagret!';
            setTimeout(() => {
                this.filterSaveMessage = null;
                this.closeFilterEditor();
            }, 1500);
        },

        validateFilters() {
            this.filterValidationError = null;

            // Duration validation
            if (this.filters.duration.min >= this.filters.duration.max) {
                this.filterValidationError = 'Minimum varighet må være mindre enn maksimum';
                return false;
            }

            // Content type validation - at least one selected
            const hasContentType = this.filters.contentType.videos ||
                                  this.filters.contentType.livestreams ||
                                  this.filters.contentType.shorts;
            if (!hasContentType) {
                this.filterValidationError = 'Velg minst en innholdstype';
                return false;
            }

            // Published time validation for absolute dates
            if (this.filters.publishedTime.type === 'absolute') {
                if (this.filters.publishedTime.fromDate && this.filters.publishedTime.toDate) {
                    const from = new Date(this.filters.publishedTime.fromDate);
                    const to = new Date(this.filters.publishedTime.toDate);
                    if (from >= to) {
                        this.filterValidationError = 'Fra-dato må være før til-dato';
                        return false;
                    }
                }
            }

            return true;
        },

        // Tag management - Themes
        addTheme() {
            const theme = this.themeInput.trim();
            if (theme && !this.filters.themes.includes(theme)) {
                this.filters.themes.push(theme);
                this.themeInput = '';
            }
        },

        removeTheme(theme) {
            const idx = this.filters.themes.indexOf(theme);
            if (idx > -1) {
                this.filters.themes.splice(idx, 1);
            }
        },

        // Tag management - Keywords
        addIncludeKeyword() {
            const keyword = this.includeKeywordInput.trim();
            if (keyword && !this.filters.keywords.include.includes(keyword)) {
                this.filters.keywords.include.push(keyword);
                this.includeKeywordInput = '';
            }
        },

        removeIncludeKeyword(keyword) {
            const idx = this.filters.keywords.include.indexOf(keyword);
            if (idx > -1) {
                this.filters.keywords.include.splice(idx, 1);
            }
        },

        addExcludeKeyword() {
            const keyword = this.excludeKeywordInput.trim();
            if (keyword && !this.filters.keywords.exclude.includes(keyword)) {
                this.filters.keywords.exclude.push(keyword);
                this.excludeKeywordInput = '';
            }
        },

        removeExcludeKeyword(keyword) {
            const idx = this.filters.keywords.exclude.indexOf(keyword);
            if (idx > -1) {
                this.filters.keywords.exclude.splice(idx, 1);
            }
        },

        // Tag management - Channels
        addIncludeChannel() {
            const channel = this.channelInput.trim();
            if (channel && !this.filters.channels.include.includes(channel)) {
                this.filters.channels.include.push(channel);
                this.channelInput = '';
            }
        },

        removeIncludeChannel(channel) {
            const idx = this.filters.channels.include.indexOf(channel);
            if (idx > -1) {
                this.filters.channels.include.splice(idx, 1);
            }
        },

        addExcludeChannel() {
            const channel = this.channelExcludeInput.trim();
            if (channel && !this.filters.channels.exclude.includes(channel)) {
                this.filters.channels.exclude.push(channel);
                this.channelExcludeInput = '';
            }
        },

        removeExcludeChannel(channel) {
            const idx = this.filters.channels.exclude.indexOf(channel);
            if (idx > -1) {
                this.filters.channels.exclude.splice(idx, 1);
            }
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
