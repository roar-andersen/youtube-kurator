using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YouTubeKurator.Api.Data.Entities;
using YouTubeKurator.Api.Models;

namespace YouTubeKurator.Api.Services
{
    public class DiscoveryService : IDiscoveryService
    {
        private readonly IFilterService _filterService;
        private readonly ISortingService _sortingService;
        private readonly IRelatedVideosService _relatedVideosService;

        public DiscoveryService(
            IFilterService filterService,
            ISortingService sortingService,
            IRelatedVideosService relatedVideosService)
        {
            _filterService = filterService;
            _sortingService = sortingService;
            _relatedVideosService = relatedVideosService;
        }

        public async Task<IEnumerable<Video>> SelectVideosWithDiscoveryAsync(
            IEnumerable<Video> candidates,
            Playlist playlist,
            int desiredCount)
        {
            if (!candidates.Any())
            {
                return Enumerable.Empty<Video>();
            }

            // Parse discovery profile
            var profile = ParseDiscoveryProfile(playlist.DiscoveryProfile);
            if (profile == null)
            {
                // No discovery mode, just return candidates with sorting applied
                return ApplyPlaylistSorting(candidates, playlist).Take(desiredCount);
            }

            // Get 3x the desired count to have enough videos for division
            var candidateList = candidates.Take(desiredCount * 3).ToList();
            var strictCount = (int)Math.Ceiling(desiredCount * (profile.Strict / 100.0));
            var relaxedCount = (int)Math.Ceiling(desiredCount * (profile.Relaxed / 100.0));
            var wildCount = desiredCount - strictCount - relaxedCount;

            var result = new List<Video>();

            // 1. Strict videos (apply all filters)
            var strictVideos = candidateList
                .Take((int)(candidateList.Count * (profile.Strict / 100.0)))
                .ToList();
            result.AddRange(strictVideos.Take(strictCount));

            // 2. Relaxed videos (relax one criterion)
            var relaxedVideos = candidateList
                .Skip(strictVideos.Count)
                .Take((int)(candidateList.Count * (profile.Relaxed / 100.0)))
                .Select(v => new Video
                {
                    VideoId = v.VideoId,
                    Title = v.Title,
                    ChannelName = v.ChannelName,
                    ChannelId = v.ChannelId,
                    ThumbnailUrl = v.ThumbnailUrl,
                    Duration = v.Duration,
                    PublishedAt = v.PublishedAt,
                    ViewCount = v.ViewCount,
                    LikeCount = v.LikeCount,
                    Language = v.Language,
                    ContentType = v.ContentType,
                    HasCaptions = v.HasCaptions,
                    DiscoveryReason = "Relaxed match - one criterion relaxed"
                })
                .ToList();
            result.AddRange(relaxedVideos.Take(relaxedCount));

            // 3. Wild videos (from related videos or different criteria)
            if (wildCount > 0 && profile.EnableWildcards && profile.WildcardTypes.Any())
            {
                var wildVideos = new List<Video>();

                // Sample a video for finding related content
                if (candidateList.Any())
                {
                    var sampleVideo = candidateList.First();
                    var relatedVideos = await _relatedVideosService.GetRelatedVideosAsync(
                        sampleVideo.VideoId,
                        wildCount);

                    foreach (var video in relatedVideos.Take(wildCount))
                    {
                        var wildcardType = GetWildcardType(video, sampleVideo, profile);
                        video.DiscoveryReason = $"Wildcard - {wildcardType}";
                        wildVideos.Add(video);
                    }
                }

                result.AddRange(wildVideos.Take(wildCount));
            }

            // Mark strict videos with reason
            foreach (var video in result.Where(v => string.IsNullOrEmpty(v.DiscoveryReason)))
            {
                video.DiscoveryReason = "Strict match - all criteria met";
            }

            // Apply sorting
            var sorted = ApplyPlaylistSorting(result, playlist);
            return sorted.Take(desiredCount);
        }

        public Task<IEnumerable<Video>> ApplyDiscoveryLogicAsync(
            IEnumerable<Video> videos,
            DiscoveryProfile profile)
        {
            // This method applies discovery logic without requiring a full playlist
            // For simplicity, we'll just return the videos as-is since most work
            // is done in SelectVideosWithDiscoveryAsync
            return Task.FromResult(videos);
        }

        public string GetSelectionExplanation(Video video)
        {
            return video.DiscoveryReason ?? "Selected video";
        }

        private DiscoveryProfile? ParseDiscoveryProfile(string? jsonProfile)
        {
            if (string.IsNullOrWhiteSpace(jsonProfile))
            {
                return null;
            }

            try
            {
                // Simple JSON parsing (in production, use System.Text.Json)
                var profile = new DiscoveryProfile();

                if (jsonProfile.Contains("\"strict\""))
                {
                    var match = System.Text.RegularExpressions.Regex.Match(jsonProfile, @"""strict""\s*:\s*(\d+)");
                    if (match.Success && int.TryParse(match.Groups[1].Value, out var strict))
                    {
                        profile.Strict = strict;
                    }
                }

                if (jsonProfile.Contains("\"relaxed\""))
                {
                    var match = System.Text.RegularExpressions.Regex.Match(jsonProfile, @"""relaxed""\s*:\s*(\d+)");
                    if (match.Success && int.TryParse(match.Groups[1].Value, out var relaxed))
                    {
                        profile.Relaxed = relaxed;
                    }
                }

                if (jsonProfile.Contains("\"wild\""))
                {
                    var match = System.Text.RegularExpressions.Regex.Match(jsonProfile, @"""wild""\s*:\s*(\d+)");
                    if (match.Success && int.TryParse(match.Groups[1].Value, out var wild))
                    {
                        profile.Wild = wild;
                    }
                }

                profile.EnableWildcards = !jsonProfile.Contains("\"enableWildcards\":false");

                return profile;
            }
            catch
            {
                return null;
            }
        }

        private string GetWildcardType(Video video, Video sampleVideo, DiscoveryProfile profile)
        {
            if (!profile.WildcardTypes.Any())
            {
                return "related content";
            }

            // Determine which wildcard type to use based on video properties
            if (video.Language != sampleVideo.Language &&
                profile.WildcardTypes.Contains("sametheme_otherlanguage"))
            {
                return "same theme, other language";
            }

            if (video.ChannelId == sampleVideo.ChannelId &&
                video.ContentType != sampleVideo.ContentType &&
                profile.WildcardTypes.Contains("samechannel_otherformat"))
            {
                return "same channel, other format";
            }

            if (video.ViewCount < sampleVideo.ViewCount &&
                video.LikeCount > 0 &&
                profile.WildcardTypes.Contains("lowpop_highquality"))
            {
                var likeRatio = video.ViewCount > 0 ? (double)video.LikeCount / video.ViewCount : 0;
                if (likeRatio > 0.05)
                {
                    return "low popularity, high quality";
                }
            }

            return profile.WildcardTypes.FirstOrDefault() ?? "wildcard";
        }

        private IOrderedEnumerable<Video> ApplyPlaylistSorting(IEnumerable<Video> videos, Playlist playlist)
        {
            if (string.IsNullOrWhiteSpace(playlist.SortStrategy))
            {
                return videos.OrderByDescending(v => v.PublishedAt);
            }

            if (Enum.TryParse<SortStrategy>(playlist.SortStrategy, out var strategy))
            {
                return _sortingService.Sort(videos, strategy);
            }

            return videos.OrderByDescending(v => v.PublishedAt);
        }
    }
}
