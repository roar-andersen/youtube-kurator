using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using YouTubeKurator.Api.Data;
using YouTubeKurator.Api.Data.Entities;

namespace YouTubeKurator.Api.Services
{
    public class WatchLaterService : IWatchLaterService
    {
        private readonly AppDbContext _context;

        public WatchLaterService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<WatchLater>> GetWatchLaterAsync(Guid userId, Guid? playlistId = null)
        {
            var query = _context.WatchLater
                .Where(wl => wl.UserId == userId);

            // Use Guid.Empty to represent global (no playlist) watch later
            var targetPlaylistId = playlistId ?? Guid.Empty;
            query = query.Where(wl => wl.PlaylistId == targetPlaylistId);

            return await query.ToListAsync();
        }

        public async Task<WatchLater> AddAsync(Guid userId, string videoId, Guid? playlistId = null)
        {
            // Use Guid.Empty to represent global (no playlist) watch later
            var targetPlaylistId = playlistId ?? Guid.Empty;

            // Check if already exists
            var existing = await _context.WatchLater
                .FirstOrDefaultAsync(wl =>
                    wl.UserId == userId &&
                    wl.VideoId == videoId &&
                    wl.PlaylistId == targetPlaylistId);

            if (existing != null)
            {
                return existing;
            }

            // Create new watch later entry
            var watchLater = new WatchLater
            {
                UserId = userId,
                VideoId = videoId,
                PlaylistId = targetPlaylistId,
                AddedUtc = DateTime.UtcNow
            };

            _context.WatchLater.Add(watchLater);
            await _context.SaveChangesAsync();

            return watchLater;
        }

        public async Task<bool> RemoveAsync(Guid userId, string videoId, Guid? playlistId = null)
        {
            // Use Guid.Empty to represent global (no playlist) watch later
            var targetPlaylistId = playlistId ?? Guid.Empty;

            var watchLater = await _context.WatchLater
                .FirstOrDefaultAsync(wl =>
                    wl.UserId == userId &&
                    wl.VideoId == videoId &&
                    wl.PlaylistId == targetPlaylistId);

            if (watchLater == null)
            {
                return false;
            }

            _context.WatchLater.Remove(watchLater);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> IsInWatchLaterAsync(Guid userId, string videoId)
        {
            return await _context.WatchLater
                .AnyAsync(wl =>
                    wl.UserId == userId &&
                    wl.VideoId == videoId);
        }
    }
}
