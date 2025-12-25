# Oppgave 2.3: Lag og eksekver initiell databasemigrasjoning

## Fase
Fase 2: Datamodell

## Avhengigheter
- Oppgave 2.2 (AppDbContext må være opprettet og registrert)

## Formål
Kjøre EF Core-migrasjoner for å opprette database-skjemaet lokalt med SQLite.

## Oppgavebeskrivelse

### 1. Opprett initiell migrasjon
I prosjektmappen (`src/YouTubeKurator.Api/`), kjør:
```bash
dotnet ef migrations add InitialCreate
```

Dette vil:
- Skannet `AppDbContext` og datamodellen
- Opprett en migrasjons-fil i `Migrations/`-mappen (f.eks. `20250115120000_InitialCreate.cs`)
- Generer SQL-logikk for å opprette tabler

### 2. Eksekver migrasjonen (opprett databasen)
```bash
dotnet ef database update
```

Dette vil:
- Kjøre migrasjonen mot SQLite-databasen
- Opprette `youtube-kurator.db` i prosjektkatalogen
- Opprette `Playlists`- og `CachedSearches`-tabeller med riktige kolonner

### 3. Verifiser databasestrukturen
Verifiser at tabellene ble opprettet korrekt. Du kan:
- Åpne `youtube-kurator.db` med SQLite-verktøy (f.eks. SQLite Browser eller VS Code-utvidelse)
- Sjekk at tabellene `Playlists` og `CachedSearches` finnes
- Verifiser at kolonnene matcher datamodellen (f.eks. `Id` som GUID/TEXT, `Name`, `SearchQuery`, etc.)

Eksempel på tabellstruktur for `Playlists`:
```
Id (TEXT, PRIMARY KEY)
Name (TEXT, NOT NULL)
SearchQuery (TEXT, NOT NULL)
CreatedUtc (TEXT, NOT NULL)
UpdatedUtc (TEXT, NOT NULL)
```

Eksempel på tabellstruktur for `CachedSearches`:
```
Id (TEXT, PRIMARY KEY)
SearchQuery (TEXT, NOT NULL, UNIQUE)
ResultsJson (TEXT, NOT NULL)
FetchedUtc (TEXT, NOT NULL)
ExpiresUtc (TEXT, NOT NULL)
```

### 4. Legg til database-filen i .gitignore (hvis prosjektet bruker Git)
Sikre at SQLite-databasen ikke committes til Git (kun for lokal utvikling):
```
*.db
*.db-shm
*.db-wal
```

## Akseptansekriterier
- [ ] `dotnet ef migrations add InitialCreate` kjører uten feil
- [ ] En migrasjons-fil finnes i `Migrations/`-mappen
- [ ] `dotnet ef database update` kjører uten feil
- [ ] `youtube-kurator.db` finnes i prosjektkatalogen
- [ ] `Playlists`-tabell finnes med riktige kolonner
- [ ] `CachedSearches`-tabell finnes med riktige kolonner
- [ ] Unikt index finnes på `CachedSearches.SearchQuery`

## Referanser
- [Spesifikasjon: Datamodell](youtube-kurator-v1-spec.md#5-datamodell)
- [Entity Framework Core Migrations](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/)
