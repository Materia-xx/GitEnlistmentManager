using System.Collections.Generic;

namespace GitEnlistmentManager.DTOs
{
    public class Repo
    {
        public MetadataFolder MetadataFolder { get; }
        public string? Name { get; set; }
        public RepoMetadata Metadata { get; set; } = new RepoMetadata();
        public List<Bucket> Buckets { get; } = new List<Bucket>();

        public Repo(MetadataFolder metadataFolder)
        {
            this.MetadataFolder = metadataFolder;
        }
    }
}
