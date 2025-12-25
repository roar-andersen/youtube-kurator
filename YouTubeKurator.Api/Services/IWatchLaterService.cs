using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using YouTubeKurator.Api.Data.Entities;

namespace YouTubeKurator.Api.Services
{
    public interface IWatchLaterService
    {
        /// <summary>
        /// Gets all watch later videos for a user.
        /// If playlistId is null, returns global watch later.
        /// If playlistId is provided, returns only watch later for that playlist.
        /// </summary>
        Task<IEnumerable<WatchLater>> GetWatchLaterAsync(Guid userId, Guid? playlistId = null);

        /// <summary>
        /// Adds a video to watch later.
        /// If playlistId is null, adds to global watch later.
        /// If playlistId is provided, adds to playlist-specific watch later.
        /// </summary>
        Task<WatchLater> AddAsync(Guid userId, string videoId, Guid? playlistId = null);

        /// <summary>
        /// Removes a video from watch later.
        /// If playlistId is null, removes from global watch later.
        /// If playlistId is provided, removes from playlist-specific watch later.
        /// Returns true if removed, false if not found.
        /// </summary>
        Task<bool> RemoveAsync(Guid userId, string videoId, Guid? playlistId = null);

        /// <summary>
        /// Checks if a video is in any watch later list for the user.
        /// </summary>
        Task<bool> IsInWatchLaterAsync(Guid userId, string videoId);
    }
}
