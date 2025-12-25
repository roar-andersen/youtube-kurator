using System;
using System.Collections.Generic;
using System.Linq;
using YouTubeKurator.Api.Data.Entities;
using YouTubeKurator.Api.Models;

namespace YouTubeKurator.Api.Services
{
    public interface ISortingService
    {
        /// <summary>
        /// Sorts a collection of videos using the specified sorting strategy.
        /// Returns an ordered enumerable that can be further filtered or converted to a list.
        /// </summary>
        IOrderedEnumerable<Video> Sort(IEnumerable<Video> videos, SortStrategy strategy);
    }
}
