using System.Collections.Generic;

namespace GitEnlistmentManager.DTOs
{
    public class Gem
    {
        public GemMetadata Metadata { get; set; } = new GemMetadata();

        public List<Repo> Repos { get; } = new List<Repo>();
    }
}
