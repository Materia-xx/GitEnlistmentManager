namespace GitEnlistmentManager.DTOs.CommandSetFilters
{
    public interface ICommandSetFilter
    {
        bool Matches(RepoCollection? repoCollection, Repo? repo, Bucket? bucket, Enlistment? enlistment);
    }
}
