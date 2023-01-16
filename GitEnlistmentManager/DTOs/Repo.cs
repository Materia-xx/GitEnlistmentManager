using System.Collections.Generic;

namespace GitEnlistmentManager.DTOs
{
    public class Repo
    {
        public Gem Gem { get; }
        public string? Name { get; set; }
        public RepoMetadata Metadata { get; set; } = new RepoMetadata();
        public List<Bucket> Buckets { get; } = new List<Bucket>();

        public Repo(Gem gem)
        {
            this.Gem = gem;
        }
    }
}
