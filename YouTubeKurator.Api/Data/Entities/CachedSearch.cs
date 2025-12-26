using System;
using System.ComponentModel.DataAnnotations;

namespace YouTubeKurator.Api.Data.Entities
{
    public class CachedSearch
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [StringLength(500)]
        public required string SearchQuery { get; set; }

        [Required]
        public required string ResultsJson { get; set; }

        [Required]
        public DateTime FetchedUtc { get; set; }

        [Required]
        public DateTime ExpiresUtc { get; set; }
    }
}
