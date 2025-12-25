# Oppgave 10: Frontend - Filter-konfigurasjon

## Mål
Implementere kompleks filter-UI for spilleliste-konfigurering.

## Kontekst

**Avhenger av:** Oppgave 04 (Filter backend)

**Krav:** UI for alle 7 filtertyper fra filter JSON-skjemaet

## Filter UI Komponenter

### Tema og søkeord
- Text input for temaer (comma-separated)
- Två separate text inputs for inkluder/ekskluder nøkkelord
- Tag-input (mulig å fjerne individuelt)

### Varighet
- Min/maks slider eller number inputs
- Display valgt range (f.eks. "5 min - 1 time")

### Publiseringstid
- Radio buttons: "Relativ" vs "Absolutt"
- Hvis relativ: dropdown eller number input for dager (1, 7, 30, 90, 365)
- Hvis absolutt: date range picker (fra/til)

### Språk og region
- Dropdown for foretrukket språk (no, en, sv, da, etc.)
- Dropdown for region (NO, SE, DK, GB, etc.)

### Innholdstype
- Checkboxes: Videos, Livestreams, Shorts
- (Kan velge flere)

### Popularitet
- Number inputs: Min visninger, Min likes
- Slider eller percentage: Min likes/views ratio
- Hjelpetekst: "Likes per 100 visninger"

### Kanaler
- Input og dropdown for å søke kanaler
- List med valgte kanaler (kan fjerne)
- Separate seksjoner for "Inkluder" og "Ekskluder"

## Implementation Strategy

### 1. Modal/Drawer for filter-edit

Opprett `wwwroot/filter-editor.html`:
- Open ved klikk på "Rediger filtre"-knapp
- Vis alle 7 filtertyper
- Save-knapp lagrer til Playlist.Filters (JSON)
- Cancel lukker uten å lagre

### 2. Alpine.js Component

```javascript
Alpine.data('filterEditor', () => ({
  filters: {},
  themes: [],
  includeKeywords: [],
  excludeKeywords: [],
  duration: { min: 0, max: 3600 },
  publishedTime: { type: 'relative', days: 30 },
  language: { preferred: 'no', region: 'NO' },
  contentType: { videos: true, livestreams: false, shorts: false },
  popularity: { minViews: 0, minLikes: 0, minLikeRatio: 0 },
  channels: { include: [], exclude: [] },

  // Load filters fra Playlist
  loadFilters(filterJson) { /* ... */ },

  // Save filters til Playlist
  saveFilters() { /* ... */ },

  // Sync JSON structure
  toJSON() { /* ... */ }
}))
```

### 3. Validation

- Varighet: min < max
- Kanaler: ikke duplikater
- Kombinasjoner: ikke alle filter disabled

### 4. UI State

- Show validation errors
- Show saved indicator
- Show loading state
- Auto-save ved blur (valgfritt)

### 5. Display Current Filters

I playlist-detalj, vis aktivt valg:
- "Tema: gaming, tutorial"
- "Varighet: 5 min - 1 time"
- "Region: Norge"
- osv.

## Akseptansekriterier

- [ ] Alle 7 filtertyper har UI
- [ ] Filter-data lagres i Playlist.Filters JSON
- [ ] Existing filters lastes og vises
- [ ] Validering fungerer
- [ ] Save/Cancel fungerer
- [ ] UI responsiv
- [ ] Hjelpetekster hvor nødvendig
- [ ] Dark mode support

## Leveranse

Nye filer:
- `wwwroot/filter-editor.html`
- `wwwroot/js/components/filter-editor.js`

Oppdaterte filer:
- `wwwroot/index.html` (Rediger filtre-knapp)
- `wwwroot/app.js` (Alpine-komponenter)
