using System.Collections.Generic;

namespace GitEnlistmentManager.DTOs
{
    public class Repo
    {
        public RepoCollection RepoCollection { get; }
        public string? Name { get; set; }
        public RepoMetadata Metadata { get; set; } = new RepoMetadata();
        public List<Bucket> Buckets { get; } = new List<Bucket>();

        public Repo(RepoCollection repoCollection)
        {
            this.RepoCollection = repoCollection;
        }
    }
}
