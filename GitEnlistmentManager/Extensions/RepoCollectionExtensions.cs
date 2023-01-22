using GitEnlistmentManager.DTOs;
using System.Collections.Generic;

namespace GitEnlistmentManager.Extensions
{
    public static class RepoCollectionExtensions
    {
        public static Dictionary<string, string> GetTokens(this RepoCollection repoCollection)
        {
            var tokens = repoCollection.Gem.GetTokens();
            tokens["RepoCollectionFolderPath"] = repoCollection.RepoCollectionFolderPath;
            if (repoCollection.Name != null)
            {
                tokens["RepoCollectionName"] = repoCollection.Name;
            }
            return tokens;
        }
    }
}
