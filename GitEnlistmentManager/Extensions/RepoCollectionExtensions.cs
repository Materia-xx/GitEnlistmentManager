﻿using GitEnlistmentManager.DTOs;
using System.Collections.Generic;

namespace GitEnlistmentManager.Extensions
{
    public static class RepoCollectionExtensions
    {
        public static Dictionary<string, string> GetTokens(this RepoCollection repoCollection)
        {
            var tokens = repoCollection.Gem.GetTokens();
            tokens["RepoCollectionDirectoryPath"] = repoCollection.RepoCollectionDirectoryPath;
            if (repoCollection.GemName != null)
            {
                tokens["RepoCollectionName"] = repoCollection.GemName;
            }
            return tokens;
        }
    }
}
