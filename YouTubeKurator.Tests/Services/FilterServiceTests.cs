using Xunit;
using YouTubeKurator.Api.Data.Entities;
using YouTubeKurator.Api.Models;
using YouTubeKurator.Api.Services;
using System;
using System.Collections.Generic;

namespace YouTubeKurator.Tests.Services
{
    public class FilterServiceTests
    {
        private readonly IFilterService _filterService = new FilterService();

        private Video CreateTestVideo(
            string title = "Gaming Tutorial",
            string channelName = "Tech Channel",
            TimeSpan? duration = null,
            DateTime? publishedAt = null,
            long viewCount = 10000,
            long likeCount = 500,
            string language = "en",
            string contentType = "video",
            string channelId = "UC_test_channel")
        {
            return new Video
            {
                VideoId = "test_video_id",
                Title = title,
                ChannelName = channelName,
                ChannelId = channelId,
                ThumbnailUrl = "https://example.com/thumb.jpg",
                Duration = duration ?? TimeSpan.FromMinutes(10),
                PublishedAt = publishedAt ?? DateTime.UtcNow.AddDays(-5),
                ViewCount = viewCount,
                LikeCount = likeCount,
                Language = language,
                ContentType = contentType,
                HasCaptions = false
            };
        }

        // Theme Filter Tests
        [Fact]
        public void EvaluateFilters_ThemeMatchesTitle_ReturnsTrue()
        {
            var video = CreateTestVideo(title: "C# Gaming Tutorial");
            var filters = new PlaylistFilters { Themes = new List<string> { "gaming" } };

            Assert.True(_filterService.EvaluateFilters(video, filters));
        }

        [Fact]
        public void EvaluateFilters_ThemeMatchesChannel_ReturnsTrue()
        {
            var video = CreateTestVideo(channelName: "Gaming Channel");
            var filters = new PlaylistFilters { Themes = new List<string> { "gaming" } };

            Assert.True(_filterService.EvaluateFilters(video, filters));
        }

        [Fact]
        public void EvaluateFilters_ThemeDoesNotMatch_ReturnsFalse()
        {
            var video = CreateTestVideo(title: "Cooking Tutorial");
            var filters = new PlaylistFilters { Themes = new List<string> { "gaming" } };

            Assert.False(_filterService.EvaluateFilters(video, filters));
        }

        [Fact]
        public void EvaluateFilters_MultipleThemesOneMatches_ReturnsTrue()
        {
            var video = CreateTestVideo(title: "Unity Game Development");
            var filters = new PlaylistFilters { Themes = new List<string> { "game", "cooking" } };

            Assert.True(_filterService.EvaluateFilters(video, filters));
        }

        [Fact]
        public void EvaluateFilters_EmptyThemeList_ReturnsTrue()
        {
            var video = CreateTestVideo();
            var filters = new PlaylistFilters { Themes = new List<string>() };

            Assert.True(_filterService.EvaluateFilters(video, filters));
        }

        // Include Keywords Tests (AND logic)
        [Fact]
        public void EvaluateFilters_AllIncludeKeywordsMatch_ReturnsTrue()
        {
            var video = CreateTestVideo(title: "Unity Game Engine Tutorial");
            var filters = new PlaylistFilters
            {
                IncludeKeywords = new List<string> { "unity", "engine" }
            };

            Assert.True(_filterService.EvaluateFilters(video, filters));
        }

        [Fact]
        public void EvaluateFilters_SomeIncludeKeywordsMissing_ReturnsFalse()
        {
            var video = CreateTestVideo(title: "Unity Tutorial");
            var filters = new PlaylistFilters
            {
                IncludeKeywords = new List<string> { "unity", "engine", "godot" }
            };

            Assert.False(_filterService.EvaluateFilters(video, filters));
        }

        // Exclude Keywords Tests
        [Fact]
        public void EvaluateFilters_ExcludeKeywordPresent_ReturnsFalse()
        {
            var video = CreateTestVideo(title: "Beginner C# Tutorial");
            var filters = new PlaylistFilters
            {
                ExcludeKeywords = new List<string> { "beginner" }
            };

            Assert.False(_filterService.EvaluateFilters(video, filters));
        }

