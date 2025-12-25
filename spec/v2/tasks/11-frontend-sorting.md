# Oppgave 11: Frontend - Sorteringsvalg

## Mål
Implementere dropdown-UI for å velge sorteringsstrategi.

## Kontekst

**Avhenger av:** Oppgave 05 (Sorting strategies backend)

**Krav:** Dropdown med 8 sorteringsalternativer

## Implementation

### 1. Dropdown UI

I playlist-detalj-visning, legg til over videolistene:

```html
<div class="sorting-controls">
  <label for="sort-strategy">Sortering:</label>
  <select id="sort-strategy" x-model="playlist.sortStrategy" @change="saveSortStrategy()">
    <option value="NewestFirst">Nyest først</option>
    <option value="MostRelevant">Mest relevant</option>
    <option value="MostPopular">Mest populær</option>
    <option value="MostPopularRelative">Mest populær (relativt)</option>
    <option value="HighestQuality">Høyest kvalitet</option>
    <option value="LengthShort">Korteste først</option>
    <option value="LengthLong">Lengste først</option>
    <option value="ChannelAuthority">Kanal-autoritet</option>
    <option value="WeightedScore">Vektet poengsum</option>
  </select>
</div>
```

### 2. Save to Backend

Implementer `saveSortStrategy()`:
```javascript
async saveSortStrategy() {
  const response = await httpClient.put(
    `/api/playlists/${playlist.id}`,
    { sortStrategy: playlist.sortStrategy }
  );
  // Update UI
}
```

### 3. Styling

- Dropdown skal være responsiv
- Label og dropdown på samme linje (hvis plass)
- Matching med eksisterende design
- Dark mode support

### 4. Sort Descriptor

Legg til forklarende tekst for hver sortering:
```javascript
const sortDescriptions = {
  NewestFirst: 'Videoer publisert nyligst først',
  MostRelevant: 'Mest relevant for søket ditt',
  MostPopular: 'Mest populære av alle',
  MostPopularRelative: 'Mest sett relativt til alder',
  HighestQuality: 'Høyest gjennomsnittlig rating',
  LengthShort: 'Korte videoer først',
  LengthLong: 'Lange videoer først',
  ChannelAuthority: 'Fra kanaler med størst oppfølging',
  WeightedScore: 'Kombinert vurdering'
};
```

Vis tooltip/hjelpetekst ved hover.

### 5. Refresh med Sortering

Når bruker trykker "Oppdater", benytt den valgte sorteringen:
- Backend sorterer basert på `Playlist.SortStrategy`
- Frontend viser resultater i den rekkefølgen
- Indikator på at sortering er brukt

## Akseptansekriterier

- [ ] Dropdown UI implementert
- [ ] Alle 8 strategier vises
- [ ] Valg lagres til backend
- [ ] Eksisterende sortering lastes
- [ ] Refresh bruker valgt sortering
- [ ] Hjelpetekster vises
- [ ] UI responsiv
- [ ] Dark mode support

## Leveranse

Oppdaterte filer:
- `wwwroot/index.html` (Dropdown HTML)
- `wwwroot/app.js` (Sort logic og save)
- `wwwroot/styles.css` (Styling)
