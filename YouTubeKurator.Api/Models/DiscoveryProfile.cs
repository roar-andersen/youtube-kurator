using System.Collections.Generic;

namespace YouTubeKurator.Api.Models
{
    public class DiscoveryProfile
    {
        public int Strict { get; set; } = 70;
        public int Relaxed { get; set; } = 20;
        public int Wild { get; set; } = 10;
        public bool EnableWildcards { get; set; } = true;
        public List<string> WildcardTypes { get; set; } = new()
        {
            "sametheme_otherlanguage",
            "samechannel_otherformat",
            "lowpop_highquality",
            "relatedchannels"
        };
    }
}