        [Fact]
        public void EvaluateFilters_ExcludeKeywordAbsent_ReturnsTrue()
        {
            var video = CreateTestVideo(title: "Advanced C# Tutorial");
            var filters = new PlaylistFilters
            {
                ExcludeKeywords = new List<string> { "beginner" }
            };

            Assert.True(_filterService.EvaluateFilters(video, filters));
        }

        // Duration Filter Tests
        [Fact]
        public void EvaluateFilters_DurationWithinRange_ReturnsTrue()
        {
            var video = CreateTestVideo(duration: TimeSpan.FromMinutes(15));
            var filters = new PlaylistFilters
            {
                Duration = new DurationFilter { MinSeconds = 300, MaxSeconds = 1800 }
            };

            Assert.True(_filterService.EvaluateFilters(video, filters));
        }

        [Fact]
        public void EvaluateFilters_DurationBelowMin_ReturnsFalse()
        {
            var video = CreateTestVideo(duration: TimeSpan.FromSeconds(100));
            var filters = new PlaylistFilters
            {
                Duration = new DurationFilter { MinSeconds = 300, MaxSeconds = 1800 }
            };

            Assert.False(_filterService.EvaluateFilters(video, filters));
        }

        [Fact]
        public void EvaluateFilters_DurationAboveMax_ReturnsFalse()
        {
            var video = CreateTestVideo(duration: TimeSpan.FromMinutes(60));
            var filters = new PlaylistFilters
            {
                Duration = new DurationFilter { MinSeconds = 300, MaxSeconds = 1800 }
            };

            Assert.False(_filterService.EvaluateFilters(video, filters));
        }

        // Published Time Filter Tests
        [Fact]
        public void EvaluateFilters_PublishedTimeRelativeWithinDays_ReturnsTrue()
        {
            var video = CreateTestVideo(publishedAt: DateTime.UtcNow.AddDays(-5));
            var filters = new PlaylistFilters
            {
                PublishedTime = new PublishedTimeFilter { Type = "relative", Days = 30 }
            };

            Assert.True(_filterService.EvaluateFilters(video, filters));
        }

        [Fact]
        public void EvaluateFilters_PublishedTimeRelativeBeyondDays_ReturnsFalse()
        {
            var video = CreateTestVideo(publishedAt: DateTime.UtcNow.AddDays(-40));
            var filters = new PlaylistFilters
            {
                PublishedTime = new PublishedTimeFilter { Type = "relative", Days = 30 }
            };

            Assert.False(_filterService.EvaluateFilters(video, filters));
        }

        [Fact]
        public void EvaluateFilters_PublishedTimeAbsoluteWithinRange_ReturnsTrue()
        {
            var video = CreateTestVideo(publishedAt: DateTime.UtcNow.AddDays(-10));
            var filters = new PlaylistFilters
            {
                PublishedTime = new PublishedTimeFilter
                {
                    Type = "absolute",
                    StartDate = DateTime.UtcNow.AddDays(-30),
                    EndDate = DateTime.UtcNow
                }
            };

            Assert.True(_filterService.EvaluateFilters(video, filters));
        }

        [Fact]
        public void EvaluateFilters_PublishedTimeAbsoluteBeforeStart_ReturnsFalse()
        {
            var video = CreateTestVideo(publishedAt: DateTime.UtcNow.AddDays(-50));
            var filters = new PlaylistFilters
            {
                PublishedTime = new PublishedTimeFilter
                {
                    Type = "absolute",
                    StartDate = DateTime.UtcNow.AddDays(-30),
                    EndDate = DateTime.UtcNow
                }
            };

            Assert.False(_filterService.EvaluateFilters(video, filters));
        }

        // Language Filter Tests
        [Fact]
        public void EvaluateFilters_LanguageMatches_ReturnsTrue()
        {
            var video = CreateTestVideo(language: "en");
            var filters = new PlaylistFilters
            {
                Language = new LanguageFilter { Preferred = "en" }
            };

            Assert.True(_filterService.EvaluateFilters(video, filters));
        }

        [Fact]
        public void EvaluateFilters_LanguageDoesNotMatch_ReturnsFalse()
        {
            var video = CreateTestVideo(language: "no");
            var filters = new PlaylistFilters
            {
                Language = new LanguageFilter { Preferred = "en" }
            };

            Assert.False(_filterService.EvaluateFilters(video, filters));
        }

