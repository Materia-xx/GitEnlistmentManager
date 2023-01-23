using GitEnlistmentManager.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

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

        public Dictionary<string, string> GetTokens()
        {
            return Enlistment?.GetTokens() ?? Bucket?.GetTokens() ?? Repo?.GetTokens() ?? RepoCollection?.GetTokens() ?? new Dictionary<string, string>();
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
