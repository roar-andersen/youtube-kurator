# Oppgave 6.2: Opprett CSS-styling

## Fase
Fase 6: Frontend – HTML og struktur

## Avhengigheter
- Oppgave 6.1 (index.html må eksistere med klasse-navn og struktur)

## Formål
Lag responsiv og brukervennlig CSS-styling for hele applikasjonen uten external rammeverk.

## Oppgavebeskrivelse

### 1. Opprett styles.css
Lag fil `src/YouTubeKurator.Api/wwwroot/styles.css`:

```css
/* ============================================
   CSS Variables & Reset
   ============================================ */

:root {
    /* Colors */
    --primary: #3b82f6;
    --primary-dark: #1e40af;
    --primary-light: #dbeafe;
    --secondary: #6b7280;
    --secondary-dark: #374151;
    --danger: #ef4444;
    --danger-dark: #b91c1c;
    --success: #10b981;
    --success-dark: #047857;
    --background: #ffffff;
    --background-secondary: #f9fafb;
    --text: #1f2937;
    --text-secondary: #6b7280;
    --border: #e5e7eb;
    --shadow: rgba(0, 0, 0, 0.1);
    --shadow-md: rgba(0, 0, 0, 0.15);

    /* Spacing */
    --spacing-xs: 0.25rem;
    --spacing-sm: 0.5rem;
    --spacing-md: 1rem;
    --spacing-lg: 1.5rem;
    --spacing-xl: 2rem;
    --spacing-2xl: 3rem;

    /* Typography */
    --font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Oxygen, Ubuntu, Cantarell, sans-serif;
    --font-size-sm: 0.875rem;
    --font-size-base: 1rem;
    --font-size-lg: 1.125rem;
    --font-size-xl: 1.25rem;
    --font-size-2xl: 1.5rem;
    --font-size-3xl: 2rem;

    /* Border Radius */
    --radius-sm: 0.375rem;
    --radius-md: 0.5rem;
    --radius-lg: 0.75rem;

    /* Transitions */
    --transition-fast: 0.15s ease-in-out;
    --transition-normal: 0.3s ease-in-out;
}

* {
    margin: 0;
    padding: 0;
    box-sizing: border-box;
}

html {
    scroll-behavior: smooth;
}

body {
    font-family: var(--font-family);
    font-size: var(--font-size-base);
    color: var(--text);
    background-color: var(--background-secondary);
    line-height: 1.5;
}

/* ============================================
   Header
   ============================================ */

.header {
    background-color: var(--background);
    border-bottom: 1px solid var(--border);
    padding: var(--spacing-md) 0;
    box-shadow: 0 1px 3px var(--shadow);
    position: sticky;
    top: 0;
    z-index: 100;
}

.header-content {
    max-width: 1200px;
    margin: 0 auto;
    padding: 0 var(--spacing-md);
    display: flex;
    justify-content: space-between;
    align-items: center;
}

.header-title {
    font-size: var(--font-size-2xl);
    margin: 0;
}

.logo-link {
    color: var(--text);
    text-decoration: none;
    transition: color var(--transition-fast);
}

.logo-link:hover {
    color: var(--primary);
}

.breadcrumb {
    font-size: var(--font-size-sm);
    color: var(--text-secondary);
}

.breadcrumb a {
    color: var(--primary);
    text-decoration: none;
    transition: color var(--transition-fast);
}

.breadcrumb a:hover {
    color: var(--primary-dark);
}

.breadcrumb .separator {
    margin: 0 var(--spacing-sm);
}

/* ============================================
   Main Content
   ============================================ */

.main-content {
    max-width: 1200px;
    margin: 0 auto;
    padding: var(--spacing-xl) var(--spacing-md);
    min-height: calc(100vh - 80px);
}

.view {
    display: block;
}

.view-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: var(--spacing-2xl);
}

.view-header h2 {
    font-size: var(--font-size-2xl);
    margin: 0;
}

/* ============================================
   Buttons
   ============================================ */

.btn {
    padding: var(--spacing-sm) var(--spacing-md);
    border: none;
    border-radius: var(--radius-md);
    font-size: var(--font-size-base);
    font-weight: 500;
    cursor: pointer;
    transition: all var(--transition-fast);
    display: inline-flex;
    align-items: center;
    gap: var(--spacing-sm);
    text-decoration: none;
}

.btn:disabled {
    opacity: 0.5;
    cursor: not-allowed;
}

.btn-primary {
    background-color: var(--primary);
    color: white;
}

.btn-primary:hover:not(:disabled) {
    background-color: var(--primary-dark);
    transform: translateY(-2px);
    box-shadow: 0 4px 8px var(--shadow-md);
}

.btn-secondary {
    background-color: var(--secondary);
    color: white;
}

.btn-secondary:hover:not(:disabled) {
    background-color: var(--secondary-dark);
}

.btn-danger {
    background-color: var(--danger);
    color: white;
}

.btn-danger:hover:not(:disabled) {
    background-color: var(--danger-dark);
}

.btn-small {
    padding: var(--spacing-xs) var(--spacing-sm);
    font-size: var(--font-size-sm);
}

.btn-large {
    padding: var(--spacing-md) var(--spacing-lg);
    font-size: var(--font-size-lg);
}

.button-group {
    display: flex;
    gap: var(--spacing-md);
    margin-top: var(--spacing-md);
}

/* ============================================
   Form Elements
   ============================================ */

.input,
textarea {
    width: 100%;
    padding: var(--spacing-sm) var(--spacing-md);
    border: 1px solid var(--border);
    border-radius: var(--radius-md);
    font-family: var(--font-family);
    font-size: var(--font-size-base);
    color: var(--text);
    transition: border-color var(--transition-fast);
}

.input:focus,
textarea:focus {
    outline: none;
    border-color: var(--primary);
    box-shadow: 0 0 0 3px var(--primary-light);
}

label {
    display: block;
    margin-bottom: var(--spacing-sm);
    font-weight: 500;
    color: var(--text);
}

.edit-form {
    display: flex;
    flex-direction: column;
    gap: var(--spacing-md);
}

/* ============================================
   Playlist Cards (List View)
   ============================================ */

.playlists-grid {
    display: grid;
    grid-template-columns: repeat(auto-fill, minmax(250px, 1fr));
    gap: var(--spacing-lg);
    margin-bottom: var(--spacing-2xl);
}

.playlist-card {
    background-color: var(--background);
    border: 1px solid var(--border);
    border-radius: var(--radius-lg);
    padding: var(--spacing-lg);
    cursor: pointer;
    transition: all var(--transition-normal);
    box-shadow: 0 1px 3px var(--shadow);
}

.playlist-card:hover {
    transform: translateY(-4px);
    box-shadow: 0 10px 20px var(--shadow-md);
    border-color: var(--primary);
}

.playlist-card-header {
    margin-bottom: var(--spacing-md);
}

.playlist-card-header h3 {
    font-size: var(--font-size-xl);
    margin: 0;
    color: var(--text);
    word-wrap: break-word;
}

.playlist-card-body {
    display: flex;
    flex-direction: column;
    gap: var(--spacing-sm);
}

.search-query {
    font-size: var(--font-size-sm);
    color: var(--text-secondary);
    font-style: italic;
    margin: 0;
}

.playlist-meta {
    font-size: var(--font-size-sm);
    color: var(--text-secondary);
    margin: 0;
}

.updated-date {
    display: block;
}

/* ============================================
   Video Cards (Detail View)
   ============================================ */

.videos-container {
    margin-top: var(--spacing-2xl);
}

.videos-grid {
    display: grid;
    grid-template-columns: repeat(auto-fill, minmax(200px, 1fr));
    gap: var(--spacing-lg);
    margin: var(--spacing-lg) 0;
}

.video-card {
    background-color: var(--background);
    border-radius: var(--radius-lg);
    overflow: hidden;
    cursor: pointer;
    transition: all var(--transition-normal);
    box-shadow: 0 1px 3px var(--shadow);
    display: flex;
    flex-direction: column;
}

.video-card:hover {
    transform: translateY(-4px);
    box-shadow: 0 10px 20px var(--shadow-md);
}

.video-thumbnail {
    position: relative;
    width: 100%;
    padding-bottom: 56.25%; /* 16:9 aspect ratio */
    overflow: hidden;
    background-color: #000;
}

.video-thumbnail img {
    position: absolute;
    top: 0;
    left: 0;
    width: 100%;
    height: 100%;
    object-fit: cover;
}

.duration-badge {
    position: absolute;
    bottom: var(--spacing-sm);
    right: var(--spacing-sm);
    background-color: rgba(0, 0, 0, 0.8);
    color: white;
    padding: var(--spacing-xs) var(--spacing-sm);
    border-radius: var(--radius-sm);
    font-size: var(--font-size-sm);
    font-weight: 500;
}

.video-info {
    padding: var(--spacing-md);
    display: flex;
    flex-direction: column;
    gap: var(--spacing-sm);
    flex: 1;
}

.video-title {
    font-size: var(--font-size-sm);
    font-weight: 500;
    margin: 0;
    line-height: 1.4;
    color: var(--text);
    display: -webkit-box;
    -webkit-line-clamp: 2;
    -webkit-box-orient: vertical;
    overflow: hidden;
}

.video-channel {
    font-size: var(--font-size-sm);
    color: var(--text-secondary);
    margin: 0;
}

.video-meta {
    display: flex;
    justify-content: space-between;
    font-size: var(--font-size-sm);
    color: var(--text-secondary);
    margin-top: auto;
}

/* ============================================
   Pagination
   ============================================ */

.pagination-top,
.pagination-bottom {
    display: flex;
    justify-content: center;
    align-items: center;
    gap: var(--spacing-md);
    padding: var(--spacing-lg) 0;
    flex-wrap: wrap;
}

.page-info {
    font-size: var(--font-size-sm);
    color: var(--text-secondary);
    white-space: nowrap;
}

/* ============================================
   Detail Header Controls
   ============================================ */

.detail-header {
    background-color: var(--background);
    border: 1px solid var(--border);
    border-radius: var(--radius-lg);
    padding: var(--spacing-lg);
    margin-bottom: var(--spacing-2xl);
    display: flex;
    justify-content: space-between;
    align-items: flex-start;
    gap: var(--spacing-lg);
    flex-wrap: wrap;
}

.detail-controls {
    flex: 1;
    min-width: 250px;
}

/* ============================================
   Messages & Alerts
   ============================================ */

.loading-spinner {
    text-align: center;
    padding: var(--spacing-2xl);
    color: var(--text-secondary);
    font-size: var(--font-size-lg);
}

.empty-state {
    text-align: center;
    padding: var(--spacing-2xl);
    color: var(--text-secondary);
    background-color: var(--background);
    border-radius: var(--radius-lg);
    border: 1px dashed var(--border);
}

.error-toast,
.success-toast {
    padding: var(--spacing-md) var(--spacing-lg);
    border-radius: var(--radius-md);
    margin-bottom: var(--spacing-md);
    display: flex;
    justify-content: space-between;
    align-items: center;
    cursor: pointer;
    animation: slideIn 0.3s ease-in-out;
    font-size: var(--font-size-sm);
}

.error-toast {
    background-color: #fee2e2;
    color: var(--danger);
    border: 1px solid #fecaca;
}

.success-toast {
    background-color: #dcfce7;
    color: var(--success);
    border: 1px solid #bbf7d0;
}

.toast-close {
    background: none;
    border: none;
    cursor: pointer;
    font-size: var(--font-size-lg);
    color: inherit;
    padding: 0;
}

.cache-info {
    background-color: #fef3c7;
    color: #92400e;
    padding: var(--spacing-md) var(--spacing-lg);
    border-radius: var(--radius-md);
    margin-bottom: var(--spacing-lg);
    border: 1px solid #fde68a;
    font-size: var(--font-size-sm);
}

/* ============================================
   Modal
   ============================================ */

.modal-overlay {
    position: fixed;
    top: 0;
    left: 0;
    right: 0;
    bottom: 0;
    background-color: rgba(0, 0, 0, 0.5);
    display: flex;
    align-items: center;
    justify-content: center;
    z-index: 1000;
    animation: fadeIn 0.2s ease-in-out;
}

.modal {
    background-color: var(--background);
    border-radius: var(--radius-lg);
    padding: var(--spacing-2xl);
    max-width: 500px;
    width: 90%;
    box-shadow: 0 20px 40px rgba(0, 0, 0, 0.2);
}

.modal h2 {
    margin-bottom: var(--spacing-lg);
    font-size: var(--font-size-2xl);
}

.modal-body {
    display: flex;
    flex-direction: column;
    gap: var(--spacing-md);
    margin-bottom: var(--spacing-lg);
}

.modal-actions {
    display: flex;
    gap: var(--spacing-md);
    justify-content: flex-end;
}

/* ============================================
   Animations
   ============================================ */

@keyframes slideIn {
    from {
        transform: translateY(-10px);
        opacity: 0;
    }
    to {
        transform: translateY(0);
        opacity: 1;
    }
}

@keyframes fadeIn {
    from {
        opacity: 0;
    }
    to {
        opacity: 1;
    }
}

/* ============================================
   Responsive Design
   ============================================ */

@media (max-width: 768px) {
    .main-content {
        padding: var(--spacing-lg) var(--spacing-md);
    }

    .view-header {
        flex-direction: column;
        align-items: flex-start;
        gap: var(--spacing-md);
    }

    .playlists-grid {
        grid-template-columns: 1fr;
    }

    .videos-grid {
        grid-template-columns: repeat(auto-fill, minmax(150px, 1fr));
        gap: var(--spacing-md);
    }

    .detail-header {
        flex-direction: column;
        align-items: stretch;
    }

    .btn-large {
        width: 100%;
        justify-content: center;
    }

    .header-content {
        flex-direction: column;
        align-items: flex-start;
        gap: var(--spacing-md);
    }

    .header-title {
        font-size: var(--font-size-xl);
    }
}

@media (max-width: 480px) {
    .btn {
        padding: var(--spacing-xs) var(--spacing-sm);
        font-size: var(--font-size-sm);
    }

    .videos-grid {
        grid-template-columns: repeat(auto-fill, minmax(120px, 1fr));
    }

    .modal {
        width: 95%;
    }
}

/* ============================================
   Dark Mode Support (future)
   ============================================ */

@media (prefers-color-scheme: dark) {
    :root {
        --background: #1f2937;
        --background-secondary: #111827;
        --text: #f3f4f6;
        --text-secondary: #d1d5db;
        --border: #374151;
    }
}
```

