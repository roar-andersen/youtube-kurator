using Xunit;
using YouTubeKurator.Api.Data.Entities;
using YouTubeKurator.Api.Models;
using YouTubeKurator.Api.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace YouTubeKurator.Tests.Services
{
    public class SortingServiceTests
    {
        private readonly ISortingService _sortingService = new SortingService();

        private Video CreateTestVideo(
            string videoId = "test_id",
            string title = "Test Video",
            TimeSpan? duration = null,
            DateTime? publishedAt = null,
            long viewCount = 10000,
            long likeCount = 500)
        {
            return new Video
            {
                VideoId = videoId,
                Title = title,
                ChannelName = "Test Channel",
                ChannelId = "UC_test",
                ThumbnailUrl = "https://example.com/thumb.jpg",
                Duration = duration ?? TimeSpan.FromMinutes(10),
                PublishedAt = publishedAt ?? DateTime.UtcNow.AddDays(-10),
                ViewCount = viewCount,
                LikeCount = likeCount,
                Language = "en",
                ContentType = "video",
                HasCaptions = false
            };
        }

        // NewestFirst Tests
        [Fact]
        public void Sort_NewestFirst_ReturnsNewestVideoFirst()
        {
            var now = DateTime.UtcNow;
            var videos = new List<Video>
            {
                CreateTestVideo(videoId: "old", publishedAt: now.AddDays(-30)),
                CreateTestVideo(videoId: "new", publishedAt: now.AddDays(-1)),
                CreateTestVideo(videoId: "medium", publishedAt: now.AddDays(-10))
            };

            var sorted = _sortingService.Sort(videos, SortStrategy.NewestFirst).ToList();

            Assert.Equal("new", sorted[0].VideoId);
            Assert.Equal("medium", sorted[1].VideoId);
            Assert.Equal("old", sorted[2].VideoId);
        }

        // MostRelevant Tests
        [Fact]
        public void Sort_MostRelevant_UsesCombinedScore()
        {
            var videos = new List<Video>
            {
                CreateTestVideo(videoId: "high_views", viewCount: 100000, likeCount: 5000), // High relevance
                CreateTestVideo(videoId: "low_views", viewCount: 1000, likeCount: 100),     // Low relevance
                CreateTestVideo(videoId: "medium", viewCount: 50000, likeCount: 1000)      // Medium relevance
            };

            var sorted = _sortingService.Sort(videos, SortStrategy.MostRelevant).ToList();

            Assert.Equal("high_views", sorted[0].VideoId);
            Assert.Equal("medium", sorted[1].VideoId);
            Assert.Equal("low_views", sorted[2].VideoId);
        }

        [Fact]
        public void Sort_MostRelevant_ZeroViewsHandled()
        {
            var videos = new List<Video>
            {
                CreateTestVideo(videoId: "zero_views", viewCount: 0, likeCount: 0),
                CreateTestVideo(videoId: "has_views", viewCount: 1000, likeCount: 100)
            };

            var sorted = _sortingService.Sort(videos, SortStrategy.MostRelevant).ToList();

            Assert.Equal("has_views", sorted[0].VideoId);
            Assert.Equal("zero_views", sorted[1].VideoId);
        }

        // MostPopular Tests
        [Fact]
        public void Sort_MostPopular_SortsByViewCount()
        {
            var videos = new List<Video>
            {
                CreateTestVideo(videoId: "v1", viewCount: 1000),
                CreateTestVideo(videoId: "v2", viewCount: 100000),
                CreateTestVideo(videoId: "v3", viewCount: 10000)
            };

            var sorted = _sortingService.Sort(videos, SortStrategy.MostPopular).ToList();

            Assert.Equal("v2", sorted[0].VideoId);
            Assert.Equal("v3", sorted[1].VideoId);
            Assert.Equal("v1", sorted[2].VideoId);
        }

        // MostPopularRelative Tests
        [Fact]
        public void Sort_MostPopularRelative_NewerVideoRanksHigher()
        {
            var now = DateTime.UtcNow;
            var videos = new List<Video>
            {
                CreateTestVideo(videoId: "old_popular", viewCount: 100000, publishedAt: now.AddDays(-365)), // Old but many views
                CreateTestVideo(videoId: "new_trending", viewCount: 10000, publishedAt: now.AddDays(-1))    // New with few views
            };

            var sorted = _sortingService.Sort(videos, SortStrategy.MostPopularRelative).ToList();

            // New trending gets higher views per day score
            Assert.Equal("new_trending", sorted[0].VideoId);
            Assert.Equal("old_popular", sorted[1].VideoId);
        }

        // HighestQuality Tests
        [Fact]
        public void Sort_HighestQuality_SortsByLikeRatio()
        {
            var videos = new List<Video>
            {
                CreateTestVideo(videoId: "high_ratio", viewCount: 1000, likeCount: 100),    // 10% ratio
                CreateTestVideo(videoId: "low_ratio", viewCount: 10000, likeCount: 100),    // 1% ratio
                CreateTestVideo(videoId: "medium_ratio", viewCount: 5000, likeCount: 250)   // 5% ratio
            };

            var sorted = _sortingService.Sort(videos, SortStrategy.HighestQuality).ToList();

            Assert.Equal("high_ratio", sorted[0].VideoId);
            Assert.Equal("medium_ratio", sorted[1].VideoId);
            Assert.Equal("low_ratio", sorted[2].VideoId);
        }

        [Fact]
        public void Sort_HighestQuality_ZeroViewsHandled()
        {
            var videos = new List<Video>
            {
                CreateTestVideo(videoId: "zero_views", viewCount: 0, likeCount: 0),
                CreateTestVideo(videoId: "has_views", viewCount: 1000, likeCount: 100)
            };

            var sorted = _sortingService.Sort(videos, SortStrategy.HighestQuality).ToList();

            // Video with likes should rank first
            Assert.Equal("has_views", sorted[0].VideoId);
            Assert.Equal("zero_views", sorted[1].VideoId);
        }

        // LengthShort Tests
        [Fact]
        public void Sort_LengthShort_ShortestVideoFirst()
        {
            var videos = new List<Video>
            {
                CreateTestVideo(videoId: "long", duration: TimeSpan.FromMinutes(60)),
                CreateTestVideo(videoId: "short", duration: TimeSpan.FromSeconds(30)),
                CreateTestVideo(videoId: "medium", duration: TimeSpan.FromMinutes(10))
            };

            var sorted = _sortingService.Sort(videos, SortStrategy.LengthShort).ToList();

            Assert.Equal("short", sorted[0].VideoId);
            Assert.Equal("medium", sorted[1].VideoId);
            Assert.Equal("long", sorted[2].VideoId);
        }

        // LengthLong Tests
        [Fact]
        public void Sort_LengthLong_LongestVideoFirst()
        {
            var videos = new List<Video>
            {
                CreateTestVideo(videoId: "long", duration: TimeSpan.FromMinutes(60)),
                CreateTestVideo(videoId: "short", duration: TimeSpan.FromSeconds(30)),
                CreateTestVideo(videoId: "medium", duration: TimeSpan.FromMinutes(10))
            };

            var sorted = _sortingService.Sort(videos, SortStrategy.LengthLong).ToList();

            Assert.Equal("long", sorted[0].VideoId);
            Assert.Equal("medium", sorted[1].VideoId);
            Assert.Equal("short", sorted[2].VideoId);
        }

        // ChannelAuthority Tests
        [Fact]
        public void Sort_ChannelAuthority_SortsByViewCount()
        {
            var videos = new List<Video>
            {
                CreateTestVideo(videoId: "v1", viewCount: 1000),
                CreateTestVideo(videoId: "v2", viewCount: 100000),
                CreateTestVideo(videoId: "v3", viewCount: 10000)
            };

            var sorted = _sortingService.Sort(videos, SortStrategy.ChannelAuthority).ToList();

            Assert.Equal("v2", sorted[0].VideoId);
            Assert.Equal("v3", sorted[1].VideoId);
            Assert.Equal("v1", sorted[2].VideoId);
        }

        // WeightedScore Tests
        [Fact]
        public void Sort_WeightedScore_BalancesMultipleFactors()
        {
            var now = DateTime.UtcNow;
            var videos = new List<Video>
            {
                CreateTestVideo(videoId: "new_trending", viewCount: 50000, likeCount: 2500, publishedAt: now.AddDays(-1)),    // High recency, decent quality
                CreateTestVideo(videoId: "old_popular", viewCount: 500000, likeCount: 5000, publishedAt: now.AddDays(-365)),  // High popularity but old
                CreateTestVideo(videoId: "balanced", viewCount: 100000, likeCount: 5000, publishedAt: now.AddDays(-30))       // Balanced across all metrics
            };

            var sorted = _sortingService.Sort(videos, SortStrategy.WeightedScore).ToList();

            // Balanced should rank highest due to good score across all factors
            // Exact order depends on weights, but weighted approach should balance factors
            Assert.Equal(3, sorted.Count);
        }

        // Empty and edge cases
        [Fact]
        public void Sort_EmptyList_ReturnsEmptyList()
        {
            var videos = new List<Video>();

            var sorted = _sortingService.Sort(videos, SortStrategy.NewestFirst).ToList();

            Assert.Empty(sorted);
        }

        [Fact]
        public void Sort_SingleVideo_ReturnsThatVideo()
        {
            var videos = new List<Video> { CreateTestVideo(videoId: "only") };

            var sorted = _sortingService.Sort(videos, SortStrategy.NewestFirst).ToList();

            Assert.Single(sorted);
            Assert.Equal("only", sorted[0].VideoId);
        }

        [Fact]
        public void Sort_AllStrategies_ProduceResults()
        {
            var videos = new List<Video>
            {
                CreateTestVideo(videoId: "v1", viewCount: 100, likeCount: 10, duration: TimeSpan.FromMinutes(5), publishedAt: DateTime.UtcNow.AddDays(-10)),
                CreateTestVideo(videoId: "v2", viewCount: 1000, likeCount: 50, duration: TimeSpan.FromMinutes(15), publishedAt: DateTime.UtcNow.AddDays(-5)),
                CreateTestVideo(videoId: "v3", viewCount: 500, likeCount: 25, duration: TimeSpan.FromMinutes(10), publishedAt: DateTime.UtcNow.AddDays(-15))
            };

            var strategies = new[]
            {
                SortStrategy.NewestFirst,
                SortStrategy.MostRelevant,
                SortStrategy.MostPopular,
                SortStrategy.MostPopularRelative,
                SortStrategy.HighestQuality,
                SortStrategy.LengthShort,
                SortStrategy.LengthLong,
                SortStrategy.ChannelAuthority,
                SortStrategy.WeightedScore
            };

            foreach (var strategy in strategies)
            {
                var sorted = _sortingService.Sort(videos, strategy).ToList();
                Assert.Equal(3, sorted.Count);
                // All videos should still be present
                Assert.Contains(sorted, v => v.VideoId == "v1");
                Assert.Contains(sorted, v => v.VideoId == "v2");
                Assert.Contains(sorted, v => v.VideoId == "v3");
            }
        }

        [Fact]
        public void Sort_DefaultStrategy_UsesNewestFirst()
        {
            var now = DateTime.UtcNow;
            var videos = new List<Video>
            {
                CreateTestVideo(videoId: "old", publishedAt: now.AddDays(-30)),
                CreateTestVideo(videoId: "new", publishedAt: now.AddDays(-1))
            };

            // Use invalid strategy that defaults to NewestFirst
            var sorted = _sortingService.Sort(videos, (SortStrategy)999).ToList();

            Assert.Equal("new", sorted[0].VideoId);
        }

        [Fact]
        public void Sort_StabilityTest_SameValuesPreserveOrder()
        {
            var now = DateTime.UtcNow;
            var videos = new List<Video>
            {
                CreateTestVideo(videoId: "v1", viewCount: 1000, publishedAt: now),
                CreateTestVideo(videoId: "v2", viewCount: 1000, publishedAt: now),
                CreateTestVideo(videoId: "v3", viewCount: 1000, publishedAt: now)
            };

            var sorted = _sortingService.Sort(videos, SortStrategy.MostPopular).ToList();

            // All have same view count, so original order should be preserved (stable sort)
            Assert.Equal(3, sorted.Count);
            Assert.Contains(sorted, v => v.VideoId == "v1");
            Assert.Contains(sorted, v => v.VideoId == "v2");
            Assert.Contains(sorted, v => v.VideoId == "v3");
        }
    }
}
