using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace YouTubeKurator.Api.TempScaffold;

public partial class YoutubeKuratorContext : DbContext
{
    public YoutubeKuratorContext()
    {
    }

    public YoutubeKuratorContext(DbContextOptions<YoutubeKuratorContext> options)
        : base(options)
    {
    }

    public virtual DbSet<CachedSearch> CachedSearches { get; set; }

    public virtual DbSet<EfmigrationsLock> EfmigrationsLocks { get; set; }

    public virtual DbSet<Playlist> Playlists { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlite("Name=ConnectionStrings:DefaultConnection");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CachedSearch>(entity =>
        {
            entity.HasIndex(e => e.SearchQuery, "IX_CachedSearches_SearchQuery").IsUnique();
        });

        modelBuilder.Entity<EfmigrationsLock>(entity =>
        {
            entity.ToTable("__EFMigrationsLock");

            entity.Property(e => e.Id).ValueGeneratedNever();
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
