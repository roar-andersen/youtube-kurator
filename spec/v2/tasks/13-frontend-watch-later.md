# Oppgave 13: Frontend - Watch Later UI

## Mål
Implementere Watch Later-funksjonalitet med knapp på videoer og egen liste-visning.

## Kontekst

**Avhenger av:** Oppgave 07 (Watch Later API)

**Krav:**
- Knapp på hver video for å legge til/fjerne fra watch later
- Egen side/modal for "Se på senere"-lister
- Global og playlist-spesifikk watch later

## Implementation

### 1. Watch Later Button on Video

Legg til knapp på videokort:

```html
<button
  @click="toggleWatchLater(video.id)"
  :class="{ active: isInWatchLater(video.id) }"
  title="Se på senere"
>
  🕐 {{ isInWatchLater(video.id) ? 'Fjern fra senere' : 'Se på senere' }}
</button>
```

### 2. Watch Later Manager Service

```javascript
class WatchLaterService {
  async addToWatchLater(videoId, playlistId = null) {
    return fetch('/api/watchlater', {
      method: 'POST',
      headers: {
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json'
      },
      body: JSON.stringify({ videoId, playlistId })
    });
  }

  async removeFromWatchLater(videoId, playlistId = null) {
    return fetch(`/api/watchlater/${videoId}?playlistId=${playlistId || ''}`, {
      method: 'DELETE',
      headers: { 'Authorization': `Bearer ${token}` }
    });
  }

  async getWatchLater(playlistId = null) {
    const query = playlistId ? `?playlistId=${playlistId}` : '';
    return fetch(`/api/watchlater${query}`, {
      headers: { 'Authorization': `Bearer ${token}` }
    }).then(r => r.json());
  }
}
```

### 3. Watch Later State

I Alpine.js:

```javascript
Alpine.data('playlist', () => ({
  watchLaterVideos: [],

  async toggleWatchLater(videoId) {
    const isInList = this.isInWatchLater(videoId);

    if (isInList) {
      await watchLaterService.removeFromWatchLater(videoId, this.playlistId);
      this.watchLaterVideos = this.watchLaterVideos.filter(v => v.id !== videoId);
    } else {
      await watchLaterService.addToWatchLater(videoId, this.playlistId);
      // Re-fetch watch later list
      this.watchLaterVideos = await watchLaterService.getWatchLater(this.playlistId);
    }

    this.showToast(`${isInList ? 'Fjernet fra' : 'Lagt til'} Se på senere`);
  },

  isInWatchLater(videoId) {
    return this.watchLaterVideos.some(v => v.id === videoId);
  },

  async loadWatchLater() {
    this.watchLaterVideos = await watchLaterService.getWatchLater(this.playlistId);
  }
}))
```

### 4. Watch Later Page/Modal

Opprett `wwwroot/watch-later.html`:

```html
<div id="watch-later-modal" style="display: none;">
  <div class="modal-header">
    <h2>Se på senere</h2>
    <button @click="closeWatchLater()">✕</button>
  </div>

  <div class="modal-body">
    <!-- Tabs for Global vs Playlist-spesifikk -->
    <div class="tabs">
      <button
        @click="watchLaterTab = 'global'"
        :class="{ active: watchLaterTab === 'global' }"
      >
        Global liste
      </button>
      <button
        @click="watchLaterTab = 'playlist'"
        :class="{ active: watchLaterTab === 'playlist' }"
      >
        {{ currentPlaylistName }}
      </button>
    </div>

    <!-- Video liste -->
    <div class="watch-later-list">
      <template x-for="video in getWatchLaterList()" :key="video.id">
        <div class="watch-later-item">
          <img :src="video.thumbnailUrl" />
          <div class="video-info">
            <h4>{{ video.title }}</h4>
            <p>{{ video.channelName }}</p>
          </div>
          <button @click="toggleWatchLater(video.id)" class="remove-btn">
            ✕
          </button>
        </div>
      </template>
    </div>

    <template x-if="getWatchLaterList().length === 0">
      <p class="empty-state">Ingen videoer på listen</p>
    </template>
  </div>
</div>
```

### 5. Navigation

Legg til "Se på senere"-knapp i header:

```html
<nav>
  <!-- ... existing nav ... -->
  <button @click="openWatchLater()" title="Se på senere">
    🕐 Se på senere (<span x-text="watchLaterVideos.length"></span>)
  </button>
</nav>
```

### 6. Styling

```css
.watch-later-list {
  display: flex;
  flex-direction: column;
  gap: 10px;
}

.watch-later-item {
  display: flex;
  gap: 10px;
  padding: 10px;
  border: 1px solid #ddd;
  border-radius: 4px;
  align-items: center;
}

.watch-later-item img {
  width: 80px;
  height: 45px;
  object-fit: cover;
  border-radius: 2px;
}

.watch-later-item .video-info {
  flex: 1;
}

.watch-later-item h4 {
  margin: 0;
  font-size: 14px;
}

.watch-later-item p {
  margin: 4px 0 0 0;
  font-size: 12px;
  color: #666;
}

.remove-btn {
  padding: 4px 8px;
  background: #f44336;
  color: white;
  border: none;
  border-radius: 4px;
  cursor: pointer;
}
```

## Akseptansekriterier

- [ ] Watch Later-knapp på hver video
- [ ] Knapp toggle i UI umiddelbart
- [ ] POST til backend fungerer
- [ ] DELETE fra backend fungerer
- [ ] Watch Later-modal/side implementert
- [ ] Global og playlist-spesifikk lister fungerer
- [ ] Antall videoer vises i header
- [ ] Responsiv design
- [ ] Dark mode support

## Leveranse

Nye filer:
- `wwwroot/watch-later.html`

Oppdaterte filer:
- `wwwroot/index.html` (Watch Later-knapp)
- `wwwroot/app.js` (Watch Later-logikk)
- `wwwroot/styles.css` (Styling)
- `wwwroot/js/http-client.js` (WatchLaterService)
