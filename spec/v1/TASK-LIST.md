# YouTube-kurator v1 – Komplett Oppgaveliste

Dette dokumentet lister alle implementerings-oppgaver for YouTube-kurator v1. Hver oppgave er en selvstendig fil som kan tilordnes til en agent/utvikler.

## Fase 1: Prosjektoppsett

- [x] **Task 1.1** – [Opprett ASP.NET Core-prosjekt og grunnleggende struktur](task-1-1-opprett-asp-net-core-prosjekt.md)
  - Opprett ASP.NET Core Web API
  - Mappestruktur
  - Program.cs-konfigurering
  - appsettings.json
  - NuGet-pakker

- [x] **Task 1.2** – [Opprett Dockerfile og container-konfigurasjon](task-1-2-opprett-dockerfile.md)
  - Multisteg Docker-build
  - .dockerignore
  - Verifiser bygning og kjøring

## Fase 2: Datamodell

- [x] **Task 2.1** – [Implementer Playlist-, CachedSearch- og Video-entiteter](task-2-1-implementer-entiteter.md)
  - Playlist.cs
  - CachedSearch.cs
  - Video.cs (DTO)
  - Dataannotasjoner

- [x] **Task 2.2** – [Opprett AppDbContext og EF Core-konfigurasjon](task-2-2-opprett-dbcontext.md)
  - DbContext-klasse
  - DbSets
  - Model configuration
  - Program.cs-registrering

- [x] **Task 2.3** – [Lag og eksekver initiell databasemigrasjoning](task-2-3-opprett-migrasjoner.md)
  - `dotnet ef migrations add InitialCreate`
  - `dotnet ef database update`
  - Verifiser tabell-struktur

## Fase 3: API-endepunkter

- [x] **Task 3.1** – [Implementer PlaylistsController med CRUD](task-3-1-implementer-playlists-controller.md)
  - GET /api/playlists
  - POST /api/playlists
  - GET /api/playlists/{id}
  - PUT /api/playlists/{id}
  - DELETE /api/playlists/{id}
  - Request/Response-klasser
  - Error handling

## Fase 4: YouTube-integrasjon

- [x] **Task 4.1** – [Implementer YouTubeService for API-kommunikasjon](task-4-1-implementer-youtube-service.md)
  - Google.Apis.YouTube NuGet-pakke
  - SearchVideosAsync-metode
  - Parse YouTube-respons
  - Error-håndtering (Quota, Network, Generic)

- [x] **Task 4.2** – [Implementer caching-logikk for søkeresultater](task-4-2-implementer-caching.md)
  - CacheService
  - GetOrFetchVideosAsync
  - 1-times cache-expiry
  - Fallback til cache ved error
  - InvalidateCache og CleanupExpiredCache
  - RefreshResponse DTO

## Fase 5: Refresh-endepunkt

- [x] **Task 5.1** – [Implementer POST /api/playlists/{id}/refresh](task-5-1-implementer-refresh-endepunkt.md)
  - Refresh-endepunkt i controller
  - Injiser CacheService
  - Hent playlist og søkeord
  - Returner respons med cache-info

## Fase 6: Frontend – HTML og CSS

- [x] **Task 6.1** – [Opprett index.html med basissider](task-6-1-opprett-index-html.md)
  - HTML5-struktur
  - PWA-metatags
  - Header og navigasjon
  - Playlist-list view
  - Playlist-detail view
  - Video-grid
  - Modaler
  - Alpine.js-direktiver

- [x] **Task 6.2** – [Opprett CSS-styling](task-6-2-opprett-styles.md)
  - CSS-variabler
  - Responsive layout
  - Mobile-first design
  - Alle komponenter
  - Dark mode support
  - Animasjoner

## Fase 7: Frontend – JavaScript og Alpine.js

- [x] **Task 7.1** – [Implementer Alpine.js-app for UI-logikk](task-7-1-implementer-alpine-js.md)
  - appState()-funksjon
  - loadPlaylists()
  - createPlaylist()
  - selectPlaylist()
  - savePlaylist()
  - deletePlaylist()
  - refreshPlaylist()
  - openVideo()
  - Paginering
  - Formatters (date, duration, views)

- [x] **Task 7.2** – [Implementer navigasjon mellom oversikt og detalj](task-7-2-implementer-navigasjon.md)
  - showView()-funksjon
  - View-toggling
  - Breadcrumb-navigasjon
  - State-reset ved view-endring
  - localStorage (valgfritt)

## Fase 8: PWA-setup

- [x] **Task 8.1** – [Opprett manifest.json og ikon](task-8-1-opprett-pwa-manifest.md)
  - manifest.json
  - Ikoner (192x192, 512x512)
  - Screenshots (valgfritt)
  - Apple-touch-icon
  - Verifiser installerbarhet

- [x] **Task 8.2** – [Implementer service worker](task-8-2-opprett-service-worker.md)
  - sw.js
  - Install event (cache static assets)
  - Activate event (clean old caches)
  - Fetch event (network/cache strategy)
  - Service Worker-registrering
  - Offline-støtte

## Fase 9: Feilmeldinger og brukeropplevelse

- [x] **Task 9.1** – [Implementer brukervenlige feilmeldinger](task-9-1-implementer-feilmeldinger.md)
  - Error-state
  - Error-toast-visning
  - Brukervennlige norske meldinger
  - AUTO-dismiss (5 sekunder)
  - Backend error-håndtering

- [x] **Task 9.2** – [Implementer loading-indikatorer](task-9-2-implementer-loading-indikatorer.md)
  - isLoading og isLoadingVideos states
  - Loading-spinners
  - Button-disabling under last
  - CSS-animasjoner
  - Cache-info-display

