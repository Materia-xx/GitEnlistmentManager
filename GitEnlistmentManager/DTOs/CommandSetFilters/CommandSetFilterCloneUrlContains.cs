using System;

namespace GitEnlistmentManager.DTOs.CommandSetFilters
{
    public class CommandSetFilterCloneUrlContains : ICommandSetFilter
    {
        public string? SearchFor { get; set; }

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
