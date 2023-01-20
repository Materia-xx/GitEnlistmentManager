using System.Collections.Generic;

namespace GitEnlistmentManager.DTOs
{
    public class RepoCollection
    {
        public Gem Gem { get; }
        public string? Name { get; set; }

        public string RepoCollectionFolderPath { get; }
        public List<Repo> Repos { get; } = new List<Repo>();

        public RepoCollection(Gem gem, string repoCollectionFolderPath)
        {
            this.Gem = gem;
            RepoCollectionFolderPath = repoCollectionFolderPath;
        }
    }
}
