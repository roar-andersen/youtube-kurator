using System;
using System.Collections.Generic;

namespace YouTubeKurator.Api.TempScaffold;

public partial class CachedSearch
{
    public string Id { get; set; } = null!;

    public string SearchQuery { get; set; } = null!;

    public string ResultsJson { get; set; } = null!;

    public string FetchedUtc { get; set; } = null!;

    public string ExpiresUtc { get; set; } = null!;
}
