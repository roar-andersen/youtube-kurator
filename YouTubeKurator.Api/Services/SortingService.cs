using System;
using System.Collections.Generic;
using System.Linq;
using YouTubeKurator.Api.Data.Entities;
using YouTubeKurator.Api.Models;

namespace YouTubeKurator.Api.Services
{
    public class SortingService : ISortingService
    {
        public IOrderedEnumerable<Video> Sort(IEnumerable<Video> videos, SortStrategy strategy)
        {
            return strategy switch
            {
                SortStrategy.NewestFirst => SortByNewestFirst(videos),
                SortStrategy.MostRelevant => SortByMostRelevant(videos),
                SortStrategy.MostPopular => SortByMostPopular(videos),
                SortStrategy.MostPopularRelative => SortByMostPopularRelative(videos),
                SortStrategy.HighestQuality => SortByHighestQuality(videos),
                SortStrategy.LengthShort => SortByLengthShort(videos),
                SortStrategy.LengthLong => SortByLengthLong(videos),
                SortStrategy.ChannelAuthority => SortByChannelAuthority(videos),
                SortStrategy.WeightedScore => SortByWeightedScore(videos),
                _ => SortByNewestFirst(videos) // Default fallback
            };
        }

        /// <summary>
        /// Sort by published date, newest first.
        /// </summary>
        private IOrderedEnumerable<Video> SortByNewestFirst(IEnumerable<Video> videos)
        {
            return videos.OrderByDescending(v => v.PublishedAt);
        }

        /// <summary>
        /// Sort by YouTube's relevance score (approximated by view count and like ratio).
        /// Uses a formula: ViewCount * LikeRatio
        /// </summary>
        private IOrderedEnumerable<Video> SortByMostRelevant(IEnumerable<Video> videos)
        {
            return videos.OrderByDescending(v =>
            {
                if (v.ViewCount == 0)
                    return 0;
                var likeRatio = (double)v.LikeCount / v.ViewCount;
                return v.ViewCount * likeRatio;
            });
        }

        /// <summary>
        /// Sort by view count, most popular first.
        /// </summary>
        private IOrderedEnumerable<Video> SortByMostPopular(IEnumerable<Video> videos)
        {
            return videos.OrderByDescending(v => v.ViewCount);
        }

        /// <summary>
        /// Sort by views per day (popularity relative to age).
        /// Newer videos get higher scores relative to older videos with similar views.
        /// </summary>
        private IOrderedEnumerable<Video> SortByMostPopularRelative(IEnumerable<Video> videos)
        {
            var now = DateTime.UtcNow;
            return videos.OrderByDescending(v =>
            {
                var ageInDays = (now - v.PublishedAt).TotalDays;
                // Prevent division by zero - videos published today get full view count
                if (ageInDays < 1)
                    return v.ViewCount;
                return v.ViewCount / ageInDays;
            });
        }

        /// <summary>
        /// Sort by like-to-view ratio (quality metric).
        /// Videos with higher engagement relative to views rank higher.
        /// </summary>
        private IOrderedEnumerable<Video> SortByHighestQuality(IEnumerable<Video> videos)
        {
            return videos.OrderByDescending(v =>
            {
                if (v.ViewCount == 0)
                    return 0;
                return (double)v.LikeCount / v.ViewCount;
            });
        }

        /// <summary>
        /// Sort by duration, shortest first.
        /// Useful for quick viewing or short-form content.
        /// </summary>
        private IOrderedEnumerable<Video> SortByLengthShort(IEnumerable<Video> videos)
        {
            return videos.OrderBy(v => v.Duration.TotalSeconds);
        }

        /// <summary>
        /// Sort by duration, longest first.
        /// Useful for in-depth content or long-form videos.
        /// </summary>
        private IOrderedEnumerable<Video> SortByLengthLong(IEnumerable<Video> videos)
        {
            return videos.OrderByDescending(v => v.Duration.TotalSeconds);
        }

        /// <summary>
        /// Sort by channel authority (estimated by average views per video on channel).
        /// Uses the channel name as a proxy - in production, would use channel-level stats.
        /// For now, uses view count as a proxy for channel authority.
        /// </summary>
        private IOrderedEnumerable<Video> SortByChannelAuthority(IEnumerable<Video> videos)
        {
            return videos.OrderByDescending(v =>
            {
                // Group by channel and calculate average views
                // Since we only have individual video data, we use ViewCount as a proxy
                return v.ViewCount;
            });
        }

        /// <summary>
        /// Sort by a weighted combination of multiple factors.
        /// Formula: (Relevance * 0.3) + (Quality * 0.25) + (PopularityRelative * 0.25) + (Recency * 0.2)
        /// </summary>
        private IOrderedEnumerable<Video> SortByWeightedScore(IEnumerable<Video> videos)
        {
            var now = DateTime.UtcNow;
            return videos.OrderByDescending(v =>
            {
                // Relevance score (0-1 normalized): viewCount * likeRatio
                var relevanceScore = v.ViewCount == 0 ? 0 :
                    Math.Min((double)v.ViewCount / 1000000, 1.0) *
                    (v.ViewCount > 0 ? (double)v.LikeCount / v.ViewCount : 0);

                // Quality score (0-1): like ratio
                var qualityScore = v.ViewCount == 0 ? 0 : (double)v.LikeCount / v.ViewCount;

                // Relative popularity score: views per day
                var ageInDays = (now - v.PublishedAt).TotalDays;
                var relativePopularityScore = ageInDays < 1 ?
                    Math.Min((double)v.ViewCount / 10000, 1.0) :
                    Math.Min((v.ViewCount / ageInDays) / 10000, 1.0);

                // Recency score (0-1): penalize old videos
                var daysOld = (now - v.PublishedAt).TotalDays;
                var recencyScore = Math.Max(1.0 - (daysOld / 365.0), 0.0); // 0 after 1 year

                // Weighted combination
                var weightedScore =
                    (relevanceScore * 0.3) +
                    (qualityScore * 0.25) +
                    (relativePopularityScore * 0.25) +
                    (recencyScore * 0.2);

                return weightedScore;
            });
        }
    }
}
