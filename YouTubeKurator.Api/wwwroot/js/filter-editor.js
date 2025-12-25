// Filter Editor Component for Alpine.js
function filterEditorData() {
    return {
        // UI State
        showFilterEditor: false,
        filterValidationError: null,
        filterSaveMessage: null,

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

        // UI input buffers
        themeInput: '',
        includeKeywordInput: '',
        excludeKeywordInput: '',
        channelInput: '',

        // ============================================================
        // Modal Control
        // ============================================================

        openFilterEditor() {
            this.showFilterEditor = true;
            this.filterValidationError = null;
            this.filterSaveMessage = null;
        },

        closeFilterEditor() {
            this.showFilterEditor = false;
            this.filterValidationError = null;
            this.filterSaveMessage = null;
        },

        // ============================================================
        // Load & Save Filters
        // ============================================================

        loadFiltersFromPlaylist(playlist) {
            if (playlist && playlist.filters) {
                try {
                    const parsed = JSON.parse(playlist.filters);
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

        // ============================================================
        // Validation
        // ============================================================

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

        // ============================================================
        // Tag Management - Themes
        // ============================================================

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

        // ============================================================
        // Tag Management - Keywords
        // ============================================================

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

        // ============================================================
        // Tag Management - Channels
        // ============================================================

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
            const channel = this.channelInput.trim();
            if (channel && !this.filters.channels.exclude.includes(channel)) {
                this.filters.channels.exclude.push(channel);
                this.channelInput = '';
            }
        },

        removeExcludeChannel(channel) {
            const idx = this.filters.channels.exclude.indexOf(channel);
            if (idx > -1) {
                this.filters.channels.exclude.splice(idx, 1);
            }
        },

        // ============================================================
        // Display Helpers
        // ============================================================

        formatDuration(seconds) {
            if (!seconds) return '0:00';
            const hours = Math.floor(seconds / 3600);
            const minutes = Math.floor((seconds % 3600) / 60);
            const secs = seconds % 60;

            if (hours > 0) {
                return `${hours}h ${minutes}m`;
            }
            return `${minutes}m ${secs}s`;
        },

        getFilterSummary() {
            const parts = [];

            if (this.filters.themes.length > 0) {
                parts.push(`Tema: ${this.filters.themes.join(', ')}`);
            }

            if (this.filters.duration.min > 0 || this.filters.duration.max < 10800) {
                const minStr = this.formatDuration(this.filters.duration.min);
                const maxStr = this.formatDuration(this.filters.duration.max);
                parts.push(`Varighet: ${minStr} - ${maxStr}`);
            }

            if (this.filters.language.region) {
                parts.push(`Region: ${this.filters.language.region}`);
            }

            if (this.filters.channels.include.length > 0) {
                parts.push(`Kanaler: ${this.filters.channels.include.join(', ')}`);
            }

            return parts.length > 0 ? parts.join(' | ') : 'Ingen filtre';
        }
    };
}
