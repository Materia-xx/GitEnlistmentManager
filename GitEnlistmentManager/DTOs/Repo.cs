using GitEnlistmentManager.Globals;
using System.Collections.Generic;

namespace GitEnlistmentManager.DTOs
{
    public class Repo : GemTreeViewItem
    {
        public RepoCollection RepoCollection { get; }
        public RepoMetadata Metadata { get; set; } = new RepoMetadata();
        public List<TargetBranch> TargetBranches { get; } = new List<TargetBranch>();

        public Repo(RepoCollection repoCollection)
        {
            this.RepoCollection = repoCollection;
            this.Icon = Icons.GetBitMapImage(@"repo.png");
        }
    }
}
