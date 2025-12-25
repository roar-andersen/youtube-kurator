# Oppgave 8.1: Opprett manifest.json og ikon

## Fase
Fase 8: PWA-setup

## Avhengigheter
- Oppgave 6.1 (index.html må eksistere med manifest-lenke)

## Formål
Konfigurere PWA (Progressive Web App) manifest slik at appen kan installeres på mobil og desktop.

## Oppgavebeskrivelse

### 1. Opprett manifest.json
Lag fil `src/YouTubeKurator.Api/wwwroot/manifest.json`:

```json
{
  "name": "YouTube Kurator",
  "short_name": "Kurator",
  "description": "Personlig YouTube-kurator for temabaserte spillelister",
  "start_url": "/",
  "scope": "/",
  "display": "standalone",
  "orientation": "portrait-primary",
  "background_color": "#ffffff",
  "theme_color": "#1f2937",
  "icons": [
    {
      "src": "/icon-192.png",
      "sizes": "192x192",
      "type": "image/png",
      "purpose": "any"
    },
    {
      "src": "/icon-512.png",
      "sizes": "512x512",
      "type": "image/png",
      "purpose": "any"
    },
    {
      "src": "/icon-maskable-192.png",
      "sizes": "192x192",
      "type": "image/png",
      "purpose": "maskable"
    },
    {
      "src": "/icon-maskable-512.png",
      "sizes": "512x512",
      "type": "image/png",
      "purpose": "maskable"
    }
  ],
  "categories": ["productivity"],
  "screenshots": [
    {
      "src": "/screenshot-1.png",
      "sizes": "540x720",
      "type": "image/png",
      "form_factor": "narrow"
    },
    {
      "src": "/screenshot-2.png",
      "sizes": "1280x720",
      "type": "image/png",
      "form_factor": "wide"
    }
  ],
  "shortcuts": [
    {
      "name": "Mine Spillelister",
      "short_name": "Spillelister",
      "description": "Åpne dine spillelister",
      "url": "/?view=list",
      "icons": [
        {
          "src": "/icon-96.png",
          "sizes": "96x96"
        }
      ]
    }
  ]
}
```

**Forklaring av felt**:
- `name`: Fullt navn på appen (vises ved installering)
- `short_name`: Kort navn (vises på home screen)
- `description`: Kort beskrivelse
- `start_url`: URL som åpnes når appen starter
- `display`: `standalone` = fullskjerm-app-opplevelse (ikke som nettleser-tab)
- `theme_color`: Toolbar-farge
- `background_color`: Bakgrunnsfarge ved oppstart
- `icons`: Nødvendig for installering (minst 192x192 og 512x512)
- `categories`: App-kategori
- `screenshots`: Vises under installering (valgfritt, men anbefalt)
- `shortcuts`: Hurtiglenker (valgfritt)

### 2. Opprett ikoner
Opprett følgende PNG-ikoner i `wwwroot/`:

**Enkel metode**: Bruk en online PNG-generator eller designverktøy for å lage:
- `icon-192.png` (192x192)
- `icon-512.png` (512x512)
- `icon-96.png` (96x96) – for shortcuts
- `icon-maskable-192.png` (192x192, med transparens for adaptiv ikon)
- `icon-maskable-512.png` (512x512, med transparens for adaptiv ikon)

**Minimumskrav**: Minst `icon-192.png` og `icon-512.png`.

**Enkelt alternativ for testing**: Du kan opprett en enkel SVG og konvertere til PNG:

```svg
<!-- Lagre som icon.svg -->
<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 512 512">
  <rect width="512" height="512" fill="#1f2937" rx="100"/>
  <circle cx="256" cy="200" r="80" fill="#3b82f6"/>
  <circle cx="150" cy="350" r="60" fill="#3b82f6" opacity="0.8"/>
  <circle cx="362" cy="350" r="60" fill="#3b82f6" opacity="0.8"/>
</svg>
```

Konvertere SVG til PNG kan gjøres med:
- Online: https://convertio.co/svg-png/ eller lignende
- CLI: `svgexport icon.svg icon-512.png 512:512`
- ImageMagick: `convert -density 512 icon.svg -resize 512x512 icon-512.png`

### 3. Opprett screenshots (valgfritt, men anbefalt)
For at appen skal kunne installeres enda bedre, lag screenshots:
- `screenshot-1.png` (540x720, narrow/mobile)
- `screenshot-2.png` (1280x720, wide/desktop)

Disse kan være screenshots av appen eller mockups.

### 4. Verifiser manifest-lenke i index.html
I `index.html`, sikre at manifest er lenket:

```html
<link rel="manifest" href="/manifest.json">
<link rel="icon" type="image/svg+xml" href="/icon.svg">
<link rel="apple-touch-icon" href="/apple-touch-icon.png">
```

`apple-touch-icon` bør være 180x180 PNG.

### 5. Test PWA installerbarhet
Start applikasjonen og test:

1. Åpne Chrome DevTools (F12)
2. Gå til **Application** → **Manifest**
3. Verifiser at manifest laster og er gyldig
4. Klikk på installerings-ikonet i URL-feltet (eller meny → "Installer YouTube Kurator")
5. Verifiser at appen installeres og åpnes

**Desktop**:
- Windows: "Install app" i Chrome-meny
- macOS: "Install YouTube Kurator" i Chrome-meny

**Mobil (Android)**:
- Klikk installings-prompt eller meny → "Install app"
- Appen vises på home screen

### 6. Verifiser Web App Manifest validering
Bruk [Google's Web App Manifest Validator](https://web.dev/analyze/) for å sjekke om manifesten er gyldig.

## Akseptansekriterier
- [ ] `manifest.json` finnes i `wwwroot/`
- [ ] Manifest inneholder alle påkrevde felt (name, start_url, display, icons)
- [ ] Minst en ikon (192x192) finnes i `wwwroot/`
- [ ] Minst en ikon (512x512) finnes i `wwwroot/`
- [ ] `index.html` lenker til `manifest.json` med `<link rel="manifest" href="/manifest.json">`
- [ ] DevTools → Application → Manifest viser gyldig manifest uten feil
- [ ] Appen kan installeres fra Chrome-meny eller installerings-prompt
- [ ] Installert app vises med riktig navn og ikon

## Referanser
- [Spesifikasjon: PWA](youtube-kurator-v1-spec.md#pwa)
- [Web.dev: Web App Manifest](https://web.dev/add-manifest/)
- [MDN: Web App Manifest](https://developer.mozilla.org/en-US/docs/Web/Manifest)
- [PWA Checklist](https://web.dev/pwa-checklist/)
