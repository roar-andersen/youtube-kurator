# YouTube Kurator v2 - Implementering

## Oversikt

Dette direktoriet inneholder komplett spesifikasjon og oppgaver for v2-implementasjonen av YouTube Kurator.

**Status:** Fase 1 (oppgaver 01-03) er klar til implementering. Fase 2-3 oppgaver er skissert og vil bli utfylt under implementering.

## Filer

### Orkestrasjon
- **[orchestrator.md](orchestrator.md)** - Hovedprompt for orkestrator-agent. Les dette først for å forstå hvordan oppgavene skal kjøres.

### Oversikt
- **[tasks/00-overview.md](tasks/00-overview.md)** - Tabell over alle oppgaver, avhengigheter, og estimater.

### Fase 1: Kjerne (Sekvensiell) - 8-12 timer
**Må gjøres i rekkefølge. Starter etter at alle tasks er klare.**

1. **[tasks/01-database-schema.md](tasks/01-database-schema.md)** - ⭐ KLAR
   - Oppretter Users, VideoStatuses, WatchLater tabeller
   - Oppdaterer Playlist med nye kolonner (OwnerUserId, Filters, SortStrategy, osv.)
   - EF Core migrations
   - **Estimat:** 2-3 timer

2. **[tasks/02-user-entity-auth.md](tasks/02-user-entity-auth.md)** - ⭐ KLAR
   - Implementerer e-post + engangskode autentisering
   - JWT-token generering
   - SMTP e-post sending
   - POST /auth/start og /auth/verify endepunkter
   - **Estimat:** 4-6 timer

3. **[tasks/03-playlist-migration.md](tasks/03-playlist-migration.md)** - ⭐ KLAR
   - Migrerer alle v1 playlists til v2 skjema
   - Opprettelse av system-bruker
   - Konverterer SearchQuery → Filters JSON
   - Gradvis migrering (v1 API fortsetter å fungere)
   - **Estimat:** 2-3 timer

### Fase 2: Backend Features (Parallell) - 16-22 timer
**Kan kjøres samtidig av 5 agenter etter Fase 1 er ferdig.**

4. **[tasks/04-extended-filters-backend.md](tasks/04-extended-filters-backend.md)**
   - Filtrering av YouTube-videoer basert på JSON config
   - 7 filtertyper: temaer, varighet, språk, popularitet, kanaler, osv.
   - **Estimat:** 4-6 timer

5. **[tasks/05-sorting-strategies-backend.md](tasks/05-sorting-strategies-backend.md)**
   - 8 sorteringsstrategier
   - Nyest først, Mest populær, Høyest kvalitet, osv.
   - **Estimat:** 3-4 timer

6. **[tasks/06-video-status-api.md](tasks/06-video-status-api.md)**
   - VideoStatus API (POST /videos/{videoId}/status)
   - Duplikatbeskyttelse i refresh
   - Status: New, Seen, Saved, Rejected
   - **Estimat:** 3-4 timer

7. **[tasks/07-watch-later-api.md](tasks/07-watch-later-api.md)**
   - Watch Later API (GET, POST, DELETE /watchlater)
   - Global og playlist-spesifikk lister
   - **Estimat:** 2-3 timer

8. **[tasks/08-discovery-mode-backend.md](tasks/08-discovery-mode-backend.md)**
   - Oppdagelsesmodus: 70% strict, 20% relaxed, 10% wild cards
   - YouTube Related Videos API
   - Explanation-feltfor hvorfor video ble valgt
   - **Estimat:** 4-5 timer

### Fase 3: Frontend (Parallell) - 16-22 timer
**Kan kjøres samtidig av 6 agenter etter Fase 2 er ferdig.**

9. **[tasks/09-frontend-auth.md](tasks/09-frontend-auth.md)**
   - Login-skjerm (e-post + engangskode)
   - JWT-token lagring og sending
   - HTTP-interceptor for Authorization header
   - Logout-funksjonalitet
   - **Estimat:** 3-4 timer

10. **[tasks/10-frontend-filters.md](tasks/10-frontend-filters.md)**
    - Filter-konfigurasjon modal/UI
    - Alle 7 filtertyper
    - Lagre til Playlist.Filters JSON
    - **Estimat:** 4-5 timer

