using GitEnlistmentManager.Extensions;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GitEnlistmentManager.DTOs
{
    public class GemNodeContext
    {
        public RepoCollection? RepoCollection { get; set; }

        public Repo? Repo { get; set; }

        public Bucket? Bucket { get; set; }

        public Enlistment? Enlistment { get; set; }

        public string? GetWorkingFolder()
        {
            return Enlistment?.GetDirectoryInfo()?.FullName ?? Bucket?.GetDirectoryInfo()?.FullName ?? Repo?.GetDirectoryInfo()?.FullName ?? RepoCollection?.RepoCollectionFolderPath;
        }

        public GemNodeContext Clone()
        {
            return new GemNodeContext()
            {
                RepoCollection = this.RepoCollection,
                Repo= this.Repo,
                Bucket = this.Bucket,
                Enlistment = this.Enlistment
            };
        }

        public async Task<Dictionary<string, string>> GetTokens()
        {
            if (this.Enlistment != null)
            {
                return await this.Enlistment.GetTokens().ConfigureAwait(false);
            }

            if (this.Bucket != null)
            {
                return this.Bucket.GetTokens();
            }

            if (this.Repo != null)
            {
                return this.Repo.GetTokens();
            }

            if (this.RepoCollection != null)
            {
                return this.RepoCollection.GetTokens();
            }

            return new Dictionary<string, string>();
        }

        public CommandSetPlacement GetPlacement()
        {
            return Enlistment != null ? CommandSetPlacement.Enlistment :
                   Bucket != null ? CommandSetPlacement.Bucket :
                   Repo != null ? CommandSetPlacement.Repo :
                   CommandSetPlacement.RepoCollection;
        }

        public static GemNodeContext GetNodeContext(RepoCollection? repoCollection = null, Repo? repo = null, Bucket? bucket = null, Enlistment? enlistment = null)
        {
            var nodeContext = new GemNodeContext()
            {
                RepoCollection = enlistment?.Bucket.Repo.RepoCollection ?? bucket?.Repo.RepoCollection ?? repo?.RepoCollection ?? repoCollection,
                Repo = enlistment?.Bucket.Repo ?? bucket?.Repo ?? repo,
                Bucket = enlistment?.Bucket ?? bucket,
                Enlistment = enlistment
            };
            return nodeContext;
        }
    }
}
