using Google.Apis.YouTube.v3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YouTubeKurator.Api.Data.Entities;
using Microsoft.Extensions.Configuration;
using System.Xml;
using GoogleVideo = Google.Apis.YouTube.v3.Data.Video;

namespace YouTubeKurator.Api.Services
{
    public class YouTubeService
    {
        private readonly Google.Apis.YouTube.v3.YouTubeService _youtubeService;
        private readonly string _apiKey;

        public YouTubeService(IConfiguration configuration)
        {
            _apiKey = configuration["YouTubeApi:ApiKey"];
            if (string.IsNullOrWhiteSpace(_apiKey))
            {
                throw new InvalidOperationException("YOUTUBE_API_KEY er ikke konfigurert.");
            }

            _youtubeService = new Google.Apis.YouTube.v3.YouTubeService(new Google.Apis.Services.BaseClientService.Initializer()
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

                foreach (GoogleVideo item in statsResponse.Items)
                {
                    var duration = TimeSpan.Zero;
                    if (!string.IsNullOrWhiteSpace(item.ContentDetails?.Duration))
                    {
                        duration = ParseDuration(item.ContentDetails.Duration);
                    }

                    var likeCount = 0L;
                    if (item.Statistics?.LikeCount.HasValue == true)
                    {
                        likeCount = (long)item.Statistics.LikeCount.Value;
                    }

                    // Determine content type: YouTube doesn't directly indicate shorts,
                    // but shorts are typically <= 60 seconds
                    var contentType = duration.TotalSeconds <= 60 ? "short" : "video";

                    videos.Add(new Video
                    {
                        VideoId = item.Id,
                        Title = item.Snippet?.Title ?? "Ukjent tittel",
                        ChannelName = item.Snippet?.ChannelTitle ?? "Ukjent kanal",
                        ChannelId = item.Snippet?.ChannelId ?? "",
                        ThumbnailUrl = item.Snippet?.Thumbnails?.Medium?.Url ?? "",
                        Duration = duration,
                        PublishedAt = item.Snippet?.PublishedAtDateTimeOffset?.DateTime ?? DateTime.UtcNow,
                        ViewCount = (long)(item.Statistics?.ViewCount ?? 0),
                        LikeCount = likeCount,
                        Language = item.Snippet?.DefaultLanguage ?? "en",
                        ContentType = contentType,
                        HasCaptions = false // YouTube API doesn't expose caption info, set default
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
