# Oppgave 7.2: Implementer navigasjon mellom oversikt og detalj

## Fase
Fase 7: Frontend – JavaScript og Alpine.js

## Avhengigheter
- Oppgave 7.1 (app.js må eksistere med navigasjonsfunksjoner)

## Formål
Sørge for at navigasjon mellom playlist-oversikt og playlist-detalj-visninger fungerer smidig med riktig state management.

## Oppgavebeskrivelse

### 1. Verifiser showView()-funksjon i app.js
Funksjonen `showView(view)` i app.js (Oppgave 7.1) skal håndtere navigasjonen:

```javascript
showView(view) {
    this.currentView = view;
    this.error = null;
    this.successMessage = null;
    if (view === 'list') {
        this.currentPlaylist = null;
        this.videos = [];
        this.currentPage = 1;
    }
}
```

### 2. Verifiser visninger i HTML
I `index.html`, sikre at begge visninger bruker `x-show` med `currentView`:

```html
<!-- Playlist List View -->
<section id="playlist-list" x-show="currentView === 'list'" class="view">
    ...
</section>

<!-- Playlist Detail View -->
<section id="playlist-detail" x-show="currentView === 'detail'" class="view">
    ...
</section>
```

### 3. Verifiser breadcrumb-navigasjon
I `index.html` header skal breadcrumb være synlig kun i detail-view:

```html
<nav class="breadcrumb" v-show="currentView === 'detail'">
    <a href="#" @click.prevent="showView('list')">Spillelister</a>
    <span class="separator">/</span>
    <span x-text="currentPlaylist?.name || 'Laster...'"></span>
</nav>
```

**Korrekt bruk av Alpine.js direktiver**:
- `x-show`: Viser/skjuler element (oppbevarer DOM)
- `x-text`: Setter tekst (dynamisk)
- `@click.prevent`: Håndterer klikk, preventerer standard oppførsel
- `:` prefix for property binding (f.eks. `:src`, `:disabled`, `:key`)

### 4. Verifiser selectPlaylist()-funksjon
I app.js skal denne funksjonen:
1. Hente playlist fra API
2. Sette `currentPlaylist`
3. Skifte view til 'detail'
4. Nullstille videos og paginering

```javascript
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
}
```

### 5. Verifiser onclick-håndtering for playlist-kort
I HTML skal playlist-kort ha `@click="selectPlaylist(playlist.id)"`:

```html
<div class="playlist-card" @click="selectPlaylist(playlist.id)">
    <div class="playlist-card-header">
        <h3 x-text="playlist.name"></h3>
    </div>
    ...
</div>
```

### 6. Verifiser tilbakeknapp
I detail-view header skal logo-linken og breadcrumb-linken tillate tilbakenavigasjon:

```html
<h1 class="header-title">
    <a href="#" @click.prevent="showView('list')" class="logo-link">
        📺 YouTube Kurator
    </a>
</h1>
```

### 7. Verifiser state oppbevares under sessjon (valgfritt)
For bedre UX kan du lagre `currentPlaylist` i `localStorage` slik at siden viser samme playlist hvis siden refreshes:

```javascript
// I selectPlaylist():
localStorage.setItem('lastPlaylistId', id);

// I initApp():
const lastPlaylistId = localStorage.getItem('lastPlaylistId');
if (lastPlaylistId) {
    await this.selectPlaylist(lastPlaylistId);
}
```

### 8. Test navigasjon
Manual testing:

1. **Åpne app**: Skal se playlist-oversikt
2. **Klikk på playlist-kort**: Skal gå til detail-view med riktig data
3. **Klikk logo eller "Spillelister" i breadcrumb**: Skal gå tilbake til oversikt
4. **Opprett ny playlist**: Skal være i oversikt-view
5. **Slette playlist fra detail-view**: Skal gå tilbake til oversikt
6. **Refresh siden mens i detail-view**: Skal være på samme playlist (hvis localStorage er implementert)

## Akseptansekriterier
- [ ] `selectPlaylist(id)` hentet playlist fra API og setter state
- [ ] `showView(view)` endrer `currentView` og nullstiller state riktig
- [ ] Begge visninger bruker `x-show="currentView === '...'"` korrekt
- [ ] Playlist-kort har `@click="selectPlaylist(playlist.id)"`
- [ ] Logo-lenke og breadcrumb-lenker bruker `@click.prevent="showView('list')"`
- [ ] Navigasjon fungerer begge veier (list → detail → list)
- [ ] Playlist-navn vises dynamisk i breadcrumb
- [ ] State resettes når man går fra detail til list (videos, currentPage)
- [ ] Ingen JavaScript-feil ved navigasjon

## Referanser
- [Spesifikasjon: Frontend – Sider/visninger](youtube-kurator-v1-spec.md#syderoversikt)
- [Alpine.js x-show](https://alpinejs.dev/directives/show)
- [Alpine.js x-text](https://alpinejs.dev/directives/text)
- [Alpine.js @click](https://alpinejs.dev/directives/on)
