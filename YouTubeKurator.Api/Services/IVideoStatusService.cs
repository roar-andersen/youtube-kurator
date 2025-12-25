using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using YouTubeKurator.Api.Data.Entities;

namespace YouTubeKurator.Api.Services
{
    public interface IVideoStatusService
    {
        /// <summary>
        /// Gets the current status of a video in a playlist.
        /// </summary>
        Task<VideoStatus?> GetStatusAsync(Guid playlistId, string videoId);

        /// <summary>
        /// Updates or creates a video status for a playlist.
        /// </summary>
        Task<VideoStatus> UpdateStatusAsync(Guid playlistId, string videoId, string status, string? rejectReason = null);

        /// <summary>
        /// Gets all existing video IDs for a playlist (for duplicate filtering).
        /// </summary>
        Task<IEnumerable<string>> GetExistingVideoIdsAsync(Guid playlistId);

        /// <summary>
        /// Checks if a video exists in a playlist.
        /// </summary>
        Task<bool> VideoExistsAsync(Guid playlistId, string videoId);
    }
}
