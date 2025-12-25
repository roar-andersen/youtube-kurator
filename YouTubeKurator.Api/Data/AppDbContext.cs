using Microsoft.EntityFrameworkCore;
using YouTubeKurator.Api.Data.Entities;

namespace YouTubeKurator.Api.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Playlist> Playlists { get; set; }
        public DbSet<VideoStatus> VideoStatuses { get; set; }
        public DbSet<WatchLater> WatchLater { get; set; }
        public DbSet<AuthCode> AuthCodes { get; set; }
        public DbSet<CachedSearch> CachedSearches { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User-konfigurasjon
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
                entity.Property(e => e.CreatedUtc).IsRequired();
                entity.Property(e => e.IsActive).IsRequired().HasDefaultValue(true);
                entity.HasIndex(e => e.Email).IsUnique();

                // Navigation: User -> Playlists
                entity.HasMany(e => e.Playlists)
                    .WithOne(p => p.Owner)
                    .HasForeignKey(p => p.OwnerUserId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Navigation: User -> WatchLater
                entity.HasMany(e => e.WatchLaterItems)
                    .WithOne(w => w.User)
                    .HasForeignKey(w => w.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Seed system user for v1 compatibility
                entity.HasData(
                    new User
                    {
                        Id = new Guid("00000000-0000-0000-0000-000000000001"),
                        Email = "system@youtube-kurator.local",
                        CreatedUtc = new DateTime(2025, 12, 20, 0, 0, 0, DateTimeKind.Utc),
                        IsActive = true
                    }
                );
            });

            // Playlist-konfigurasjon
            modelBuilder.Entity<Playlist>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
                entity.Property(e => e.SearchQuery).HasMaxLength(500);
                entity.Property(e => e.Description).HasMaxLength(1000);
                entity.Property(e => e.Filters).HasColumnType("TEXT");
                entity.Property(e => e.SortStrategy).HasMaxLength(50).HasDefaultValue("NewestFirst");
                entity.Property(e => e.DiscoveryProfile).HasColumnType("TEXT");
                entity.Property(e => e.IsPaused).IsRequired().HasDefaultValue(false);
                entity.Property(e => e.CreatedUtc).IsRequired();
                entity.Property(e => e.UpdatedUtc).IsRequired();
                entity.HasIndex(e => e.OwnerUserId);

                // Navigation: Playlist -> VideoStatuses
                entity.HasMany(e => e.VideoStatuses)
                    .WithOne(vs => vs.Playlist)
                    .HasForeignKey(vs => vs.PlaylistId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // VideoStatus-konfigurasjon
            modelBuilder.Entity<VideoStatus>(entity =>
            {
                entity.HasKey(e => new { e.PlaylistId, e.VideoId });
                entity.Property(e => e.VideoId).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Status).IsRequired();
                entity.Property(e => e.FirstSeenUtc).IsRequired();
                entity.Property(e => e.LastUpdatedUtc).IsRequired();
                entity.Property(e => e.RejectReason).HasMaxLength(500);
                entity.HasIndex(e => e.PlaylistId);
                entity.HasIndex(e => e.Status);
            });

            // WatchLater-konfigurasjon
            modelBuilder.Entity<WatchLater>(entity =>
            {
                entity.HasKey(e => new { e.UserId, e.VideoId, e.PlaylistId });
                entity.Property(e => e.VideoId).IsRequired().HasMaxLength(50);
                entity.Property(e => e.PlaylistId).IsRequired().HasDefaultValue(Guid.Empty);
                entity.Property(e => e.AddedUtc).IsRequired();
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.PlaylistId);

                // PlaylistId uses Guid.Empty to represent global (no playlist) watch later
                // No FK navigation - PlaylistId is just a regular GUID property without FK validation
            });

            // AuthCode-konfigurasjon
            modelBuilder.Entity<AuthCode>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Code).IsRequired().HasMaxLength(10);
                entity.Property(e => e.CreatedUtc).IsRequired();
                entity.Property(e => e.ExpiresUtc).IsRequired();
                entity.Property(e => e.IsUsed).IsRequired().HasDefaultValue(false);
                entity.HasIndex(e => new { e.Email, e.IsUsed });
            });

            // CachedSearch-konfigurasjon
            modelBuilder.Entity<CachedSearch>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.SearchQuery).IsRequired().HasMaxLength(500);
                entity.Property(e => e.ResultsJson).IsRequired();
                entity.Property(e => e.FetchedUtc).IsRequired();
                entity.Property(e => e.ExpiresUtc).IsRequired();

                // Unikt index på SearchQuery slik at det bare er én cache per søkeord
                entity.HasIndex(e => e.SearchQuery).IsUnique();
            });
        }
    }
}
