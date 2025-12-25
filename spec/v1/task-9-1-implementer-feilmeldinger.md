# Oppgave 9.1: Implementer brukervenlige feilmeldinger

## Fase
Fase 9: Feilmeldinger og brukeropplevelse

## Avhengigheter
- Oppgave 7.1 (app.js med error-state må eksistere)
- Oppgave 6.2 (CSS for toasts må eksistere)

## Formål
Sørge for at alle feilscenarioer vises med klare, brukervennlige norske feilmeldinger.

## Oppgavebeskrivelse

### 1. Verifiser error-state i app.js
I `app.js` skal følgende states finnes:

```javascript
error: null,
successMessage: null
```

### 2. Implementer error-håndtering for alle API-kall
For hver API-kallsted skal error-håndtering være på plass. Eksempel:

```javascript
// Template for alle try-catch blokker
async someFunction() {
    this.error = null; // Clear previous errors
    try {
        // API call
        // Success handling
        this.successMessage = 'Operasjonen var vellykket!';
    } catch (err) {
        console.error('Error:', err);
        this.error = 'Brukervennlig feilmelding';
    } finally {
        this.isLoading = false;
    }
}
```

### 3. Definér standardiserte feilmeldinger
I `app.js`, legg til en feilmeldingskatalog:

```javascript
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
    LOAD_PLAYLISTS_FAILED: 'Kunne ikke laste spillelister. Sjekk internettforbindelsen.'
};
```

### 4. Håndter spesifikke feil fra backend
Backend kan returnere error-objekt som dette:

```json
{
  "videos": [...],
  "fromCache": true,
  "error": {
    "type": "QuotaExceeded",
    "message": "YouTube-kvoten er brukt opp for i dag. Prøv igjen i morgen."
  }
}
```

I `refreshPlaylist()` skal dette håndteres:

```javascript
async refreshPlaylist() {
    if (!this.currentPlaylist) return;

    this.isLoadingVideos = true;
    this.error = null;
    try {
        const response = await fetch(`/api/playlists/${this.currentPlaylist.id}/refresh`, {
            method: 'POST'
        });

        if (!response.ok) {
            this.error = ERROR_MESSAGES.LOAD_VIDEOS_FAILED;
            throw new Error('Failed to refresh');
        }

        const data = await response.json();
        this.videos = data.videos || [];
        this.lastRefresh = {
            fromCache: data.fromCache,
            cacheExpiresUtc: data.cacheExpiresUtc
        };

        // Hvis backend returnerer error, vis den selv om requesten var 200 OK
        if (data.error) {
            this.error = data.error.message || ERROR_MESSAGES.GENERIC_ERROR;
        }

        this.currentPage = 1;
    } catch (err) {
        console.error('Error refreshing playlist:', err);
        if (!this.error) {
            this.error = ERROR_MESSAGES.LOAD_VIDEOS_FAILED;
        }
    } finally {
        this.isLoadingVideos = false;
    }
}
```

### 5. Verifiser error-visning i HTML
I `index.html`, sikre at error-toast-en vises:

```html
<!-- Error Message -->
<div x-show="error" class="error-toast" @click="error = null">
    <span x-text="error"></span>
    <button class="toast-close">✕</button>
</div>
```

### 6. Implementer auto-dismiss for toasts
Legg til denne funksjonen i `appState()`:

```javascript
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
}
```

Bruk den slik:

```javascript
// I stedet for: this.error = 'message'
// Bruk: this.showError(ERROR_MESSAGES.NETWORK_ERROR)
```

### 7. Test feilscenarioer
Manuell testing av alle feilscenarioer:

1. **Nettverksfeil**: Slå av internett (eller bruk DevTools offline-modus)
   - Forventet: "Kunne ikke koble til..."

2. **YouTube-kvote**: Bruk invalid API-nøkkel eller test med quota-exceeded-simulering
   - Forventet: "YouTube-grensen er nådd..."

3. **Generell feil**: Backend returnerer error
   - Forventet: Spesifikk feilmelding fra backend eller fallback til "Noe gikk galt"

4. **Validering**: Forsøk å opprett playlist uten navn/søkeord
   - Forventet: "Navn og søkeord er påkrevd"

5. **Toast-oppførsel**: Feilmelding skal vises, så forsvinne etter 5 sekunder
   - Eller du kan klikk på toasten for å lukke den manuelt

## Akseptansekriterier
- [ ] Error-state eksisterer i `appState()`
- [ ] Alle API-kall har try-catch-blokker
- [ ] Feilmeldinger er norske og brukervennlige
- [ ] ERROR_MESSAGES-katalog er definert
- [ ] `refreshPlaylist()` håndterer backend error-objekt
- [ ] Error-toast vises i HTML med `x-show="error"`
- [ ] Toast lukkes når bruker klikker på den
- [ ] Toast auto-dismiss fungerer (5 sekunder)
- [ ] Success-toast fungerer også (3 sekunder)
- [ ] Alle feilscenarioer er testet manuelt

## Referanser
- [Spesifikasjon: Feilmeldinger](youtube-kurator-v1-spec.md#feilmeldinger)
- [Spesifikasjon: Feilhåndtering](youtube-kurator-v1-spec.md#feilhåndtering)
- [WCAG: Error Identification](https://www.w3.org/WAI/WCAG21/Understanding/error-identification.html)
