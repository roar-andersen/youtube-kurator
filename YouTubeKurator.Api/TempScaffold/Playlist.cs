using System;
using System.Collections.Generic;

namespace YouTubeKurator.Api.TempScaffold;

public partial class Playlist
{
    public string Id { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string SearchQuery { get; set; } = null!;

    public string CreatedUtc { get; set; } = null!;

    public string UpdatedUtc { get; set; } = null!;
}
