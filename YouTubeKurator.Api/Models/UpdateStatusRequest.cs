using System;

namespace YouTubeKurator.Api.Models
{
    public record UpdateStatusRequest(
        Guid PlaylistId,
        string Status,
        string? RejectReason = null
    );
}