### 2. Strukturkrav
CSS-filen skal inneholde:

- [ ] CSS-variabler for farger, spacing, typografi
- [ ] Responsiv layout med CSS Grid og Flexbox
- [ ] Styling for alle komponenter (buttons, inputs, cards, modaler)
- [ ] Mobile-first design
- [ ] Hover-effekter og transitions
- [ ] Animasjoner (slideIn, fadeIn)
- [ ] Paginering-styling
- [ ] Modal-styling
- [ ] Toast-meldinger (error og success)
- [ ] Tilgjengelighets-fokus (kontrastforhold, font-størrelser)

## Akseptansekriterier
- [ ] `styles.css` finnes i `wwwroot/`
- [ ] Filen importeres korrekt i `index.html` med `<link rel="stylesheet" href="/styles.css">`
- [ ] Layouten er responsiv og fungerer på mobil, tablet og desktop
- [ ] Alle farger møter WCAG AA-kontrastkrav
- [ ] Buttons er klikkbare og har visuell feedback
- [ ] Animasjoner fungerer uten performanceproblemer
- [ ] Dark mode-support er forberedt (prefers-color-scheme)

## Referanser
- [Spesifikasjon: Frontend](youtube-kurator-v1-spec.md#8-frontend)
- [CSS Custom Properties (CSS Variables)](https://developer.mozilla.org/en-US/docs/Web/CSS/--*)
- [WCAG Contrast Requirements](https://www.w3.org/WAI/WCAG21/Understanding/contrast-minimum.html)
