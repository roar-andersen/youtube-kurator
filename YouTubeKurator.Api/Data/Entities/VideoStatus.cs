using System;
using System.ComponentModel.DataAnnotations;

namespace YouTubeKurator.Api.Data.Entities
{
    public class VideoStatus
    {
        [Required]
        public Guid PlaylistId { get; set; }

        [Required]
        [StringLength(50)]
        public string VideoId { get; set; } = string.Empty;

        [Required]
        public VideoStatusEnum Status { get; set; }

        [Required]
        public DateTime FirstSeenUtc { get; set; }

        [Required]
        public DateTime LastUpdatedUtc { get; set; }

        [StringLength(500)]
        public string? RejectReason { get; set; }

        // Navigation property
        public Playlist Playlist { get; set; } = null!;
    }

    public enum VideoStatusEnum
    {
        New,
        Seen,
        Saved,
        Rejected
    }
}
