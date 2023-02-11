using System;
using GitEnlistmentManager.DTOs;

namespace GitEnlistmentManager.CommandSetFilters
{
    public class CommandSetFilterGemCompareOptionSet : ICommandSetFilter
    {
        public bool Matches(RepoCollection? repoCollection, Repo? repo, Bucket? bucket, Enlistment? enlistment)
        {
            if (repoCollection == null)
            {
                return false;
            }

            return !string.IsNullOrWhiteSpace(repoCollection.Gem.LocalAppData.CompareProgram);
        }
    }
}