        [Fact]
        public void EvaluateFilters_LanguagePartialMatch_ReturnsTrue()
        {
            var video = CreateTestVideo(language: "en-US");
            var filters = new PlaylistFilters
            {
                Language = new LanguageFilter { Preferred = "en" }
            };

            Assert.True(_filterService.EvaluateFilters(video, filters));
        }

        // Content Type Filter Tests
        [Fact]
        public void EvaluateFilters_ContentTypeVideoAllowed_ReturnsTrue()
        {
            var video = CreateTestVideo(contentType: "video");
            var filters = new PlaylistFilters
            {
                ContentType = new ContentTypeFilter { Videos = true, Livestreams = false, Shorts = false }
            };

            Assert.True(_filterService.EvaluateFilters(video, filters));
        }

        [Fact]
        public void EvaluateFilters_ContentTypeVideoNotAllowed_ReturnsFalse()
        {
            var video = CreateTestVideo(contentType: "video");
            var filters = new PlaylistFilters
            {
                ContentType = new ContentTypeFilter { Videos = false, Livestreams = true, Shorts = true }
            };

            Assert.False(_filterService.EvaluateFilters(video, filters));
        }

        [Fact]
        public void EvaluateFilters_ContentTypeShort_ReturnsTrue()
        {
            var video = CreateTestVideo(contentType: "short", duration: TimeSpan.FromSeconds(30));
            var filters = new PlaylistFilters
            {
                ContentType = new ContentTypeFilter { Videos = false, Livestreams = false, Shorts = true }
            };

            Assert.True(_filterService.EvaluateFilters(video, filters));
        }

        // Popularity Filter Tests
        [Fact]
        public void EvaluateFilters_PopularityAllConditionsMet_ReturnsTrue()
        {
            var video = CreateTestVideo(viewCount: 10000, likeCount: 500);
            var filters = new PlaylistFilters
            {
                Popularity = new PopularityFilter { MinViews = 1000, MinLikes = 100, MinLikeRatio = 0.01 }
            };

            Assert.True(_filterService.EvaluateFilters(video, filters));
        }

        [Fact]
        public void EvaluateFilters_PopularityBelowMinViews_ReturnsFalse()
        {
            var video = CreateTestVideo(viewCount: 500, likeCount: 50);
            var filters = new PlaylistFilters
            {
                Popularity = new PopularityFilter { MinViews = 1000, MinLikes = 100, MinLikeRatio = 0.01 }
            };

            Assert.False(_filterService.EvaluateFilters(video, filters));
        }

        [Fact]
        public void EvaluateFilters_PopularityBelowMinLikeRatio_ReturnsFalse()
        {
            var video = CreateTestVideo(viewCount: 10000, likeCount: 50); // 0.005 ratio
            var filters = new PlaylistFilters
            {
                Popularity = new PopularityFilter { MinViews = 0, MinLikes = 0, MinLikeRatio = 0.01 }
            };

            Assert.False(_filterService.EvaluateFilters(video, filters));
        }

        // Channel Filter Tests
        [Fact]
        public void EvaluateFilters_ChannelInIncludeList_ReturnsTrue()
        {
            var video = CreateTestVideo(channelId: "UC_tech_channel");
            var filters = new PlaylistFilters
            {
                Channels = new ChannelFilter
                {
                    Include = new List<string> { "UC_tech_channel", "UC_gaming_channel" }
                }
            };

            Assert.True(_filterService.EvaluateFilters(video, filters));
        }

        [Fact]
        public void EvaluateFilters_ChannelNotInIncludeList_ReturnsFalse()
        {
            var video = CreateTestVideo(channelId: "UC_other_channel");
            var filters = new PlaylistFilters
            {
                Channels = new ChannelFilter
                {
                    Include = new List<string> { "UC_tech_channel", "UC_gaming_channel" }
                }
            };

            Assert.False(_filterService.EvaluateFilters(video, filters));
        }

        [Fact]
        public void EvaluateFilters_ChannelInExcludeList_ReturnsFalse()
        {
            var video = CreateTestVideo(channelId: "UC_spam_channel");
            var filters = new PlaylistFilters
            {
                Channels = new ChannelFilter
                {
                    Exclude = new List<string> { "UC_spam_channel", "UC_ads_channel" }
                }
            };

            Assert.False(_filterService.EvaluateFilters(video, filters));
        }

