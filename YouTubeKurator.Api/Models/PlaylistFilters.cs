using System;
using System.Collections.Generic;

namespace YouTubeKurator.Api.Models
{
    public class PlaylistFilters
    {
        public List<string> Themes { get; set; } = new();
        public List<string> IncludeKeywords { get; set; } = new();
        public List<string> ExcludeKeywords { get; set; } = new();
        public DurationFilter Duration { get; set; } = new();
        public PublishedTimeFilter PublishedTime { get; set; } = new();
        public LanguageFilter Language { get; set; } = new();
        public ContentTypeFilter ContentType { get; set; } = new();
        public PopularityFilter Popularity { get; set; } = new();
        public ChannelFilter Channels { get; set; } = new();
    }

    public class DurationFilter
    {
        public int MinSeconds { get; set; } = 0;
        public int MaxSeconds { get; set; } = int.MaxValue;
    }

    public class PublishedTimeFilter
    {
        public string Type { get; set; } = "relative"; // "relative" or "absolute"
        public int Days { get; set; } = 3650; // For relative (default: 10 years)
        public DateTime? StartDate { get; set; } // For absolute
        public DateTime? EndDate { get; set; } // For absolute
    }

    public class LanguageFilter
    {
        public string Preferred { get; set; } = "";
        public string Region { get; set; } = "";
    }

    public class ContentTypeFilter
    {
        public bool Videos { get; set; } = true;
        public bool Livestreams { get; set; } = false;
        public bool Shorts { get; set; } = false;
    }

    public class PopularityFilter
    {
        public long MinViews { get; set; } = 0;
        public long MinLikes { get; set; } = 0;
        public double MinLikeRatio { get; set; } = 0; // likes / views
    }

    public class ChannelFilter
    {
        public List<string> Include { get; set; } = new();
        public List<string> Exclude { get; set; } = new();
    }
}
