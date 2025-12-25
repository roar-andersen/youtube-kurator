# Oppgave 12.1: Lag README og setup-instruksjoner

## Fase
Fase 12: Dokumentasjon

## Avhengigheter
- Alle forrige oppgaver (hele appen må være implementert)

## Formål
Dokumentere prosjektet slik at andre utviklere og brukere kan bruke applikasjonen.

## Oppgavebeskrivelse

### 1. Opprett README.md
Lag fil `README.md` i rotkatalogen:

```markdown
# YouTube Kurator – Personlig YouTube-kurator v1

En webapplikasjon for å lage temabaserte YouTube-spillelister som automatisk hentes og oppdateres fra YouTube.

## Features

- ✅ Opprett, rediger og slett spillelister
- ✅ Automatisk henting av 50 nyeste videoer fra YouTube
- ✅ Smart caching (1 time) for å spare YouTube API-kvota
- ✅ Responsiv design (mobil, tablet, desktop)
- ✅ Installabel PWA – fungerer offline (statiske sider)
- ✅ Sikker lagring av API-nøkkler i Azure Key Vault
- ✅ Deployer direkte til Azure Container Apps

## Tech Stack

| Component | Technology |
|-----------|-----------|
| Frontend | Vanilla JS + Alpine.js + PWA |
| Backend | ASP.NET Core 8.0 Web API |
| Database | SQLite (dev) / Azure SQL (prod) |
| Hosting | Azure Container Apps |
| Caching | In-Memory + Database |

## Getting Started

### Lokalt Setup

#### Forutsetninger
- .NET 8.0 SDK eller nyere
- Git
- YouTube Data API v3 API-nøkkel (fra [Google Cloud Console](https://console.cloud.google.com/))

#### Installering

1. **Klon prosjektet**
```bash
git clone https://github.com/ditt-brukernavn/youtube-kurator.git
cd youtube-kurator
```

2. **Installer avhengigheter**
```bash
cd src/YouTubeKurator.Api
dotnet restore
```

3. **Konfigurer miljøvariabler**
Opprett `appsettings.Development.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=youtube-kurator.db"
  },
  "YouTubeApi": {
    "ApiKey": "din-youtube-api-nøkkel-her"
  }
}
```

4. **Opprett database**
```bash
dotnet ef database update
```

5. **Start applikasjonen**
```bash
dotnet run
```

Besøk `http://localhost:5000` i nettleseren.

### Hvordan bruke

1. **Opprett spilleliste**
   - Klikk "+ Ny Spilleliste"
   - Gi den ett navn og søkeord
   - Klikk "Opprett"

2. **Oppdater videoer**
   - Klikk på en spilleliste
   - Klikk "Oppdater Videoer"
   - 50 nyeste videoer fra YouTube vil vises

3. **Browse videoer**
   - Scroll gjennom videoene
   - Klikk på en video for å åpne på YouTube
   - Bruk paginering for å se flere

4. **Installer som app**
   - Åpne Chrome-meny
   - Velg "Install YouTube Kurator"
   - Appen installeres på home screen
   - Åpne fra app-ikonet

## API-dokumentasjon

### Playlists

```
GET    /api/playlists              → Liste over alle spillelister
POST   /api/playlists              → Opprett ny spilleliste
GET    /api/playlists/{id}         → Hent en spilleliste
PUT    /api/playlists/{id}         → Oppdater spilleliste
DELETE /api/playlists/{id}         → Slett spilleliste
POST   /api/playlists/{id}/refresh → Hent og cache videoer
```

### Eksempel: Opprett playlist
```bash
curl -X POST http://localhost:5000/api/playlists \
  -H "Content-Type: application/json" \
  -d '{"name":"Musikk","searchQuery":"best songs 2025"}'
```

Respons:
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "name": "Musikk",
  "searchQuery": "best songs 2025",
  "createdUtc": "2025-01-15T10:00:00Z",
  "updatedUtc": "2025-01-15T10:00:00Z"
}
```

### Eksempel: Refresh videoer
```bash
curl -X POST http://localhost:5000/api/playlists/550e8400-e29b-41d4-a716-446655440000/refresh
```

Respons:
```json
{
  "videos": [
    {
      "videoId": "dQw4w9WgXcQ",
      "title": "Rick Astley – Never Gonna Give You Up",
      "channelName": "Rick Astley Official",
      "thumbnailUrl": "https://...",
      "duration": "00:03:33",
      "publishedAt": "2025-01-14T15:00:00Z",
      "viewCount": 1234567890
    },
    ...
  ],
  "fromCache": false,
  "cacheExpiresUtc": "2025-01-15T11:00:00Z",
  "error": null
}
```

## Miljøvariabler

| Variabel | Beskrivelse | Eksempel |
|----------|-------------|---------|
| `YOUTUBE_API_KEY` | YouTube Data API v3 nøkkel | `AIza...` |
| `ConnectionStrings__DefaultConnection` | Database-tilkoblings-streng | `Server=tcp:...` |

## Testing

### Unit Tests
```bash
dotnet test YouTubeKurator.Tests
```

### Manuell Testing
Se [test-checklist](spec/task-10-2-manuell-testing.md)

## Deployment til Azure

### Forutsetninger
- Azure-abonnement
- Azure CLI installert
- Docker installert

### Steps

1. **Opprett Azure-ressurser**
```bash
./scripts/setup-azure.sh  # eller kjør komandoene manuelt
```

2. **Bygg og push Docker-image**
```bash
docker build -t youtubekuratoracr.azurecr.io/youtube-kurator:latest .
docker push youtubekuratoracr.azurecr.io/youtube-kurator:latest
```

3. **Deploy til Container Apps**
```bash
az containerapp update \
  --name youtube-kurator \
  --resource-group youtube-kurator-rg \
  --image youtubekuratoracr.azurecr.io/youtube-kurator:latest
