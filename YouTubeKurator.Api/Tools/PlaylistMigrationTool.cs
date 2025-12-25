using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using YouTubeKurator.Api.Data;
using YouTubeKurator.Api.Data.Entities;

namespace YouTubeKurator.Api.Tools
{
    public class PlaylistMigrationTool
    {
        public static async Task MigrateAllPlaylistsAsync(AppDbContext context)
        {
            var systemUser = await context.Users
                .FirstOrDefaultAsync(u => u.Email == "system@youtube-kurator.local");

            if (systemUser == null)
            {
                systemUser = new User
                {
                    Id = new Guid("00000000-0000-0000-0000-000000000001"),
                    Email = "system@youtube-kurator.local",
                    CreatedUtc = DateTime.UtcNow,
                    IsActive = true
                };
                context.Users.Add(systemUser);
                await context.SaveChangesAsync();
            }

            var playlistsToMigrate = await context.Playlists
                .Where(p => p.OwnerUserId == Guid.Empty || p.OwnerUserId == default)
                .ToListAsync();

            foreach (var playlist in playlistsToMigrate)
            {
                playlist.OwnerUserId = systemUser.Id;

                // Convert SearchQuery to Filters JSON
                if (!string.IsNullOrEmpty(playlist.SearchQuery))
                {
                    var filters = new Dictionary<string, object>
                    {
                        { "themes", new[] { playlist.SearchQuery } }
                    };
                    playlist.Filters = JsonSerializer.Serialize(filters);
                }

                playlist.SortStrategy = "NewestFirst";
                playlist.IsPaused = false;
            }

            await context.SaveChangesAsync();
        }
    }
}
