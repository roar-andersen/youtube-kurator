# Oppgave 4.2: Implementer caching-logikk for søkeresultater

## Fase
Fase 4: YouTube-integrasjon

## Avhengigheter
- Oppgave 4.1 (YouTubeService må eksistere)
- Oppgave 2.3 (CachedSearch-entitet må eksistere i database)

## Formål
Implementere caching-mekanisme som lagrer YouTube-søkeresultater i 1 time for å spare API-kvota og forbedre ytelse.

## Oppgavebeskrivelse

### 1. Opprett CacheService.cs
Lag fil `src/YouTubeKurator.Api/Services/CacheService.cs`:

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using YouTubeKurator.Api.Data;
using YouTubeKurator.Api.Data.Entities;

namespace YouTubeKurator.Api.Services
{
    public class CacheService
    {
        private readonly AppDbContext _context;
        private readonly YouTubeService _youtubeService;

        public CacheService(AppDbContext context, YouTubeService youtubeService)
        {
            _context = context;
            _youtubeService = youtubeService;
        }

        /// <summary>
        /// Henter videoer fra cache hvis gyldig, ellers fra YouTube.
        /// </summary>
        public async Task<(List<Video> videos, bool fromCache, DateTime? cacheExpiresUtc, string errorType, string errorMessage)>
            GetOrFetchVideosAsync(string searchQuery)
        {
            if (string.IsNullOrWhiteSpace(searchQuery))
            {
                return (new List<Video>(), false, null, "InvalidQuery", "Søkeordet er tomt.");
            }

            // Sjekk cache
            var cached = await _context.CachedSearches
                .FirstOrDefaultAsync(cs => cs.SearchQuery == searchQuery);

            if (cached != null && cached.ExpiresUtc > DateTime.UtcNow)
            {
                // Cache er gyldig
                var cachedVideos = JsonSerializer.Deserialize<List<Video>>(cached.ResultsJson);
                return (cachedVideos ?? new List<Video>(), true, cached.ExpiresUtc, null, null);
            }

            // Cache er utløpt eller finnes ikke – hent fra YouTube
            var (videos, errorType, errorMessage) = await _youtubeService.SearchVideosAsync(searchQuery);

            // Lagre i cache selv om det er feil (hvis vi har tidligere cache)
            if (errorType != null)
            {
                // Hvis det er QuotaExceeded eller NetworkError, returner sist lagret cache med error
                if (errorType == "QuotaExceeded" || errorType == "NetworkError")
                {
                    if (cached != null)
                    {
                        var cachedVideos = JsonSerializer.Deserialize<List<Video>>(cached.ResultsJson);
                        return (cachedVideos ?? new List<Video>(), true, cached.ExpiresUtc, errorType, errorMessage);
                    }
                }
                // Hvis det er annen feil, returner tom liste med error
                return (new List<Video>(), false, null, errorType, errorMessage);
            }

            // Suksess – lagre i cache
            var resultsJson = JsonSerializer.Serialize(videos);
            var now = DateTime.UtcNow;
            var expiresUtc = now.AddHours(1);

            if (cached != null)
            {
                // Oppdater eksisterende cache
                cached.ResultsJson = resultsJson;
                cached.FetchedUtc = now;
                cached.ExpiresUtc = expiresUtc;
                _context.CachedSearches.Update(cached);
            }
            else
            {
                // Opprett ny cache
                var newCache = new CachedSearch
                {
                    Id = Guid.NewGuid(),
                    SearchQuery = searchQuery,
                    ResultsJson = resultsJson,
                    FetchedUtc = now,
                    ExpiresUtc = expiresUtc
                };
                _context.CachedSearches.Add(newCache);
            }

            await _context.SaveChangesAsync();
            return (videos, false, expiresUtc, null, null);
        }

        /// <summary>
        /// Sletter cache for et spesifikt søkeord.
        /// </summary>
        public async Task InvalidateCacheAsync(string searchQuery)
        {
            var cached = await _context.CachedSearches
                .FirstOrDefaultAsync(cs => cs.SearchQuery == searchQuery);

            if (cached != null)
            {
                _context.CachedSearches.Remove(cached);
                await _context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Sletter all cache som er utløpt.
        /// </summary>
        public async Task CleanupExpiredCacheAsync()
        {
            var expiredCache = await _context.CachedSearches
                .Where(cs => cs.ExpiresUtc <= DateTime.UtcNow)
                .ToListAsync();

            if (expiredCache.Any())
            {
                _context.CachedSearches.RemoveRange(expiredCache);
                await _context.SaveChangesAsync();
            }
        }
    }
}
```

### 2. Registrer CacheService i Program.cs
I `src/YouTubeKurator.Api/Program.cs`, legg til:

```csharp
builder.Services.AddScoped<CacheService>();
```

Denne linjen skal legges til etter YouTubeService-registreringen og før `var app = builder.Build();`.

### 3. Opprett return-objekt for refresh-endepunkt
For å organisere svaret fra `CacheService.GetOrFetchVideosAsync()`, kan du opprett en DTO:

Lag fil `src/YouTubeKurator.Api/Services/RefreshResponse.cs`:

```csharp
using System;
using System.Collections.Generic;
using YouTubeKurator.Api.Data.Entities;

namespace YouTubeKurator.Api.Services
{
    public class RefreshResponse
    {
        public List<Video> Videos { get; set; }
        public bool FromCache { get; set; }
        public DateTime? CacheExpiresUtc { get; set; }
        public ErrorDetail Error { get; set; }
    }

    public class ErrorDetail
    {
        public string Type { get; set; }
        public string Message { get; set; }
    }
}
```

## Akseptansekriterier
- [ ] CacheService.cs eksisterer i `Services/`-mappen
- [ ] GetOrFetchVideosAsync() returnerer tuple med (videos, fromCache, cacheExpiresUtc, errorType, errorMessage)
- [ ] Søk innenfor cache-vindu (1 time) returnerer cached resultat med `fromCache: true`
- [ ] Søk etter cache-vindu henter fra YouTube og lagrer i cache
- [ ] Ved QuotaExceeded-feil returneres sist lagret cache (hvis finnes) + error
- [ ] Ved NetworkError-feil returneres sist lagret cache (hvis finnes) + error
- [ ] CacheService er registrert som Scoped service
- [ ] RefreshResponse-klassen kan brukes til å strukturere API-respons

## Referanser
- [Spesifikasjon: Caching](youtube-kurator-v1-spec.md#forretningsregler)
- [Spesifikasjon: Feilhåndtering](youtube-kurator-v1-spec.md#feilhåndtering)
