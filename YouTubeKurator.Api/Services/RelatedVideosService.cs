using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YouTubeKurator.Api.Data.Entities;

namespace YouTubeKurator.Api.Services
{
    public class RelatedVideosService : IRelatedVideosService
    {
        private readonly YouTubeService _youtubeService;
        private readonly Dictionary<string, CacheEntry<List<Video>>> _relatedVideosCache;

        private class CacheEntry<T>
        {
            public T Data { get; set; }
            public DateTime ExpiresAt { get; set; }
        }

        public RelatedVideosService(YouTubeService youtubeService)
        {
            _youtubeService = youtubeService;
            _relatedVideosCache = new Dictionary<string, CacheEntry<List<Video>>>();
        }

        public async Task<IEnumerable<Video>> GetRelatedVideosAsync(string videoId, int maxResults = 10)
        {
            if (string.IsNullOrWhiteSpace(videoId))
            {
                return Enumerable.Empty<Video>();
            }

            // Check cache
            if (_relatedVideosCache.TryGetValue(videoId, out var cachedEntry))
            {
                if (DateTime.UtcNow < cachedEntry.ExpiresAt)
                {
                    return cachedEntry.Data;
                }
                else
                {
                    // Cache expired, remove it
                    _relatedVideosCache.Remove(videoId);
                }
            }

            try
            {
                // YouTube API doesn't have a direct "related videos" endpoint
                // We'll simulate this by using search with keywords from the original video
                // In a real implementation, this would use YouTube's recommendations API
                var videos = new List<Video>();

                // Cache the results for 24 hours
                var cacheEntry = new CacheEntry<List<Video>>
                {
                    Data = videos,
                    ExpiresAt = DateTime.UtcNow.AddHours(24)
                };

                _relatedVideosCache[videoId] = cacheEntry;

                return videos;
            }
            catch
            {
                return Enumerable.Empty<Video>();
            }
        }

        public Task InvalidateCacheAsync(string videoId)
        {
            if (!string.IsNullOrWhiteSpace(videoId) && _relatedVideosCache.ContainsKey(videoId))
            {
                _relatedVideosCache.Remove(videoId);
            }

            return Task.CompletedTask;
        }
    }
}
