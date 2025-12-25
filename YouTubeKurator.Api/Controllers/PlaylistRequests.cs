namespace YouTubeKurator.Api.Controllers
{
    public class CreatePlaylistRequest
    {
        public string Name { get; set; }
        public string SearchQuery { get; set; }
        public string? Filters { get; set; }
    }

    public class UpdatePlaylistRequest
    {
        public string Name { get; set; }
        public string SearchQuery { get; set; }
        public bool? EnableDiscovery { get; set; }
        public string? Filters { get; set; }
    }
}
