# Spesifikasjon: Personlig YouTube-kurator v1

## 1. Formål

En personlig webapplikasjon for å lage temabaserte spillelister som automatisk henter videoer fra YouTube.

Brukeren oppretter spillelister med navn og søkeord, trykker refresh, og får en liste med relevante videoer sortert på nyeste først.

---

## 2. Teknologivalg

| Komponent | Teknologi |
|-----------|-----------|
| Frontend | Vanilla JS + Alpine.js |
| Backend | ASP.NET Core Web API |
| Database (lokal) | SQLite |
| Database (prod) | Azure SQL |
| Hosting | Azure Container Apps |
| Deployment | Dockerfile, `az containerapp up` |

Frontend og backend deployes i **samme container**. Backend serverer statiske filer fra `/wwwroot`.

---

## 3. Arkitektur

```
┌─────────────────────────────────────────┐
│         Azure Container Apps            │
│  ┌───────────────────────────────────┐  │
│  │      ASP.NET Core Web API         │  │
│  │  ┌─────────────┬───────────────┐  │  │
│  │  │  /wwwroot   │   /api        │  │  │
│  │  │  (Alpine.js │  (REST API)   │  │  │
│  │  │   PWA)      │               │  │  │
│  │  └─────────────┴───────────────┘  │  │
│  └───────────────────────────────────┘  │
└─────────────────────────────────────────┘
           │                    │
           │                    ▼
           │            ┌──────────────┐
           │            │  Azure SQL   │
           │            └──────────────┘
           ▼
    ┌──────────────┐
    │ YouTube API  │
    │    v3        │
    └──────────────┘
```

---

## 4. Brukermodell

**Ingen autentisering i v1.** Appen har én implisitt bruker. All data tilhører denne brukeren.

---

## 5. Datamodell

### Playlist

| Felt | Type | Beskrivelse |
|------|------|-------------|
| Id | GUID | Primærnøkkel |
| Name | string | Visningsnavn |
| SearchQuery | string | Fritekst søkeord |
| CreatedUtc | DateTime | Opprettelsestidspunkt |
| UpdatedUtc | DateTime | Sist endret |

### CachedSearch

| Felt | Type | Beskrivelse |
|------|------|-------------|
| Id | GUID | Primærnøkkel |
| SearchQuery | string | Søkeord (unik) |
| ResultsJson | string | Serialiserte videoresultater |
| FetchedUtc | DateTime | Hentetidspunkt |
| ExpiresUtc | DateTime | Utløpstidspunkt (FetchedUtc + 1 time) |

### Video (ikke lagret, kun DTO)

| Felt | Type |
|------|------|
| VideoId | string |
| Title | string |
| ChannelName | string |
| ThumbnailUrl | string |
| Duration | TimeSpan |
| PublishedAt | DateTime |
| ViewCount | long |

---

## 6. Forretningsregler

### Spillelister
- Bruker kan opprette, redigere og slette spillelister
- Hver spilleliste har et navn og ett søkeord (fritekst)
- Datamodellen skal være **utvidbar for flere filtre senere**, men kun søkeord implementeres i v1

### Refresh
- Refresh er **manuell** (bruker trykker knapp)
- Ved refresh hentes 50 videoer fra YouTube, sortert nyest først
- Resultatet erstatter forrige liste

### Caching
- Søkeresultater caches i **1 time**
- Cache-nøkkel er søkeordet
- Refresh innenfor cache-vindu returnerer cached resultat
- Flere spillelister med samme søkeord deler cache

### Feilhåndtering
- YouTube API utilgjengelig → Behold forrige liste + vis feilmelding
- YouTube-kvote oppbrukt → Vis tydelig, lettforståelig melding til bruker
- Ingen internett → Vis feilmelding (PWA krever nett)

---

## 7. API-endepunkter

### Spillelister

```
GET    /api/playlists           → Liste over alle spillelister
POST   /api/playlists           → Opprett spilleliste
GET    /api/playlists/{id}      → Hent én spilleliste
PUT    /api/playlists/{id}      → Oppdater spilleliste
DELETE /api/playlists/{id}      → Slett spilleliste
```

### Videoer

```
POST   /api/playlists/{id}/refresh  → Hent videoer (bruker cache hvis gyldig)
```

Response:
```json
{
  "videos": [...],
  "fromCache": true,
  "cacheExpiresUtc": "2025-01-15T14:30:00Z",
  "error": null
}
```

Ved feil:
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

---

## 8. Frontend

### Teknologi
- Vanilla JavaScript
- Alpine.js for reaktivitet
- Ingen byggesteg
- Ren CSS (ingen rammeverk)

### PWA
- Installerbar på mobil/desktop
- `manifest.json` med app-ikon og navn
- Service worker for installerbarhet
- **Krever internett** – viser feilmelding offline

### Sider/visninger

1. **Spilleliste-oversikt**
   - Liste over alle spillelister
   - Knapp for å opprette ny
   - Klikk på spilleliste åpner den

2. **Spilleliste-detalj**
   - Navn og søkeord (redigerbar)
   - Refresh-knapp
   - Slett-knapp
   - Video-liste

