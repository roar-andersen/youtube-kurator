using Microsoft.AspNetCore.Mvc;
using YouTubeKurator.Api.Data;
using YouTubeKurator.Api.Data.Entities;
using YouTubeKurator.Api.Services;
using YouTubeKurator.Api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.Extensions.Logging;

namespace YouTubeKurator.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PlaylistsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly CacheService _cacheService;
        private readonly IFilterService _filterService;
        private readonly ISortingService _sortingService;
        private readonly IDiscoveryService _discoveryService;
        private readonly ILogger<PlaylistsController> _logger;

        public PlaylistsController(AppDbContext context, CacheService cacheService, IFilterService filterService, ISortingService sortingService, IDiscoveryService discoveryService, ILogger<PlaylistsController> logger)
        {
            _context = context;
            _cacheService = cacheService;
            _filterService = filterService;
            _sortingService = sortingService;
            _discoveryService = discoveryService;
            _logger = logger;
        }

        // GET /api/playlists
        [HttpGet]
        [Microsoft.AspNetCore.Authorization.Authorize]
        public async Task<IActionResult> GetPlaylists()
        {
            try
            {
                // Get current user ID from claims
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!Guid.TryParse(userIdClaim, out var userId))
                {
                    return Unauthorized(new { error = "User ID not found in token." });
                }

                var playlists = await _context.Playlists
                    .Where(p => p.OwnerUserId == userId)
                    .ToListAsync();
                return Ok(playlists);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting playlists: {Message}", ex.Message);
                return StatusCode(500, new { error = "En feil oppstod ved henting av spillelister." });
            }
        }

        // GET /api/playlists/{id}
        [HttpGet("{id}")]
        [Microsoft.AspNetCore.Authorization.Authorize]
        public async Task<IActionResult> GetPlaylist(Guid id)
        {
            try
            {
                // Get current user ID from claims
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!Guid.TryParse(userIdClaim, out var userId))
                {
                    return Unauthorized(new { error = "User ID not found in token." });
                }

                var playlist = await _context.Playlists.FindAsync(id);
                if (playlist == null)
                {
                    return NotFound(new { error = "Spillelisten ble ikke funnet." });
                }

                // Verify ownership
                if (playlist.OwnerUserId != userId)
                {
                    return Forbid();
                }

                return Ok(playlist);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting playlist {PlaylistId}: {Message}", id, ex.Message);
                return StatusCode(500, new { error = "En feil oppstod ved henting av spillelisten." });
            }
        }

        // POST /api/playlists
        [HttpPost]
        [Microsoft.AspNetCore.Authorization.Authorize]
        public async Task<IActionResult> CreatePlaylist([FromBody] CreatePlaylistRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.SearchQuery))
            {
                return BadRequest(new { error = "Navn og søkeord er påkrevd." });
            }

            try
            {
                // Get current user ID from claims
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!Guid.TryParse(userIdClaim, out var userId))
                {
                    return Unauthorized(new { error = "User ID not found in token." });
                }

                var now = DateTime.UtcNow;
                var playlist = new Playlist
                {
                    Id = Guid.NewGuid(),
                    Name = request.Name,
                    SearchQuery = request.SearchQuery,
                    OwnerUserId = userId,
                    CreatedUtc = now,
                    UpdatedUtc = now,
                    IsPaused = false,
                    SortStrategy = "NewestFirst",
                    Filters = request.Filters,
                    DiscoveryProfile = null
                };

                _context.Playlists.Add(playlist);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetPlaylist), new { id = playlist.Id }, playlist);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating playlist: {Message}", ex.Message);
                return StatusCode(500, new { error = "En feil oppstod ved opprettelse av spillelisten." });
            }
        }

        // PUT /api/playlists/{id}
        [HttpPut("{id}")]
        [Microsoft.AspNetCore.Authorization.Authorize]
        public async Task<IActionResult> UpdatePlaylist(Guid id, [FromBody] UpdatePlaylistRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.SearchQuery))
            {
                return BadRequest(new { error = "Navn og søkeord er påkrevd." });
            }

            try
            {
                // Get current user ID from claims
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!Guid.TryParse(userIdClaim, out var userId))
                {
                    return Unauthorized(new { error = "User ID not found in token." });
                }

                var playlist = await _context.Playlists.FindAsync(id);
                if (playlist == null)
                {
                    return NotFound(new { error = "Spillelisten ble ikke funnet." });
                }

                // Verify ownership
                if (playlist.OwnerUserId != userId)
                {
                    return Forbid();
                }

                playlist.Name = request.Name;
                playlist.SearchQuery = request.SearchQuery;
                playlist.UpdatedUtc = DateTime.UtcNow;

                if (request.EnableDiscovery.HasValue)
                {
                    playlist.DiscoveryProfile = request.EnableDiscovery.Value ? "enabled" : null;
                }

                if (request.Filters != null)
                {
                    playlist.Filters = request.Filters;
                }

                _context.Playlists.Update(playlist);
                await _context.SaveChangesAsync();

                return Ok(playlist);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating playlist {PlaylistId}: {Message}", id, ex.Message);
                return StatusCode(500, new { error = "En feil oppstod ved oppdatering av spillelisten." });
            }
        }

        // DELETE /api/playlists/{id}
        [HttpDelete("{id}")]
        [Microsoft.AspNetCore.Authorization.Authorize]
        public async Task<IActionResult> DeletePlaylist(Guid id)
        {
            try
            {
                // Get current user ID from claims
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!Guid.TryParse(userIdClaim, out var userId))
                {
                    return Unauthorized(new { error = "User ID not found in token." });
                }

                var playlist = await _context.Playlists.FindAsync(id);
                if (playlist == null)
                {
                    return NotFound(new { error = "Spillelisten ble ikke funnet." });
                }

                // Verify ownership
                if (playlist.OwnerUserId != userId)
                {
                    return Forbid();
                }

                _context.Playlists.Remove(playlist);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting playlist {PlaylistId}: {Message}", id, ex.Message);
                return StatusCode(500, new { error = "En feil oppstod ved sletting av spillelisten." });
            }
        }

        // POST /api/playlists/{id}/refresh
        [HttpPost("{id}/refresh")]
        public async Task<IActionResult> RefreshPlaylist(Guid id)
        {
            try
            {
                // Hent playlist
                var playlist = await _context.Playlists.FindAsync(id);
                if (playlist == null)
                {
                    return NotFound(new { error = "Spillelisten ble ikke funnet." });
                }

                // Hent videoer fra cache eller YouTube
                var (videos, fromCache, cacheExpiresUtc, errorType, errorMessage) =
                    await _cacheService.GetOrFetchVideosAsync(playlist.SearchQuery);

                // Hvis det ble en feil, returner med error
                if (errorType != null)
                {
                    var errorResponse = new
                    {
                        videos = new List<Video>(),
                        fromCache = fromCache,
                        cacheExpiresUtc = cacheExpiresUtc,
                        error = new { type = errorType, message = errorMessage }
                    };
                    return Ok(errorResponse);
                }

                // Parse filters fra JSON
                PlaylistFilters filters = new();
                if (!string.IsNullOrEmpty(playlist.Filters))
                {
                    try
                    {
                        filters = JsonSerializer.Deserialize<PlaylistFilters>(playlist.Filters) ?? new();
                    }
                    catch
                    {
                        // Hvis JSON parsing feiler, bruk default filters
                        filters = new();
                    }
                }

                // Hent VideoStatuses for denne playlisten for å unngå duplikater
                var existingVideoIds = await _context.VideoStatuses
                    .Where(vs => vs.PlaylistId == id)
                    .Select(vs => vs.VideoId)
                    .ToListAsync();

                // Filtrer videoer
                var filteredVideos = videos
                    .Where(v => _filterService.EvaluateFilters(v, filters)) // Apply filters
                    .Where(v => !existingVideoIds.Contains(v.VideoId)) // Exclude existing videos
                    .ToList();

                // Apply discovery mode or standard sorting
                var finalVideos = await _discoveryService.SelectVideosWithDiscoveryAsync(
                    filteredVideos,
                    playlist,
                    20); // Request up to 20 videos

                var sortedVideos = finalVideos.ToList();

                // Bygg respons
                var response = new
                {
                    videos = sortedVideos,
                    fromCache = fromCache,
                    cacheExpiresUtc = cacheExpiresUtc,
                    error = (object?)null
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing playlist {PlaylistId}: {Message}", id, ex.Message);
                return StatusCode(500, new
                {
                    videos = new object[0],
                    fromCache = false,
                    cacheExpiresUtc = (object?)null,
                    error = new { type = "GenericError", message = "En feil oppstod ved henting av videoer." }
                });
            }
        }
    }
}