        [Fact]
        public void EvaluateFilters_ChannelNotInExcludeList_ReturnsTrue()
        {
            var video = CreateTestVideo(channelId: "UC_good_channel");
            var filters = new PlaylistFilters
            {
                Channels = new ChannelFilter
                {
                    Exclude = new List<string> { "UC_spam_channel", "UC_ads_channel" }
                }
            };

            Assert.True(_filterService.EvaluateFilters(video, filters));
        }

        // Combined Filter Tests
        [Fact]
        public void EvaluateFilters_MultipleFiltersCombined_AllPass_ReturnsTrue()
        {
            var video = CreateTestVideo(
                title: "Unity Game Development Tutorial",
                duration: TimeSpan.FromMinutes(30),
                publishedAt: DateTime.UtcNow.AddDays(-10),
                viewCount: 50000,
                likeCount: 2000,
                channelId: "UC_tech_channel"
            );

            var filters = new PlaylistFilters
            {
                Themes = new List<string> { "game" },
                IncludeKeywords = new List<string> { "unity", "development" },
                Duration = new DurationFilter { MinSeconds = 600, MaxSeconds = 3600 },
                PublishedTime = new PublishedTimeFilter { Type = "relative", Days = 30 },
                Popularity = new PopularityFilter { MinViews = 10000, MinLikes = 1000 },
                Channels = new ChannelFilter { Include = new List<string> { "UC_tech_channel" } }
            };

            Assert.True(_filterService.EvaluateFilters(video, filters));
        }

        [Fact]
        public void EvaluateFilters_MultipleFiltersCombined_OneFails_ReturnsFalse()
        {
            var video = CreateTestVideo(
                title: "Cooking Tutorial",
                duration: TimeSpan.FromMinutes(30)
            );

            var filters = new PlaylistFilters
            {
                Themes = new List<string> { "gaming" }, // Will fail
                IncludeKeywords = new List<string> { "cooking" }
            };

            Assert.False(_filterService.EvaluateFilters(video, filters));
        }

        // NormalizeVideoData Tests
        [Fact]
        public void NormalizeVideoData_ConvertsLanguageToLowercase()
        {
            var video = CreateTestVideo(language: "EN");

            var normalized = _filterService.NormalizeVideoData(video);

            Assert.Equal("en", normalized.Language);
        }

        [Fact]
        public void NormalizeVideoData_ConvertsContentTypeToLowercase()
        {
            var video = CreateTestVideo(contentType: "VIDEO");

            var normalized = _filterService.NormalizeVideoData(video);

            Assert.Equal("video", normalized.ContentType);
        }

        // Edge Cases
        [Fact]
        public void EvaluateFilters_NullFilters_ReturnsTrue()
        {
            var video = CreateTestVideo();

            Assert.True(_filterService.EvaluateFilters(video, null));
        }

        [Fact]
        public void EvaluateFilters_NullVideo_ReturnsTrue()
        {
            var filters = new PlaylistFilters { Themes = new List<string> { "gaming" } };

            Assert.True(_filterService.EvaluateFilters(null, filters));
        }

        [Fact]
        public void EvaluateFilters_NullThemeButOtherFilters_FiltersApplied()
        {
            var video = CreateTestVideo(duration: TimeSpan.FromSeconds(100));
            var filters = new PlaylistFilters
            {
                Themes = null,
                Duration = new DurationFilter { MinSeconds = 300, MaxSeconds = 3600 }
            };

            Assert.False(_filterService.EvaluateFilters(video, filters));
        }

        [Fact]
        public void EvaluateFilters_CaseInsensitiveThemeMatching()
        {
            var video = CreateTestVideo(title: "GAMING Tutorial");
            var filters = new PlaylistFilters
            {
                Themes = new List<string> { "gaming" }
            };

            Assert.True(_filterService.EvaluateFilters(video, filters));
        }

        [Fact]
        public void EvaluateFilters_CaseInsensitiveChannelMatching()
        {
            var video = CreateTestVideo(channelId: "UC_Tech_Channel");
            var filters = new PlaylistFilters
            {
                Channels = new ChannelFilter
                {
                    Include = new List<string> { "uc_tech_channel" }
                }
            };

            Assert.True(_filterService.EvaluateFilters(video, filters));
        }
    }
}
