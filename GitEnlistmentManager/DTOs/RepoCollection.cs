using GitEnlistmentManager.Globals;
using System.Collections.Generic;

namespace GitEnlistmentManager.DTOs
{
    public class RepoCollection : GemTreeViewItem
    {
        public Gem Gem { get; }

        public string RepoCollectionDirectoryPath { get; }
        public List<Repo> Repos { get; } = new List<Repo>();

        public RepoCollection(Gem gem, string repoCollectionDirectoryPath)
        {
            this.Gem = gem;
            RepoCollectionDirectoryPath = repoCollectionDirectoryPath;
            this.Icon = Icons.GetBitMapImage(@"repocollection.png");
        }
    }
}
