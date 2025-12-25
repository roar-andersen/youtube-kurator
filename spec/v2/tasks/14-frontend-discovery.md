# Oppgave 14: Frontend - Oppdagelsesmodus Indikatorer

## Mål
Implementere visuell indikator for hvorfor videoer ble inkludert via oppdagelsesmodus.

## Kontekst

**Avhenger av:** Oppgave 08 (Discovery mode backend)

**Krav:**
- Vise indikator når video er fra "oppdagelse" (ikke streng match)
- Forklare hvorfor video ble valgt
- Tooltip/popup med detaljer

## Implementation

### 1. Discovery Indicator Badge

Legg til badge på videokort hvis `video.discoveryReason` er satt:

```html
<div class="video-card">
  <img :src="video.thumbnailUrl" />

  <template x-if="video.discoveryReason">
    <div class="discovery-badge" :title="video.discoveryReason">
      🔍 Oppdagelse
    </div>
  </template>

  <!-- ... rest of card ... -->
</div>
```

### 2. Discovery Explanation Map

```javascript
const discoveryExplanations = {
  'sametheme_otherlanguage': '📍 Samme tema, annet språk',
  'samechannel_otherformat': '📺 Samme kanal, annet format',
  'lowpop_highquality': '⭐ Lav popularitet, høy kvalitet',
  'relatedchannels': '🔗 Relatert kanal',
  'strict_match': 'Strengt match'
};
```

### 3. Tooltip/Popup

Legg til hover-effekt som viser full forklaring:

```css
.discovery-badge {
  position: absolute;
  top: 8px;
  right: 8px;
  background: rgba(0, 123, 255, 0.9);
  color: white;
  padding: 4px 8px;
  border-radius: 4px;
  font-size: 11px;
  cursor: help;
  transition: all 0.2s;
}

.discovery-badge:hover {
  background: rgba(0, 123, 255, 1);
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.2);
}

.discovery-badge::after {
  content: attr(title);
  position: absolute;
  bottom: 120%;
  left: 0;
  right: 0;
  background: #333;
  color: white;
  padding: 8px;
  border-radius: 4px;
  font-size: 12px;
  white-space: nowrap;
  z-index: 1000;
  opacity: 0;
  pointer-events: none;
  transition: opacity 0.2s;
}

.discovery-badge:hover::after {
  opacity: 1;
}
```

### 4. Discovery Profile Indicator

I playlist-detalj, vise om Discovery Mode er aktivert:

```html
<div class="playlist-info">
  <h2>{{ playlist.name }}</h2>

  <template x-if="playlist.discoveryProfile">
    <div class="discovery-profile-info">
      <p>🔍 Oppdagelsesmodus aktivert</p>
      <p class="description">
        Blanding av streng matching og villkort for å finne nye favoritter
      </p>
    </div>
  </template>

  <!-- ... rest of info ... -->
</div>
```

### 5. Statistics

Valgfritt: Vis antall videoer fra hver kategori:

```javascript
Alpine.data('playlist', () => ({
  getDiscoveryStats() {
    return {
      strict: this.videos.filter(v => !v.discoveryReason).length,
      relaxed: this.videos.filter(v => v.discoveryReason?.includes('other')).length,
      wild: this.videos.filter(v => v.discoveryReason?.includes('related')).length
    };
  }
}))
```

Display:
```html
<template x-if="playlist.discoveryProfile">
  <div class="discovery-stats">
    <div class="stat">
      <strong>Strengt:</strong>
      <span x-text="getDiscoveryStats().strict"></span>
    </div>
    <div class="stat">
      <strong>Oppdagelse:</strong>
      <span x-text="getDiscoveryStats().relaxed + getDiscoveryStats().wild"></span>
    </div>
  </div>
</template>
```

### 6. Styling

```css
.discovery-badge {
  display: inline-block;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  padding: 4px 8px;
  border-radius: 12px;
  font-size: 11px;
  color: white;
  margin: 4px 0;
}

.discovery-profile-info {
  background: #f0f4ff;
  border-left: 4px solid #667eea;
  padding: 12px;
  margin: 12px 0;
  border-radius: 4px;
}

@media (prefers-color-scheme: dark) {
  .discovery-profile-info {
    background: #1a1a2e;
    border-color: #667eea;
  }
}
```

### 7. Documentation

I app.js eller separatfil, dokumenter discovery reasons:

```javascript
const DISCOVERY_REASONS = {
  SAME_THEME_OTHER_LANGUAGE: 'sametheme_otherlanguage',
  SAME_CHANNEL_OTHER_FORMAT: 'samechannel_otherformat',
  LOW_POP_HIGH_QUALITY: 'lowpop_highquality',
  RELATED_CHANNELS: 'relatedchannels',
  STRICT_MATCH: null // No reason = strict match
};
```

## Akseptansekriterier

- [ ] Discovery badge vises når `discoveryReason` finnes
- [ ] Badge har visuell distinksjon
- [ ] Tooltip/hover viser full forklaring
- [ ] Discovery profile-info vises i detalj
- [ ] Antall videoer fra hver kategori kan sees (valgfritt)
- [ ] Responsive design
- [ ] Dark mode support
- [ ] Accessible (aria-labels, etc.)

## Leveranse

Oppdaterte filer:
- `wwwroot/index.html` (Discovery badge HTML)
- `wwwroot/app.js` (Discovery constants og logic)
- `wwwroot/styles.css` (Discovery styling)
