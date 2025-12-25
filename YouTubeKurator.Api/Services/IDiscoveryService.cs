using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using YouTubeKurator.Api.Data.Entities;
using YouTubeKurator.Api.Models;

namespace YouTubeKurator.Api.Services
{
    public interface IDiscoveryService
    {
        /// <summary>
        /// Selects videos using discovery mode logic with strict/relaxed/wild mixing.
        /// </summary>
        Task<IEnumerable<Video>> SelectVideosWithDiscoveryAsync(
            IEnumerable<Video> candidates,
            Playlist playlist,
            int desiredCount);

        /// <summary>
        /// Applies discovery logic to a list of videos, categorizing and blending them.
        /// </summary>
        Task<IEnumerable<Video>> ApplyDiscoveryLogicAsync(
            IEnumerable<Video> videos,
            DiscoveryProfile profile);

        /// <summary>
        /// Gets explanation for why a video was selected.
        /// </summary>
        string GetSelectionExplanation(Video video);
    }
}
