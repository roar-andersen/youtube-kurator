# YouTube Kurator v2 - Oppgaveoversikt

## Formål
Dette dokumentet gir en oversikt over alle oppgaver for v2-implementasjonen og deres avhengigheter.

## Prosjektstruktur

**Rot-mappe**: `c:\src\youku\`

**Viktige mapper:**
- `src/YouTubeKurator.Api/` - Backend API-prosjekt
- `src/YouTubeKurator.Api/wwwroot/` - Frontend (PWA)
- `src/YouTubeKurator.Api/Data/` - Database entities og DbContext
- `src/YouTubeKurator.Api/Controllers/` - API controllers
- `src/YouTubeKurator.Api/Services/` - Business logic services
- `YouTubeKurator.Tests/` - Unit tests
- `spec/v1/` - v1 spesifikasjon
- `spec/v2/` - v2 spesifikasjon og oppgaver

## Oppgaveoversikt

| # | Oppgave | Fase | Avhenger av | Estimat |
|---|---------|------|-------------|---------|
| 01 | Database schema | 1 | - | 2-3t |
| 02 | User entity + auth | 1 | 01 | 4-6t |
| 03 | Playlist migration | 1 | 01, 02 | 2-3t |
| 04 | Extended filters (BE) | 2 | 03 | 4-6t |
| 05 | Sorting strategies (BE) | 2 | 03 | 3-4t |
| 06 | VideoStatus API | 2 | 03 | 3-4t |
| 07 | WatchLater API | 2 | 03 | 2-3t |
| 08 | Discovery mode (BE) | 2 | 04, 05 | 4-5t |
| 09 | Frontend auth | 3 | 02 | 3-4t |
| 10 | Frontend filters | 3 | 04 | 4-5t |
| 11 | Frontend sorting | 3 | 05 | 2-3t |
| 12 | Frontend video status | 3 | 06 | 3-4t |
| 13 | Frontend watch later | 3 | 07 | 2-3t |
| 14 | Frontend discovery | 3 | 08 | 2-3t |

**Total estimert tid**: 45-60 timer

## Faseplan

### Fase 1: Kjerne (sekvensiell) - 8-12t
Database, autentisering, migrering. Må gjøres i rekkefølge.

### Fase 2: Backend (parallell) - 16-22t
Alle backend features kan utvikles samtidig av 5 agenter.

### Fase 3: Frontend (parallell) - 16-22t
Alle frontend features kan utvikles samtidig av 6 agenter.

### Fase 4: Integrasjon og testing - 3-5t
Testing, bugfixes, deploy.

## Viktige prinsipper

1. **Bakoverkompatibilitet**: Backend må støtte både v1 og v2 API-kall under migreringen
2. **Database-migrering**: Eksisterende v1-data må bevares og migreres
3. **Testing**: Hver oppgave skal inkludere unit tests
4. **Norwegian UI**: Alle feilmeldinger og UI-tekster på norsk
5. **PWA**: Behold PWA-funksjonalitet
6. **Azure-ready**: Skal kunne deployes til Azure Container Apps

## Teknisk stack (uendret fra v1)

- Backend: ASP.NET Core 10.0 Web API
- Frontend: Vanilla JS + Alpine.js 3.x
- Database: SQLite (dev) / Azure SQL (prod)
- ORM: Entity Framework Core 10.0
- External API: YouTube Data API v3
- Deployment: Docker + Azure Container Apps

## Neste steg

Les `../orchestrator.md` for instruksjoner om hvordan oppgavene skal kjøres.
