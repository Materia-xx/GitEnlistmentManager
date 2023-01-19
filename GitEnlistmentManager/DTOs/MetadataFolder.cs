using System.Collections.Generic;

namespace GitEnlistmentManager.DTOs
{
    public class MetadataFolder
    {
        public Gem Gem { get; }
        public string? Name { get; set; }

        public string MetadataFolderPath { get; }
        public List<Repo> Repos { get; } = new List<Repo>();

        public MetadataFolder(Gem gem, string metadataFolderPath)
        {
            this.Gem = gem;
            MetadataFolderPath = metadataFolderPath;
        }
    }
}
