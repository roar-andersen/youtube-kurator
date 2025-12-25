using System;

namespace YouTubeKurator.Api.Models
{
    public record AddWatchLaterRequest(string VideoId, Guid? PlaylistId = null);
}
