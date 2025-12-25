using System;
using System.ComponentModel.DataAnnotations;

namespace YouTubeKurator.Api.Data.Entities
{
    public class AuthCode
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [StringLength(255)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(10)]
        public string Code { get; set; } = string.Empty;

        [Required]
        public DateTime CreatedUtc { get; set; }

        [Required]
        public DateTime ExpiresUtc { get; set; }

        [Required]
        public bool IsUsed { get; set; }

        public DateTime? UsedUtc { get; set; }
    }
}