3. **Video-liste**
   - Viser 10-15 videoer om gangen
   - Enkel paginering (neste/forrige eller "last flere")
   - For hver video:
     - Thumbnail
     - Tittel
     - Kanalnavn
     - Varighet
     - Publiseringsdato
     - Antall visninger
   - Klikk på video åpner `https://youtu.be/{videoId}` i ny fane

### Feilmeldinger
- Nettverksfeil: "Kunne ikke koble til. Sjekk internettforbindelsen."
- Kvote oppbrukt: "YouTube-grensen er nådd for i dag. Du kan fortsatt se lagrede videoer. Prøv å oppdatere i morgen."
- Generell feil: "Noe gikk galt. Prøv igjen senere."

---

## 9. YouTube API-integrasjon

### Autentisering
- API-nøkkel (ikke OAuth)
- Nøkkel lagres i miljøvariabel/Azure Key Vault

### Endepunkt
- `search.list` med parametere:
  - `q`: søkeord
  - `type`: video
  - `order`: date (nyest først)
  - `maxResults`: 50
  - `part`: snippet

### Kvotehåndtering
- Gratiskvote: 10 000 enheter/dag
- Søk koster ~100 enheter
- Ved `quotaExceeded`-feil: returner cached resultat + feilmelding

---

## 10. Byggerekkefølge

Avhengighetsgraf for implementasjon:

```
1. Prosjektoppsett
   └── ASP.NET Core Web API
   └── EF Core + SQLite
   └── Dockerfile

2. Datalag
   └── Playlist-entitet
   └── CachedSearch-entitet
   └── DbContext
   └── Migrasjoner

3. API-endepunkter
   └── PlaylistsController (CRUD)
   └── Krever: Datalag

4. YouTube-integrasjon
   └── YouTubeService
   └── Caching-logikk
   └── Feilhåndtering
   └── Krever: Datalag

5. Refresh-endepunkt
   └── Kobler Playlist + YouTube + Cache
   └── Krever: YouTube-integrasjon

6. Frontend
   └── HTML + Alpine.js
   └── PWA-manifest + service worker
   └── Krever: API-endepunkter

7. Azure-deployment
   └── Azure SQL-database
   └── Container Apps
   └── Key Vault for API-nøkkel
   └── Krever: Alt over
```

---

## 11. Akseptansekriterier

v1 er ferdig når:

- [ ] Bruker kan opprette en spilleliste med navn og søkeord
- [ ] Bruker kan se liste over alle spillelister
- [ ] Bruker kan redigere navn og søkeord på en spilleliste
- [ ] Bruker kan slette en spilleliste
- [ ] Bruker kan trykke refresh og få 50 videoer sortert nyest først
- [ ] Videoer vises med thumbnail, tittel, kanal, varighet, dato, visninger
- [ ] Klikk på video åpner YouTube i ny fane/app
- [ ] Video-listen har paginering (10-15 per side)
- [ ] Søkeresultater caches i 1 time
- [ ] Ved YouTube-feil beholdes forrige liste + feilmelding vises
- [ ] Ved kvote-feil vises tydelig melding
- [ ] Appen er installerbar som PWA
- [ ] Offline vises feilmelding
- [ ] Appen kjører i Azure Container Apps

---

## 12. Utenfor v1-scope (bevisst utelatt)

- Brukerkontoer og autentisering
- Synkronisering på tvers av enheter
- Flere filtre (varighet, språk, kanaler, popularitet)
- Sorteringsvalg
- "Sett"/"Forkast"-markering
- Se på senere-funksjon
- Duplikatbeskyttelse på tvers av refreshes
- Offline-lesing av cached data
- Oppdagelsesmodus/villkort

---

## 13. Filstruktur (forslag)

```
/YouTubeKurator
├── src/
│   └── YouTubeKurator.Api/
│       ├── Controllers/
│       │   └── PlaylistsController.cs
│       ├── Services/
│       │   └── YouTubeService.cs
│       ├── Data/
│       │   ├── AppDbContext.cs
│       │   └── Entities/
│       │       ├── Playlist.cs
│       │       └── CachedSearch.cs
│       ├── wwwroot/
│       │   ├── index.html
│       │   ├── app.js
│       │   ├── styles.css
│       │   ├── manifest.json
│       │   └── sw.js
│       ├── Program.cs
│       ├── appsettings.json
│       └── Dockerfile
├── tests/
│   └── YouTubeKurator.Tests/
└── README.md
```

---

## 14. Miljøvariabler

| Variabel | Beskrivelse |
|----------|-------------|
| `YOUTUBE_API_KEY` | API-nøkkel for YouTube Data API v3 |
| `ConnectionStrings__DefaultConnection` | Database-tilkoblingsstreng |

---

## 15. Neste steg etter v1

Prioritert liste for v2:
1. Flere filtre (varighet, ekskluder Shorts, kanaler)
2. Markere videoer som sett (duplikatbeskyttelse)
3. Brukerkontoer (magic link)
4. Se på senere-funksjon
