using System;
using System.Collections.Generic;
using YouTubeKurator.Api.Data.Entities;

namespace YouTubeKurator.Api.Services
{
    public class RefreshResponse
    {
        public List<Video> Videos { get; set; }
        public bool FromCache { get; set; }
        public DateTime? CacheExpiresUtc { get; set; }
        public ErrorDetail Error { get; set; }
    }

    public class ErrorDetail
    {
        public string Type { get; set; }
        public string Message { get; set; }
    }
}
