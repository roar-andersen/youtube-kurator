using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace YouTubeKurator.Api.Data.Entities
{
    public class Playlist
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid OwnerUserId { get; set; }

        [Required]
        [StringLength(255)]
        public string Name { get; set; }

        // v1 field - keep for backward compatibility during migration
        [StringLength(500)]
        public string? SearchQuery { get; set; }

        // v2 fields
        [StringLength(1000)]
        public string? Description { get; set; }

        public string? Filters { get; set; } // JSON

        [StringLength(50)]
        public string SortStrategy { get; set; } = "NewestFirst";

        public string? DiscoveryProfile { get; set; } // JSON

        public bool IsPaused { get; set; } = false;

        [Required]
        public DateTime CreatedUtc { get; set; }

        [Required]
        public DateTime UpdatedUtc { get; set; }

        // Navigation properties
        public User Owner { get; set; } = null!;
        public ICollection<VideoStatus> VideoStatuses { get; set; } = new List<VideoStatus>();
    }
}
