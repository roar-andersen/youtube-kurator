using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using YouTubeKurator.Api.Data.Entities;

namespace YouTubeKurator.Api.Services
{
    public interface IRelatedVideosService
    {
        /// <summary>
        /// Gets related videos for a given video ID from YouTube.
        /// Results are cached to preserve API quota.
        /// </summary>
        Task<IEnumerable<Video>> GetRelatedVideosAsync(string videoId, int maxResults = 10);

        /// <summary>
        /// Invalidates cache for a specific video's related videos.
        /// </summary>
        Task InvalidateCacheAsync(string videoId);
    }
}
