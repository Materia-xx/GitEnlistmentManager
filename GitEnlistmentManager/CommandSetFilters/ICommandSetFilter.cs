using GitEnlistmentManager.DTOs;

namespace GitEnlistmentManager.CommandSetFilters
{
    public interface ICommandSetFilter
    {
        bool Matches(RepoCollection? repoCollection, Repo? repo, Bucket? bucket, Enlistment? enlistment);

        string Documentation { get; }
    }
}
