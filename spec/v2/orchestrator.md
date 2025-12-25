# YouTube Kurator v2 - Orkestrator-agent

## Din rolle
Du er orkestrator-agenten som koordinerer implementasjonen av YouTube Kurator v2. Du skal starte og koordinere subagenter som utfører selvstendige oppgaver.

## Prosjektkontekst

**Eksisterende v1-implementasjon:**
- Monolittisk ASP.NET Core Web API + embedded PWA frontend
- SQLite database med Playlist og CachedSearch tabeller
- Ingen autentisering (én implisitt bruker)
- Enkel søkefunksjonalitet (SearchQuery-felt)
- Fast sortering (nyeste først)
- Ingen video-historikk eller duplikatbeskyttelse

**v2-mål:**
- Multi-user med e-postbasert autentisering (magic link)
- JWT-tokens
- Utvidede filtre (JSON-basert)
- Flere sorteringsstrategier
- Video-historikk og duplikatbeskyttelse (VideoStatus)
- Watch Later-funksjonalitet
- Oppdagelsesmodus (discovery profile)

## Oppgavestruktur

Oppgavene er organisert i 3 faser:

### Fase 1: Kjerne (SEKVENSIELL - må gjøres i rekkefølge)
1. `tasks/01-database-schema.md` - Opprette nye tabeller og migrere eksisterende
2. `tasks/02-user-entity-auth.md` - Implementere User entity og autentisering
3. `tasks/03-playlist-migration.md` - Migrere eksisterende v1-data til v2-skjema

### Fase 2: Backend features (PARALLELL - kan kjøres samtidig)
4. `tasks/04-extended-filters-backend.md` - Implementere filter-logikk
5. `tasks/05-sorting-strategies-backend.md` - Implementere sorteringsstrategier
6. `tasks/06-video-status-api.md` - VideoStatus API og duplikatbeskyttelse
7. `tasks/07-watch-later-api.md` - Watch Later API
8. `tasks/08-discovery-mode-backend.md` - Oppdagelsesmodus backend

### Fase 3: Frontend (PARALLELL - kan kjøres samtidig)
9. `tasks/09-frontend-auth.md` - Login UI og token-håndtering
10. `tasks/10-frontend-filters.md` - Filter-konfigurasjon UI
11. `tasks/11-frontend-sorting.md` - Sorteringsvalg UI
12. `tasks/12-frontend-video-status.md` - Video-status UI (Seen/Saved/Rejected)
13. `tasks/13-frontend-watch-later.md` - Watch Later UI
14. `tasks/14-frontend-discovery.md` - Oppdagelsesmodus indikatorer

## Hvordan kjøre implementasjonen

### Steg 1: Kjør Fase 1 sekvensielt
Start en Task-agent for hver oppgave i Fase 1, vent på fullføring før neste:

```
Task 1: Implement database schema from tasks/01-database-schema.md
  → Vent til fullført
Task 2: Implement user entity and auth from tasks/02-user-entity-auth.md
  → Vent til fullført
Task 3: Migrate v1 data from tasks/03-playlist-migration.md
  → Vent til fullført
```

### Steg 2: Kjør Fase 2 parallelt
Start ALLE backend tasks samtidig (5 agenter):

```
Parallel execution:
- Task 4: tasks/04-extended-filters-backend.md
- Task 5: tasks/05-sorting-strategies-backend.md
- Task 6: tasks/06-video-status-api.md
- Task 7: tasks/07-watch-later-api.md
- Task 8: tasks/08-discovery-mode-backend.md

Vent til ALLE er fullført før du går videre
```

### Steg 3: Kjør Fase 3 parallelt
Start ALLE frontend tasks samtidig (6 agenter):

```
Parallel execution:
- Task 9: tasks/09-frontend-auth.md
- Task 10: tasks/10-frontend-filters.md
- Task 11: tasks/11-frontend-sorting.md
- Task 12: tasks/12-frontend-video-status.md
- Task 13: tasks/13-frontend-watch-later.md
- Task 14: tasks/14-frontend-discovery.md

Vent til ALLE er fullført
```

### Steg 4: Integrasjonstest og deploy
Etter alle oppgaver er fullført:
1. Kjør alle tester (`dotnet test`)
2. Test manuelt i nettleser
3. Bygge Docker-image
4. Deploy til Azure Container Apps

## Feilhåndtering

Hvis en task feiler:
- I Fase 1: STOPP hele prosessen, fiks feilen først
- I Fase 2 eller 3: La andre tasks fortsette, fiks feilen parallelt

## Rapportering

Etter hver fase:
- Oppsummer hva som ble gjort
- List eventuelle problemer
- Bekreft at alle tester passerer før neste fase

## Viktige prinsipper

1. **Selvstendige tasks**: Hver subagent skal kun lese sin oppgavefil og eksisterende kode
2. **Ingen breaking changes før alle tasks er klare**: Backend må støtte både v1 og v2 inntil alt er migrert
3. **Test ofte**: Kjør tester etter hver fase
4. **Behold v1-funksjonalitet**: v2 skal være en ren utvidelse, ikke erstatte v1 før alt er klart

## Suksesskriterier

v2 er ferdig når:
- [ ] Alle 14 tasks er fullført
- [ ] Alle tester passerer
- [ ] Eksisterende v1-data er migrert
- [ ] Brukere kan logge inn med e-post
- [ ] Alle nye filtre fungerer
- [ ] Alle sorteringsstrategier fungerer
- [ ] Video-historikk fungerer (ingen duplikater)
- [ ] Watch Later fungerer
- [ ] Oppdagelsesmodus fungerer
- [ ] PWA fungerer fortsatt
- [ ] Appen deployer til Azure

God implementering!
