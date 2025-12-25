using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YouTubeKurator.Api.Data;
using YouTubeKurator.Api.Models;
using YouTubeKurator.Api.Services;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace YouTubeKurator.Api.Controllers
{
    [ApiController]
    [Route("api/videos")]
    [Authorize]
    public class VideoStatusController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IVideoStatusService _videoStatusService;

        public VideoStatusController(AppDbContext context, IVideoStatusService videoStatusService)
        {
            _context = context;
            _videoStatusService = videoStatusService;
        }

        /// <summary>
        /// Updates the status of a video in a playlist.
        /// Only the playlist owner can update video statuses.
        /// </summary>
        [HttpPost("{videoId}/status")]
        public async Task<IActionResult> UpdateStatus(string videoId, [FromBody] UpdateStatusRequest request)
        {
            if (string.IsNullOrWhiteSpace(videoId))
            {
                return BadRequest(new { error = "Video ID is required." });
            }

            if (request == null)
            {
                return BadRequest(new { error = "Request body is required." });
            }

            if (string.IsNullOrWhiteSpace(request.Status))
            {
                return BadRequest(new { error = "Status is required." });
            }

            try
            {
                // Get the current user ID from claims
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!Guid.TryParse(userIdClaim, out var userId))
                {
                    return Unauthorized(new { error = "User ID not found in token." });
                }

                // Get the playlist to verify ownership
                var playlist = await _context.Playlists.FindAsync(request.PlaylistId);
                if (playlist == null)
                {
                    return NotFound(new { error = "Playlist not found." });
                }

                // Check if user owns the playlist
                if (playlist.OwnerUserId != userId)
                {
                    return Forbid();
                }

                // Update the video status
                var videoStatus = await _videoStatusService.UpdateStatusAsync(
                    request.PlaylistId,
                    videoId,
                    request.Status,
                    request.RejectReason);

                return Ok(new
                {
                    videoId = videoStatus.VideoId,
                    playlistId = videoStatus.PlaylistId,
                    status = videoStatus.Status.ToString(),
                    rejectReason = videoStatus.RejectReason,
                    firstSeenUtc = videoStatus.FirstSeenUtc,
                    lastUpdatedUtc = videoStatus.LastUpdatedUtc
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    error = "An error occurred while updating video status.",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets the status of a specific video in a playlist.
        /// Only the playlist owner can view video statuses.
        /// </summary>
        [HttpGet("{videoId}/status")]
        public async Task<IActionResult> GetStatus(string videoId, [FromQuery] Guid playlistId)
        {
            if (string.IsNullOrWhiteSpace(videoId))
            {
                return BadRequest(new { error = "Video ID is required." });
            }

            if (playlistId == Guid.Empty)
            {
                return BadRequest(new { error = "Playlist ID is required." });
            }

            try
            {
                // Get the current user ID from claims
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!Guid.TryParse(userIdClaim, out var userId))
                {
                    return Unauthorized(new { error = "User ID not found in token." });
                }

                // Get the playlist to verify ownership
                var playlist = await _context.Playlists.FindAsync(playlistId);
                if (playlist == null)
                {
                    return NotFound(new { error = "Playlist not found." });
                }

                // Check if user owns the playlist
                if (playlist.OwnerUserId != userId)
                {
                    return Forbid();
                }

                // Get the video status
                var videoStatus = await _videoStatusService.GetStatusAsync(playlistId, videoId);

                if (videoStatus == null)
                {
                    return NotFound(new { error = "Video status not found." });
                }

                return Ok(new
                {
                    videoId = videoStatus.VideoId,
                    playlistId = videoStatus.PlaylistId,
                    status = videoStatus.Status.ToString(),
                    rejectReason = videoStatus.RejectReason,
                    firstSeenUtc = videoStatus.FirstSeenUtc,
                    lastUpdatedUtc = videoStatus.LastUpdatedUtc
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    error = "An error occurred while retrieving video status.",
                    message = ex.Message
                });
            }
        }
    }
}
