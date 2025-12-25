# Oppgave 4.1: Implementer YouTubeService for API-kommunikasjon

## Fase
Fase 4: YouTube-integrasjon

## Avhengigheter
- Oppgave 2.3 (Database struktur må være på plass)
- Oppgave 1.1 (ASP.NET Core-prosjektet må være opprettet)

## Formål
Implementere tjeneste som kommuniserer med YouTube Data API v3 for å søke etter videoer.

## Oppgavebeskrivelse

### 1. Installer YouTube API NuGet-pakke
```bash
dotnet add package Google.Apis.YouTube.v3
dotnet add package Google.Apis.Core
```

### 2. Opprett YouTubeService.cs
Lag fil `src/YouTubeKurator.Api/Services/YouTubeService.cs`:

```csharp
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YouTubeKurator.Api.Data.Entities;
using Microsoft.Extensions.Configuration;

namespace YouTubeKurator.Api.Services
{
    public class YouTubeService
    {
        private readonly YouTubeService _youtubeService;
        private readonly string _apiKey;

        public YouTubeService(IConfiguration configuration)
        {
            _apiKey = configuration["YouTubeApi:ApiKey"];
            if (string.IsNullOrWhiteSpace(_apiKey))
            {
                throw new InvalidOperationException("YOUTUBE_API_KEY er ikke konfigurert.");
            }

            _youtubeService = new YouTubeService(new Google.Apis.Services.BaseClientService.Initializer()
            {
                ApiKey = _apiKey
            });
        }

        public async Task<(List<Video> videos, string errorType, string errorMessage)> SearchVideosAsync(
            string searchQuery,
            int maxResults = 50)
        {
            if (string.IsNullOrWhiteSpace(searchQuery))
            {
                return (new List<Video>(), "InvalidQuery", "Søkeordet er tomt.");
            }

            try
            {
                var request = _youtubeService.Search.List("snippet");
                request.Q = searchQuery;
                request.Type = "video";
                request.Order = SearchResource.ListRequest.OrderEnum.Date;
                request.MaxResults = maxResults;
                request.RegionCode = "NO"; // Norske resultater

                var response = await request.ExecuteAsync();

                if (response?.Items == null || response.Items.Count == 0)
                {
                    return (new List<Video>(), null, null); // Ingen feil, bare ingen resultater
                }

                var videos = new List<Video>();
                var videoIds = string.Join(",", response.Items.Select(item => item.Id.VideoId));

                // Hent detaljert informasjon (varighet, visningsantall)
                var statsRequest = _youtubeService.Videos.List("statistics,contentDetails,snippet");
                statsRequest.Id = videoIds;
                var statsResponse = await statsRequest.ExecuteAsync();

                foreach (var item in statsResponse.Items)
                {
                    var duration = TimeSpan.Zero;
                    if (!string.IsNullOrWhiteSpace(item.ContentDetails?.Duration))
                    {
                        duration = ParseDuration(item.ContentDetails.Duration);
                    }

                    videos.Add(new Video
                    {
                        VideoId = item.Id,
                        Title = item.Snippet?.Title ?? "Ukjent tittel",
                        ChannelName = item.Snippet?.ChannelTitle ?? "Ukjent kanal",
                        ThumbnailUrl = item.Snippet?.Thumbnails?.Medium?.Url ?? "",
                        Duration = duration,
                        PublishedAt = item.Snippet?.PublishedAtDateTimeOffset?.DateTime ?? DateTime.UtcNow,
                        ViewCount = long.TryParse(item.Statistics?.ViewCount, out var count) ? count : 0
                    });
                }

                return (videos, null, null); // Suksess, ingen feil
            }
            catch (Google.GoogleApiException ex) when (ex.Error.Code == 403 && ex.Error.Message.Contains("quota"))
            {
                return (new List<Video>(), "QuotaExceeded",
                    "YouTube-kvoten er brukt opp for i dag. Prøv igjen i morgen.");
            }
            catch (Google.GoogleApiException ex)
            {
                return (new List<Video>(), "YouTubeApiError",
                    $"YouTube API-feil: {ex.Message}");
            }
            catch (HttpRequestException)
            {
                return (new List<Video>(), "NetworkError",
                    "Kunne ikke koble til YouTube. Sjekk internettforbindelsen.");
            }
            catch (Exception ex)
            {
                return (new List<Video>(), "GenericError",
                    "Noe gikk galt ved søk. Prøv igjen senere.");
            }
        }

        private TimeSpan ParseDuration(string duration)
        {
            // Parse ISO 8601 duration (f.eks. PT1H2M30S)
            try
            {
                return XmlConvert.ToTimeSpan(duration);
            }
            catch
            {
                return TimeSpan.Zero;
            }
        }
    }
}
```

**OBS**: Legg til `using System.Xml;` på toppen av filen for `XmlConvert`.

### 3. Registrer YouTubeService i Program.cs
I `src/YouTubeKurator.Api/Program.cs`, legg til:

```csharp
builder.Services.AddScoped<YouTubeService>();
```

Denne linjen skal legges til før `var app = builder.Build();`.

### 4. Konfigurer API-nøkkel i appsettings.json
Sikre at `appsettings.json` inneholder:

```json
"YouTubeApi": {
  "ApiKey": ""
}
```

For lokal utvikling, legg API-nøkkelen din i `appsettings.Development.json`:

```json
{
  "YouTubeApi": {
    "ApiKey": "din-youtube-api-nøkkel-her"
  }
}
```

**OBS**: Aldri commit API-nøkkelen til Git. Legg `appsettings.Development.json` i `.gitignore`.

### 5. Test YouTubeService
Lag en enkel test-endepunkt (eller unit-test) for å sjekke at den fungerer:

```csharp
// I PlaylistsController eller TestController
[HttpGet("test-youtube")]
public async Task<IActionResult> TestYouTube()
{
    var (videos, errorType, errorMessage) = await _youtubeService.SearchVideosAsync("C# tutorial");
    if (errorType != null)
    {
        return BadRequest(new { error = errorType, message = errorMessage });
    }
    return Ok(videos);
}
```

## Akseptansekriterier
- [ ] YouTubeService.cs eksisterer i `Services/`-mappen
- [ ] Klassen tar IConfiguration via constructor
- [ ] SearchVideosAsync() returnerer tuple med (videos, errorType, errorMessage)
- [ ] Søk mot YouTube API fungerer og returnerer videoer
- [ ] ErrorHandling for QuotaExceeded fungerer
- [ ] ErrorHandling for NetworkError fungerer
- [ ] TimeSpan-parsing fra ISO 8601-format fungerer
- [ ] Tjenesten er registrert som Scoped service i Program.cs

## Referanser
- [Spesifikasjon: YouTube API-integrasjon](youtube-kurator-v1-spec.md#9-youtube-api-integrasjon)
- [Google APIs Nuget Package](https://www.nuget.org/packages/Google.Apis.YouTube.v3/)
- [YouTube Data API v3 Search Documentation](https://developers.google.com/youtube/v3/docs/search/list)
