using System;
using System.ComponentModel.DataAnnotations;

namespace YouTubeKurator.Api.Data.Entities
{
    public class WatchLater
    {
        [Required]
        public Guid UserId { get; set; }

        [Required]
        [StringLength(50)]
        public string VideoId { get; set; } = string.Empty;

        [Required]
        public Guid PlaylistId { get; set; } = Guid.Empty;

        [Required]
        public DateTime AddedUtc { get; set; }

        // Navigation properties
        public User User { get; set; } = null!;
    }
}
