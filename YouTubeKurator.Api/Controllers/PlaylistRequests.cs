namespace YouTubeKurator.Api.Controllers
{
    public class CreatePlaylistRequest
    {
        public required string Name { get; set; }
        public string? SearchQuery { get; set; }  // Valgfritt - søkeord er bare ett av mange filtrer
        public string? Filters { get; set; }       // Påkrevd - minst ett filter må være satt
    }

    public class UpdatePlaylistRequest
    {
        public required string Name { get; set; }
        public string? SearchQuery { get; set; }  // Valgfritt - søkeord er bare ett av mange filtrer
        public bool? EnableDiscovery { get; set; }
        public string? Filters { get; set; }
    }
}