```

## Arkitektur

```
┌─────────────────────────────────────────┐
│      Azure Container Apps               │
│  ┌───────────────────────────────────┐  │
│  │   ASP.NET Core Web API            │  │
│  │   /wwwroot  (Alpine.js PWA)       │  │
│  │   /api      (REST API)            │  │
│  └───────────────────────────────────┘  │
└──────────────┬──────────────────────────┘
               │
    ┌──────────┴──────────┐
    ▼                     ▼
┌──────────┐      ┌──────────────┐
│YouTube   │      │Azure SQL     │
│API v3    │      │Database      │
└──────────┘      └──────────────┘
```

## Filer & Struktur

```
youtube-kurator/
├── src/
│   └── YouTubeKurator.Api/
│       ├── Controllers/        # API-kontrollere
│       ├── Services/           # Business logic
│       ├── Data/               # Database (DbContext, entities)
│       ├── wwwroot/            # Frontend (HTML, CSS, JS, ikon)
│       ├── Program.cs          # App-oppsett
│       ├── appsettings.json    # Config
│       └── Dockerfile          # Container-config
├── tests/
│   └── YouTubeKurator.Tests/   # Unit tests
├── spec/
│   └── *.md                    # Spesifikasjoner & task-filer
├── README.md                   # Denne filen
└── .gitignore
```

## Troubleshooting

### "YOUTUBE_API_KEY er ikke konfigurert"
Sikre at `appsettings.Development.json` inneholder en gyldig API-nøkkel.

### "Database-filen ikke funnet"
Kjør: `dotnet ef database update`

### "Service Worker registrering feiler"
Sikre at appen kjører på HTTPS eller localhost (requirement for SW).

### "YouTube-kvoten er oppbrukt"
API-nøkkelen din har nådd dags grense. Vent til neste dag eller oppgrader til betalt plan.

## Kontakt & Support

- GitHub Issues: [Link til issues]
- Dokumentasjon: [Link til wiki]

## Lisens

MIT License – Se [LICENSE](LICENSE) filen for detaljer.

## Bidrag

Bidrag er velkomne! Vennligst:
1. Fork prosjektet
2. Opprett en feature-branch
3. Commit endringene
4. Push og lag en Pull Request

---

**Versjon**: 1.0
**Sist oppdatert**: 2025-01-15
**Status**: Produksjon-klar ✅
```

### 2. Opprett CONTRIBUTING.md (valgfritt)
Lag fil `CONTRIBUTING.md` for å dokumentere hvordan man bidrar:

```markdown
# Bidragsretningslinjer

## Før du starter

1. Fork prosjektet
2. Opprett en branch: `git checkout -b feature/min-feature`
3. Gjør endringer og test
4. Commit med beskrivende melding: `git commit -m "Add feature X"`
5. Push og lag Pull Request

## Kodestil

- Follow [C# Coding Conventions](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- JavaScript: Vanlig ES6, med 2-spacers indenting
- CSS: BEM-naming convention

## Testing

- Alle nye features skal ha unit tests
- Kjør `dotnet test` før du pusher
- Gjennomfør manuell testing (se task-10-2-manuell-testing.md)

## Commits

- Kortfattet og beskrivende
- Imperativ form ("Add feature" ikke "Added feature")
- Referans issue nummer hvis relevant: "Fix #123"
```

### 3. Opprett DEPLOYMENT.md (valgfritt)
Lag fil `DEPLOYMENT.md` for detaljert deployment-dokumentasjon:

```markdown
# Deployment Guide

Se oppgaver i `spec/` mappen:
- [task-11-1-konfigurer-azure-sql.md](spec/task-11-1-konfigurer-azure-sql.md) – SQL Setup
- [task-11-2-konfigurer-container-apps.md](spec/task-11-2-konfigurer-container-apps.md) – Container Apps
- [task-11-3-bygge-og-deploye-container.md](spec/task-11-3-bygge-og-deploye-container.md) – Build & Deploy
```

## Akseptansekriterier
- [ ] `README.md` opprettet med fullstendig dokumentasjon
- [ ] Installation-instruksjoner er klare og testa
- [ ] API-dokumentasjon inneholder eksempler
- [ ] Arkitektur-diagram finnes
- [ ] Troubleshooting-seksjon dekker vanlige problemer
- [ ] `CONTRIBUTING.md` opprettet (valgfritt)
- [ ] `DEPLOYMENT.md` opprettet (valgfritt)

## Referanser
- [Spesifikasjon](youtube-kurator-v1-spec.md)
- [GitHub README Best Practices](https://docs.github.com/en/repositories/managing-your-repositorys-settings-and-features/customizing-your-repository/about-readmes)
