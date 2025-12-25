# Oppgave 12: Frontend - Video-status UI

## Mål
Implementere UI-knapper for å markere videoer som Seen/Saved/Rejected, med visuell feedback.

## Kontekst

**Avhenger av:** Oppgave 06 (VideoStatus API)

**Krav:**
- Knapper for Seen, Saved, Rejected på hver videokort
- Visuell indikator for valgt status
- One-click update til backend

## Implementation

### 1. Video Card Update

Oppdater videokort-HTML i `wwwroot/index.html`:

```html
<div class="video-card">
  <img :src="video.thumbnailUrl" />
  <h3>{{ video.title }}</h3>
  <p>{{ video.channelName }}</p>
  <!-- Existing info -->

  <div class="video-actions">
    <button
      @click="updateVideoStatus(video.id, 'Seen')"
      :class="{ active: video.status === 'Seen' }"
      title="Markér som sett"
    >
      👁️ Sett
    </button>

    <button
      @click="updateVideoStatus(video.id, 'Saved')"
      :class="{ active: video.status === 'Saved' }"
      title="Lagre for senere"
    >
      ⭐ Lagret
    </button>

    <button
      @click="updateVideoStatus(video.id, 'Rejected')"
      :class="{ active: video.status === 'Rejected' }"
      title="Ikke interessert"
    >
      ✕ Ikke av interesse
    </button>
  </div>
</div>
```

### 2. Status Management

I Alpine.js data:

```javascript
Alpine.data('playlist', () => ({
  videos: [],

  async updateVideoStatus(videoId, status) {
    try {
      const response = await httpClient.post(
        `/api/videos/${videoId}/status`,
        {
          playlistId: this.playlistId,
          status: status,
          rejectReason: status === 'Rejected' ? 'Bruker valgte å forkaste' : null
        }
      );

      // Update local video status
      const video = this.videos.find(v => v.id === videoId);
      if (video) {
        video.status = status;
      }

      // Show success toast
      this.showToast(`Video markert som ${this.statusLabel(status)}`);
    } catch (error) {
      this.showToast('Kunne ikke oppdatere video-status', 'error');
    }
  },

  statusLabel(status) {
    const labels = {
      'Seen': 'sett',
      'Saved': 'lagret',
      'Rejected': 'avvist'
    };
    return labels[status] || status;
  }
}))
```

### 3. Styling

```css
.video-actions {
  display: flex;
  gap: 8px;
  margin-top: 10px;
  flex-wrap: wrap;
}

.video-actions button {
  flex: 1;
  min-width: 80px;
  padding: 6px 8px;
  border: 1px solid #ccc;
  border-radius: 4px;
  background: white;
  cursor: pointer;
  font-size: 12px;
  transition: all 0.2s;
}

.video-actions button:hover {
  background: #f0f0f0;
}

.video-actions button.active {
  background: #4CAF50;
  color: white;
  border-color: #4CAF50;
}

@media (prefers-color-scheme: dark) {
  .video-actions button {
    background: #333;
    border-color: #555;
    color: white;
  }

  .video-actions button:hover {
    background: #444;
  }

  .video-actions button.active {
    background: #4CAF50;
  }
}
```

### 4. Status Persistence

- Load video status fra backend på refresh
- Persist status lokalt i component data
- Show loading state mens request kjører
- Handle errors med retry-option

### 5. Filter by Status

Valgfritt: Legg til filter-knapper for å vise kun:
- Alle videoer
- Umarkerte (New)
- Sett
- Lagret
- Avvist

## Akseptansekriterier

- [ ] Tre status-knapper vises på hver video
- [ ] Klikk sender POST til backend
- [ ] UI oppdateres umiddelbart
- [ ] Status persisteres
- [ ] Visuell indikator for aktiv status
- [ ] Success/error toasts vises
- [ ] Dark mode support
- [ ] Responsiv design

## Leveranse

Oppdaterte filer:
- `wwwroot/index.html` (Knapp-HTML)
- `wwwroot/app.js` (Status-logikk)
- `wwwroot/styles.css` (Styling)
