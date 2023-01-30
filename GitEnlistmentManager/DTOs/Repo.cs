using GitEnlistmentManager.Globals;
using System.Collections.Generic;

namespace GitEnlistmentManager.DTOs
{
    public class Repo : GemTreeViewItem
    {
        public RepoCollection RepoCollection { get; }
        public RepoMetadata Metadata { get; set; } = new RepoMetadata();
        public List<Bucket> Buckets { get; } = new List<Bucket>();

        public Repo(RepoCollection repoCollection)
        {
            this.RepoCollection = repoCollection;
            this.Icon = Icons.GetBitMapImage(@"repo.png");
        }
    }
}
