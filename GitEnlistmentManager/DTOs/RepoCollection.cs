using GitEnlistmentManager.Globals;
using System.Collections.Generic;

namespace GitEnlistmentManager.DTOs
{
    public class RepoCollection : GemTreeViewItem
    {
        public Gem Gem { get; }

        public string RepoCollectionFolderPath { get; }
        public List<Repo> Repos { get; } = new List<Repo>();

        public RepoCollection(Gem gem, string repoCollectionFolderPath)
        {
            this.Gem = gem;
            RepoCollectionFolderPath = repoCollectionFolderPath;
            this.Icon = Icons.GetBitMapImage(@"repocollection.png");
        }
    }
}
