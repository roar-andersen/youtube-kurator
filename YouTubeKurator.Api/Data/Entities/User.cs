using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace YouTubeKurator.Api.Data.Entities
{
    public class User
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [StringLength(255)]
        public string Email { get; set; } = string.Empty;

        [Required]
        public DateTime CreatedUtc { get; set; }

        public DateTime? LastLoginUtc { get; set; }

        [Required]
        public bool IsActive { get; set; } = true;

        // Navigation properties
        public ICollection<Playlist> Playlists { get; set; } = new List<Playlist>();
        public ICollection<WatchLater> WatchLaterItems { get; set; } = new List<WatchLater>();
    }
}
