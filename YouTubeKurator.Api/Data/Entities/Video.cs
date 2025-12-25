using System;

namespace YouTubeKurator.Api.Data.Entities
{
    public class Video
    {
        public string VideoId { get; set; }
        public string Title { get; set; }
        public string ChannelName { get; set; }
        public string ChannelId { get; set; }
        public string ThumbnailUrl { get; set; }
        public TimeSpan Duration { get; set; }
        public DateTime PublishedAt { get; set; }
        public long ViewCount { get; set; }
        public long LikeCount { get; set; }
        public string Language { get; set; } = "en";
        public string ContentType { get; set; } = "video"; // "video", "livestream", or "short"
        public bool HasCaptions { get; set; } = false;
        public string? DiscoveryReason { get; set; } // Explanation for why video was selected in discovery mode
    }
}
