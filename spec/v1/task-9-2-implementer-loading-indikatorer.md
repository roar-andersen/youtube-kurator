# Oppgave 9.2: Implementer loading-indikatorer

## Fase
Fase 9: Feilmeldinger og brukeropplevelse

## Avhengigheter
- Oppgave 7.1 (app.js med loading-states)
- Oppgave 6.1 (index.html med loading-spinners)
- Oppgave 6.2 (CSS for loading-animasjoner)

## Formål
Vise visuelle indikatorer når operasjoner pågår, for å gi brukeren feedback og hindre dupliserte klikk.

## Oppgavebeskrivelse

### 1. Verifiser loading-states i app.js
I `appState()` skal disse states eksistere:

```javascript
isLoading: false,        // For CRUD-operasjoner på playlists
isLoadingVideos: false   // For refresh av videoer
```

### 2. Implementer loading-state for alle async operasjoner
For hver async-funksjon skal loading-state settes korrekt:

```javascript
async loadPlaylists() {
    this.isLoading = true;
    try {
        // ... API call
    } catch (err) {
        // ... error handling
    } finally {
        this.isLoading = false;
    }
}

async createPlaylist() {
    this.isLoading = true;
    try {
        // ... API call
    } catch (err) {
        // ... error handling
    } finally {
        this.isLoading = false;
    }
}

async deletePlaylist() {
    this.isLoading = true;
    try {
        // ... API call
    } catch (err) {
        // ... error handling
    } finally {
        this.isLoading = false;
    }
}

async savePlaylist() {
    this.isLoading = true;
    try {
        // ... API call
    } catch (err) {
        // ... error handling
    } finally {
        this.isLoading = false;
    }
}

async refreshPlaylist() {
    this.isLoadingVideos = true;
    try {
        // ... API call
    } catch (err) {
        // ... error handling
    } finally {
        this.isLoadingVideos = false;
    }
}
```

### 3. Verifiser loading-spinners i HTML
I `index.html`, sikre at loading-spinners er på riktige steder:

```html
<!-- List view loading -->
<div x-show="isLoading" class="loading-spinner">
    Laster spillelister...
</div>

<!-- Detail view loading -->
<div x-show="isLoadingVideos" class="loading-spinner">
    Henter videoer...
</div>
```

### 4. Implementer button-disabling under lasting
Buttons skal være disabled mens operasjoner pågår:

```html
<!-- Create button -->
<button class="btn btn-primary"
        @click="showCreateDialog = true"
        :disabled="isLoading">
    + Ny Spilleliste
</button>

<!-- Refresh button -->
<button class="btn btn-primary btn-large"
        @click="refreshPlaylist()"
        :disabled="isLoadingVideos">
    🔄 Oppdater Videoer
</button>

<!-- Save button -->
<button class="btn btn-secondary"
        @click="savePlaylist()"
        :disabled="isLoading">
    💾 Lagre
</button>

<!-- Delete button -->
<button class="btn btn-danger"
        @click="deletePlaylist()"
        :disabled="isLoading">
    🗑️ Slett
</button>
```

### 5. Verifiser CSS for loading-spinner
I `styles.css` (Oppgave 6.2), skal spinner-styling være på plass:

```css
.loading-spinner {
    text-align: center;
    padding: var(--spacing-2xl);
    color: var(--text-secondary);
    font-size: var(--font-size-lg);
    animation: pulse 1.5s ease-in-out infinite;
}

@keyframes pulse {
    0%, 100% {
        opacity: 1;
    }
    50% {
        opacity: 0.5;
    }
}
```

**Eller en mer fancy spinner med dots**:

```css
.loading-spinner::after {
    content: '.';
    animation: dots 1.5s steps(3, end) infinite;
}

@keyframes dots {
    0%, 20% {
        content: '.';
    }
    40% {
        content: '..';
    }
    60%, 100% {
        content: '...';
    }
}
```

### 6. Implementer cache-info-visning
Når videoer hentes fra cache, vis info:

```html
<!-- Cache Info -->
<div x-show="lastRefresh && lastRefresh.fromCache" class="cache-info">
    ℹ️ Videoene er hentet fra cache og er gyldige til
    <span x-text="formatDateTime(lastRefresh.cacheExpiresUtc)"></span>
</div>
```

### 7. Test loading-indikatorer
Manuell testing:

1. **Last spillelister**:
   - Åpne appen
   - Spinner skal vises mens laster
   - Spinner skal forsvinne når ferdig

2. **Opprett playlist**:
   - Klikk "+ Ny Spilleliste"
   - Fyll inn og klikk "Opprett"
   - Spinner skal vises
   - Button skal være disabled
   - Spinner skal forsvinne når ferdig

3. **Refresh videoer**:
   - Klikk på playlist
   - Klikk "Oppdater Videoer"
   - Spinner skal vises ("Henter videoer...")
   - Refresh-knappen skal være disabled
   - Videoer skal lastes
   - Cache-info skal vises hvis fra cache

4. **Lagre playlist**:
   - Endre navn/søkeord
   - Klikk "Lagre"
   - Button skal være disabled
   - Spinner skal vises kort
   - Success-melding skal vises

## Akseptansekriterier
- [ ] `isLoading` og `isLoadingVideos` states eksisterer
- [ ] Alle async-funksjoner setter loading-state i try/finally
- [ ] Loading-spinners vises i HTML med `x-show="isLoading..."`
- [ ] Buttons er disabled med `:disabled="isLoading..."`
- [ ] CSS-animasjon for spinner eksisterer
- [ ] Cache-info vises når videoer fra cache
- [ ] Buttons blir enabled igjen etter operasjon
- [ ] Spinners forsvinner når operasjon er ferdig
- [ ] Ingen dupliserte requests hvis bruker klikker flere ganger (buttons disabled)

## Referanser
- [Spesifikasjon: Frontend](youtube-kurator-v1-spec.md#8-frontend)
- [Alpine.js :disabled Binding](https://alpinejs.dev/directives/bind)
- [WCAG: Loading States](https://www.w3.org/WAI/WCAG21/Understanding/status-messages.html)