11. **[tasks/11-frontend-sorting.md](tasks/11-frontend-sorting.md)**
    - Dropdown for sorteringsvalg
    - 8 strategier
    - Save valg til backend
    - **Estimat:** 2-3 timer

12. **[tasks/12-frontend-video-status.md](tasks/12-frontend-video-status.md)**
    - Seen/Saved/Rejected-knapper på videokort
    - Visuell feedback
    - POST til backend
    - **Estimat:** 3-4 timer

13. **[tasks/13-frontend-watch-later.md](tasks/13-frontend-watch-later.md)**
    - Watch Later-knapp på videoer
    - Egen modal/side for watch later-liste
    - Global og playlist-spesifikk visning
    - **Estimat:** 2-3 timer

14. **[tasks/14-frontend-discovery.md](tasks/14-frontend-discovery.md)**
    - Discovery badge på videoer
    - Forklaring på tooltip
    - Discovery statistics
    - **Estimat:** 2-3 timer

### Fase 4: Testing & Deploy - 3-5 timer
- Kjør `dotnet test` for alle tester
- Manuell testing i nettleser
- Bygge Docker-image
- Deploy til Azure Container Apps

## Hvordan Starte

### For Orkestrator-agenten:
1. Les [orchestrator.md](orchestrator.md)
2. Kjør Fase 1 oppgaver sekvensielt (01 → 02 → 03)
3. Kjør Fase 2 oppgaver parallelt (04-08 samtidig)
4. Kjør Fase 3 oppgaver parallelt (09-14 samtidig)
5. Kjør testing og deploy

### For Subagenter:
1. Les din oppgavefil (f.eks. tasks/04-extended-filters-backend.md)
2. Implementer alt som står i "Implementering" seksjonen
3. Skriv tester som beskrevet
4. Sørg for at alle akseptansekriterier er oppfylt
5. Returner liste over nye og oppdaterte filer

## Viktige Prinsipper

✅ **DO:**
- Behold v1-API funksjonalitet (bakoverkompatibilitet)
- Skriv unit tests for hver oppgave
- Implementer autorisasjon (JWT-validering) på alle v2-API
- Håndter feil graceful med norske feilmeldinger
- Test manuelt i nettleser før gjort

❌ **DON'T:**
- Bryt eksisterende v1-API
- Skippe tester
- Legge til features som ikke står i oppgaven
- Over-engineer (keep it simple)
- Committe uten å teste først

## Prosjektstruktur

```
c:\src\youku\
├── src/YouTubeKurator.Api/
│   ├── Controllers/
│   ├── Services/
│   ├── Data/Entities/
│   ├── wwwroot/ (Frontend)
│   └── ...
├── YouTubeKurator.Tests/
├── spec/
│   ├── v1/
│   │   └── youtube-kurator-v1-spec.md
│   └── v2/
│       ├── orchestrator.md (du er her)
│       ├── tasks/
│       │   ├── 00-overview.md
│       │   ├── 01-database-schema.md
│       │   ├── ...
│       │   └── 14-frontend-discovery.md
│       └── README.md (du er her)
└── README.md (prosjekt root)
```

## Testing

Hver oppgave skal inkludere unit tests. Test-fil-lokasjon:
- Backend: `YouTubeKurator.Tests/[Category]/[FeatureName]Tests.cs`
- Frontend: `wwwroot/js/tests/` (eller lignende)

Kjør tests med:
```bash
dotnet test
```

## Deployment

Se [DEPLOYMENT.md](../DEPLOYMENT.md) for instruksjoner om Azure deployment.

## Kontakt

Hvis du har spørsmål om en oppgave:
1. Les oppgavefilen nøye (den skal være selvforklarende)
2. Sjekk related tasks for kontekst
3. Sjekk v1-kode for mønstre

## Status Tracking

- [ ] Fase 1 fullført (oppgaver 01-03)
- [ ] Fase 2 fullført (oppgaver 04-08)
- [ ] Fase 3 fullført (oppgaver 09-14)
- [ ] Alle tester passerer
- [ ] Manuell testing OK
- [ ] Deploy til Azure OK

---

**Lykke til med implementeringen!** 🚀
