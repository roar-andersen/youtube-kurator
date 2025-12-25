using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using YouTubeKurator.Api.Data;
using YouTubeKurator.Api.Data.Entities;

namespace YouTubeKurator.Api.Services
{
    public class VideoStatusService : IVideoStatusService
    {
        private readonly AppDbContext _context;

        public VideoStatusService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<VideoStatus?> GetStatusAsync(Guid playlistId, string videoId)
        {
            return await _context.VideoStatuses
                .FirstOrDefaultAsync(vs =>
                    vs.PlaylistId == playlistId &&
                    vs.VideoId == videoId);
        }

        public async Task<VideoStatus> UpdateStatusAsync(Guid playlistId, string videoId, string status, string? rejectReason = null)
        {
            // Validate and convert status to enum
            if (!Enum.TryParse<VideoStatusEnum>(status, out var statusEnum))
            {
                var validStatuses = string.Join(", ", Enum.GetNames(typeof(VideoStatusEnum)));
                throw new ArgumentException($"Invalid status. Must be one of: {validStatuses}", nameof(status));
            }

            // Get existing status or create new
            var videoStatus = await GetStatusAsync(playlistId, videoId);

            if (videoStatus == null)
            {
                // Create new status
                videoStatus = new VideoStatus
                {
                    PlaylistId = playlistId,
                    VideoId = videoId,
                    Status = statusEnum,
                    RejectReason = rejectReason,
                    FirstSeenUtc = DateTime.UtcNow,
                    LastUpdatedUtc = DateTime.UtcNow
                };
                _context.VideoStatuses.Add(videoStatus);
            }
            else
            {
                // Update existing status
                videoStatus.Status = statusEnum;
                videoStatus.RejectReason = rejectReason;
                videoStatus.LastUpdatedUtc = DateTime.UtcNow;
                _context.VideoStatuses.Update(videoStatus);
            }

            await _context.SaveChangesAsync();
            return videoStatus;
        }

        public async Task<IEnumerable<string>> GetExistingVideoIdsAsync(Guid playlistId)
        {
            return await _context.VideoStatuses
                .Where(vs => vs.PlaylistId == playlistId)
                .Select(vs => vs.VideoId)
                .ToListAsync();
        }

        public async Task<bool> VideoExistsAsync(Guid playlistId, string videoId)
        {
            return await _context.VideoStatuses
                .AnyAsync(vs =>
                    vs.PlaylistId == playlistId &&
                    vs.VideoId == videoId);
        }
    }
}
