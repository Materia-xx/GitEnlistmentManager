using System;
using GitEnlistmentManager.DTOs;

namespace GitEnlistmentManager.CommandSetFilters
{
    public class CommandSetFilterCloneUrlContains : ICommandSetFilter
    {
        public string? SearchFor { get; set; }
        public string Documentation => "True if a specific string is found within the repo's clone URL";

        public bool Matches(RepoCollection? repoCollection, Repo? repo, Bucket? bucket, Enlistment? enlistment)
        {
            if (SearchFor == null || repo?.Metadata.CloneUrl == null)
            {
                return false;
            }

            return repo.Metadata.CloneUrl.Contains(SearchFor, StringComparison.OrdinalIgnoreCase);
        }
    }
}
