# Oppgave 1.1: Opprett ASP.NET Core-prosjekt og grunnleggende struktur

## Fase
Fase 1: Prosjektoppsett

## Avhengigheter
Ingen – denne kan kjøres først

## Formål
Sette opp grunnlaget for ASP.NET Core Web API-applikasjonen med riktig mappestruktur og basisk konfigurering.

## Oppgavebeskrivelse

### 1. Opprett ASP.NET Core Web API-prosjekt
```bash
dotnet new webapi -n YouTubeKurator.Api -o src/YouTubeKurator.Api
cd src/YouTubeKurator.Api
```

### 2. Opprett mappestruktur
Inne i `src/YouTubeKurator.Api/` skal følgende mapper eksistere:
- `Controllers/` – for API-kontrollere
- `Services/` – for business logic (YouTubeService, CacheService, etc.)
- `Data/` – for database-kontekst og entities
- `Data/Entities/` – for Playlist og CachedSearch-klasser
- `wwwroot/` – for statiske filer (HTML, CSS, JS, ikon, manifest)

### 3. Konfigurer Program.cs
- Sett opp dependency injection container
- Legg til services som vil trenges senere (DbContext, HttpClient for YouTube API)
- Konfigurer CORS hvis nødvendig for lokal utvikling
- Sett opp app til å serve statiske filer fra `wwwroot`:
  ```csharp
  app.UseStaticFiles();
  ```
- Standardmapning for root-request skal servere `index.html`

### 4. Konfigurer appsettings.json
Opprett følgende struktur:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=youtube-kurator.db"
  },
  "YouTubeApi": {
    "ApiKey": ""
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  }
}
```

Opprett også `appsettings.Development.json` for lokal utvikling (med samme struktur, men kan ha andre verdier).

### 5. Installer nødvendige NuGet-pakker
```bash
dotnet add package Microsoft.EntityFrameworkCore.Sqlite
dotnet add package Microsoft.EntityFrameworkCore.Tools
dotnet add package Microsoft.AspNetCore.Cors
```

## Akseptansekriterier
- [ ] Prosjektet bygges uten feil: `dotnet build`
- [ ] `dotnet run` starter applikasjonen på en lokal port (typisk http://localhost:5000)
- [ ] Mappestrukturen `Controllers/`, `Services/`, `Data/`, `Data/Entities/`, `wwwroot/` finnes
- [ ] `Program.cs` er konfigurert med StaticFiles middleware
- [ ] `appsettings.json` eksisterer med ConnectionString og YouTubeApi-seksjon

## Referanser
- [Spesifikasjon: Teknologivalg](youtube-kurator-v1-spec.md#2-teknologivalg)
- [Spesifikasjon: Filstruktur](youtube-kurator-v1-spec.md#13-filstruktur-forslag)
- [Spesifikasjon: Miljøvariabler](youtube-kurator-v1-spec.md#14-miljøvariabler)