## Fase 10: Testing og validering

- [x] **Task 10.1** – [Skrive enhetstester for backend-services](task-10-1-skrive-tester.md)
  - YouTubeServiceTests
  - CacheServiceTests
  - PlaylistsControllerTests
  - xUnit + Moq
  - In-memory database

- [x] **Task 10.2** – [Manuell end-to-end testing](task-10-2-manuell-testing.md)
  - Test-sjekkliste (17 kategorier)
  - Alle features
  - Error-scenarier
  - PWA
  - Responsivitet
  - Performance
  - Accessibility

## Fase 11: Azure-deployment

- [x] **Task 11.1** – [Konfigurere Azure SQL Database](task-11-1-konfigurer-azure-sql.md)
  - Resource Group
  - Azure SQL Server og Database
  - Firewall
  - Key Vault
  - Secrets
  - Migrasjoner mot Azure SQL

- [x] **Task 11.2** – [Konfigurere Azure Container Apps og Key Vault](task-11-2-konfigurer-container-apps.md)
  - Container Registry (ACR)
  - Container Apps-miljø
  - Managed Identity
  - Key Vault-access
  - Container App
  - Environment-variabler
  - Auto-scaling

- [x] **Task 11.3** – [Bygge og deploye container](task-11-3-bygge-og-deploye-container.md)
  - Lokal Docker-test
  - Push til ACR
  - Deploy til Container Apps
  - Verifiser produksjon
  - Logs og debugging
  - CD-pipeline (GitHub Actions)

## Fase 12: Dokumentasjon

- [x] **Task 12.1** – [Lag README og setup-instruksjoner](task-12-1-lag-dokumentasjon.md)
  - README.md (fullstendig)
  - Lokalt setup
  - API-dokumentasjon
  - Deployment-guide
  - Arkitektur
  - Troubleshooting
  - CONTRIBUTING.md (valgfritt)
  - DEPLOYMENT.md (valgfritt)

---

## Oppgaver per Fase

| Fase | Oppgaver | Status |
|------|----------|--------|
| 1. Prosjektoppsett | 1.1, 1.2 | ✅ |
| 2. Datamodell | 2.1, 2.2, 2.3 | ✅ |
| 3. API-endepunkter | 3.1 | ✅ |
| 4. YouTube-integrasjon | 4.1, 4.2 | ✅ |
| 5. Refresh-endepunkt | 5.1 | ✅ |
| 6. Frontend HTML/CSS | 6.1, 6.2 | ✅ |
| 7. Frontend JS | 7.1, 7.2 | ✅ |
| 8. PWA | 8.1, 8.2 | ✅ |
| 9. Feilhåndtering | 9.1, 9.2 | ✅ |
| 10. Testing | 10.1, 10.2 | ✅ |
| 11. Azure Deployment | 11.1, 11.2, 11.3 | ✅ |
| 12. Dokumentasjon | 12.1 | ✅ |

**Totalt: 22 oppgaver**

---

## Parallelliserings-guide

Oppgaver som **kan kjøres parallelt** (ingen avhengigheter):

### Fase 1
- Task 1.1 + Task 1.2

### Fase 2
- Task 2.1 + Task 2.2 (Task 2.3 avhenger av begge)

### Fase 3 & 4
- Task 3.1 + Task 4.1 + Task 4.2 (parallelt, Task 5.1 avhenger av både 3 og 4)

### Fase 6 & 7 & 8
- Task 6.1 + Task 6.2 (parallelt)
- Task 7.1 + Task 7.2 (parallelt, avhenger av 6)
- Task 8.1 + Task 8.2 (parallelt, avhenger av 6.1)

### Fase 9
- Task 9.1 + Task 9.2 (parallelt, avhenger av 7)

### Fase 10
- Task 10.1 + Task 10.2 (parallelt, avhenger av alt)

### Fase 11
- Task 11.1 (avhenger av 2.3)
- Task 11.2 (avhenger av 11.1)
- Task 11.3 (avhenger av 11.2 + hele appen)

### Fase 12
- Task 12.1 (avhenger av alt)

**Optimal sekvensiering for agenter:**

```
Gruppe 1: 1.1 + 1.2 + 2.1 + 2.2
Gruppe 2: 2.3 (avhenger av Gruppe 1) + 3.1 + 4.1 + 4.2
Gruppe 3: 5.1 (avhenger av Gruppe 2) + 6.1 + 6.2
Gruppe 4: 7.1 + 7.2 + 8.1 + 8.2 (avhenger av Gruppe 3)
Gruppe 5: 9.1 + 9.2 (avhenger av Gruppe 4)
Gruppe 6: 10.1 + 10.2 (avhenger av alt)
Gruppe 7: 11.1 (avhenger av 2.3)
Gruppe 8: 11.2 (avhenger av 11.1)
Gruppe 9: 11.3 (avhenger av 11.2)
Gruppe 10: 12.1 (avhenger av alt)
```

**Estimert tidsbruk per oppgave:** 1-3 timer (avhenger av erfaring)

---

## Hvordan bruke denne oppgavelisten

1. **For agenter**: Velg en oppgave fra listen og les hele oppgavefilen
2. **For project manager**: Tilordne oppgaver basert på parallelliserings-guiden
3. **For tracking**: Marker oppgaver som `in_progress` eller `completed` etter hvert som de utføres
4. **For debug**: Hvis en oppgave blokkeres, sjekk "Avhengigheter"-seksjonen

---

Sist oppdatert: 2025-01-15
