using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YouTubeKurator.Api.Data;
using YouTubeKurator.Api.Models;
using YouTubeKurator.Api.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace YouTubeKurator.Api.Controllers
{
    [ApiController]
    [Route("api/watchlater")]
    [Authorize]
    public class WatchLaterController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IWatchLaterService _watchLaterService;

        public WatchLaterController(AppDbContext context, IWatchLaterService watchLaterService)
        {
            _context = context;
            _watchLaterService = watchLaterService;
        }

        /// <summary>
        /// Gets all watch later videos for the user.
        /// If playlistId is provided, returns only watch later for that playlist.
        /// If no playlistId, returns global watch later.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] Guid? playlistId = null)
        {
            try
            {
                // Get the current user ID from claims
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!Guid.TryParse(userIdClaim, out var userId))
                {
                    return Unauthorized(new { error = "User ID not found in token." });
                }

                // If playlistId is provided, verify the user owns the playlist
                if (playlistId.HasValue)
                {
                    var playlist = await _context.Playlists.FindAsync(playlistId);
                    if (playlist == null)
                    {
                        return NotFound(new { error = "Playlist not found." });
                    }

                    if (playlist.OwnerUserId != userId)
                    {
                        return Forbid();
                    }
                }

                // Get watch later videos
                var watchLaterItems = await _watchLaterService.GetWatchLaterAsync(userId, playlistId);

                return Ok(new
                {
                    videoIds = watchLaterItems.Select(wl => wl.VideoId).ToList(),
                    playlistId = playlistId,
                    count = watchLaterItems.Count()
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    error = "An error occurred while retrieving watch later items.",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Adds a video to watch later.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Add([FromBody] AddWatchLaterRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.VideoId))
            {
                return BadRequest(new { error = "Video ID is required." });
            }

            try
            {
                // Get the current user ID from claims
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!Guid.TryParse(userIdClaim, out var userId))
                {
                    return Unauthorized(new { error = "User ID not found in token." });
                }

                // If playlistId is provided, verify the user owns the playlist
                if (request.PlaylistId.HasValue)
                {
                    var playlist = await _context.Playlists.FindAsync(request.PlaylistId);
                    if (playlist == null)
                    {
                        return NotFound(new { error = "Playlist not found." });
                    }

                    if (playlist.OwnerUserId != userId)
                    {
                        return Forbid();
                    }
                }

                // Add to watch later
                var watchLater = await _watchLaterService.AddAsync(userId, request.VideoId, request.PlaylistId);

                return Ok(new
                {
                    videoId = watchLater.VideoId,
                    playlistId = watchLater.PlaylistId,
                    addedUtc = watchLater.AddedUtc
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    error = "An error occurred while adding to watch later.",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Removes a video from watch later.
        /// </summary>
        [HttpDelete("{videoId}")]
        public async Task<IActionResult> Remove(string videoId, [FromQuery] Guid? playlistId = null)
        {
            if (string.IsNullOrWhiteSpace(videoId))
            {
                return BadRequest(new { error = "Video ID is required." });
            }

            try
            {
                // Get the current user ID from claims
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!Guid.TryParse(userIdClaim, out var userId))
                {
                    return Unauthorized(new { error = "User ID not found in token." });
                }

                // If playlistId is provided, verify the user owns the playlist
                if (playlistId.HasValue)
                {
                    var playlist = await _context.Playlists.FindAsync(playlistId);
                    if (playlist == null)
                    {
                        return NotFound(new { error = "Playlist not found." });
                    }

                    if (playlist.OwnerUserId != userId)
                    {
                        return Forbid();
                    }
                }

                // Remove from watch later
                var removed = await _watchLaterService.RemoveAsync(userId, videoId, playlistId);

                if (!removed)
                {
                    return NotFound(new { error = "Watch later item not found." });
                }

                return Ok(new { message = "Watch later item removed." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    error = "An error occurred while removing from watch later.",
                    message = ex.Message
                });
            }
        }
    }
}
