using System;
using System.Threading.Tasks;
using YouTubeKurator.Api.Data.Entities;
using YouTubeKurator.Api.Models;

namespace YouTubeKurator.Api.Services
{
    public interface IFilterService
    {
        /// <summary>
        /// Evaluates if a video matches all specified filters.
        /// Returns true if video passes all filters (AND logic).
        /// </summary>
        bool EvaluateFilters(Video video, PlaylistFilters filters);

        /// <summary>
        /// Normalizes and enriches a video with filtering-related data.
        /// </summary>
        Video NormalizeVideoData(Video video);
    }
}
