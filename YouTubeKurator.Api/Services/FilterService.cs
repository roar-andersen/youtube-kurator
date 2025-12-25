using System;
using System.Collections.Generic;
using System.Linq;
using YouTubeKurator.Api.Data.Entities;
using YouTubeKurator.Api.Models;

namespace YouTubeKurator.Api.Services
{
    public class FilterService : IFilterService
    {
        public bool EvaluateFilters(Video video, PlaylistFilters filters)
        {
            if (video == null || filters == null)
                return true;

            // Theme filter - OR logic (video must contain at least one theme)
            if (filters.Themes != null && filters.Themes.Count > 0)
            {
                if (!EvaluateThemes(video, filters.Themes))
                    return false;
            }

            // Include Keywords filter - AND logic (video must contain all keywords)
            if (filters.IncludeKeywords != null && filters.IncludeKeywords.Count > 0)
            {
                if (!EvaluateIncludeKeywords(video, filters.IncludeKeywords))
                    return false;
            }

            // Exclude Keywords filter - NOT logic (video must NOT contain any excluded keywords)
            if (filters.ExcludeKeywords != null && filters.ExcludeKeywords.Count > 0)
            {
                if (!EvaluateExcludeKeywords(video, filters.ExcludeKeywords))
                    return false;
            }

            // Duration filter
            if (filters.Duration != null)
            {
                if (!EvaluateDuration(video, filters.Duration))
                    return false;
            }

            // Published Time filter
            if (filters.PublishedTime != null)
            {
                if (!EvaluatePublishedTime(video, filters.PublishedTime))
                    return false;
            }

            // Language filter
            if (filters.Language != null)
            {
                if (!EvaluateLanguage(video, filters.Language))
                    return false;
            }

            // Content Type filter
            if (filters.ContentType != null)
            {
                if (!EvaluateContentType(video, filters.ContentType))
                    return false;
            }

            // Popularity filter
            if (filters.Popularity != null)
            {
                if (!EvaluatePopularity(video, filters.Popularity))
                    return false;
            }

            // Channel filter
            if (filters.Channels != null)
            {
                if (!EvaluateChannels(video, filters.Channels))
                    return false;
            }

            return true;
        }

        public Video NormalizeVideoData(Video video)
        {
            // Normalize language code to lowercase
            if (!string.IsNullOrWhiteSpace(video.Language))
            {
                video.Language = video.Language.ToLower();
            }

            // Ensure ContentType is lowercase
            if (!string.IsNullOrWhiteSpace(video.ContentType))
            {
                video.ContentType = video.ContentType.ToLower();
            }

            return video;
        }

        private bool EvaluateThemes(Video video, List<string> themes)
        {
            if (themes == null || themes.Count == 0)
                return true;

            var videoTitle = video.Title?.ToLower() ?? "";
            var videoChannel = video.ChannelName?.ToLower() ?? "";

            // At least one theme must match in title or channel name
            return themes.Any(theme =>
                videoTitle.Contains(theme.ToLower()) ||
                videoChannel.Contains(theme.ToLower())
            );
        }

        private bool EvaluateIncludeKeywords(Video video, List<string> keywords)
        {
            if (keywords == null || keywords.Count == 0)
                return true;

            var videoTitle = video.Title?.ToLower() ?? "";

            // ALL keywords must be present in title
            return keywords.All(keyword =>
                videoTitle.Contains(keyword.ToLower())
            );
        }

        private bool EvaluateExcludeKeywords(Video video, List<string> keywords)
        {
            if (keywords == null || keywords.Count == 0)
                return true;

            var videoTitle = video.Title?.ToLower() ?? "";

            // NONE of the excluded keywords should be present
            return !keywords.Any(keyword =>
                videoTitle.Contains(keyword.ToLower())
            );
        }

        private bool EvaluateDuration(Video video, DurationFilter duration)
        {
            if (duration == null)
                return true;

            var seconds = (long)video.Duration.TotalSeconds;
            return seconds >= duration.MinSeconds && seconds <= duration.MaxSeconds;
        }

        private bool EvaluatePublishedTime(Video video, PublishedTimeFilter publishedTime)
        {
            if (publishedTime == null)
                return true;

            var now = DateTime.UtcNow;

            if (publishedTime.Type == "relative")
            {
                // Relative: check if published within the last N days
                var daysAgo = now.AddDays(-publishedTime.Days);
                return video.PublishedAt >= daysAgo;
            }
            else if (publishedTime.Type == "absolute")
            {
                // Absolute: check if published between startDate and endDate
                if (publishedTime.StartDate.HasValue && video.PublishedAt < publishedTime.StartDate)
                    return false;

                if (publishedTime.EndDate.HasValue && video.PublishedAt > publishedTime.EndDate)
                    return false;

                return true;
            }

            return true;
        }

        private bool EvaluateLanguage(Video video, LanguageFilter language)
        {
            if (language == null)
                return true;

            // If preferred language is specified, check if video language matches
            if (!string.IsNullOrWhiteSpace(language.Preferred))
            {
                var videoLang = video.Language?.ToLower() ?? "";
                var preferredLang = language.Preferred.ToLower();

                // Support both full codes (en-US) and short codes (en)
                if (!videoLang.StartsWith(preferredLang))
                    return false;
            }

            // Region filter could be implemented if needed
            // For now, we only check preferred language

            return true;
        }

        private bool EvaluateContentType(Video video, ContentTypeFilter contentType)
        {
            if (contentType == null)
                return true;

            var videoType = video.ContentType?.ToLower() ?? "video";

            return (videoType == "video" && contentType.Videos) ||
                   (videoType == "livestream" && contentType.Livestreams) ||
                   (videoType == "short" && contentType.Shorts);
        }

        private bool EvaluatePopularity(Video video, PopularityFilter popularity)
        {
            if (popularity == null)
                return true;

            // Check minimum views
            if (video.ViewCount < popularity.MinViews)
                return false;

            // Check minimum likes
            if (video.LikeCount < popularity.MinLikes)
                return false;

            // Check minimum like ratio (likes / views)
            if (popularity.MinLikeRatio > 0 && video.ViewCount > 0)
            {
                var likeRatio = (double)video.LikeCount / video.ViewCount;
                if (likeRatio < popularity.MinLikeRatio)
                    return false;
            }

            return true;
        }

        private bool EvaluateChannels(Video video, ChannelFilter channels)
        {
            if (channels == null)
                return true;

            var channelId = video.ChannelId ?? "";

            // If channel is in exclude list, reject it
            if (channels.Exclude != null && channels.Exclude.Count > 0)
            {
                if (channels.Exclude.Any(ch => ch.Equals(channelId, StringComparison.OrdinalIgnoreCase)))
                    return false;
            }

            // If include list is specified, channel must be in it
            if (channels.Include != null && channels.Include.Count > 0)
            {
                if (!channels.Include.Any(ch => ch.Equals(channelId, StringComparison.OrdinalIgnoreCase)))
                    return false;
            }

            return true;
        }
    }
}
