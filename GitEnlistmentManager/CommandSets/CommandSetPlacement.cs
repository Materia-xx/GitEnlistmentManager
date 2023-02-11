namespace GitEnlistmentManager.CommandSets
{
    public enum CommandSetPlacement
    {
        /// <summary>
        /// A command set that is available everywhere
        /// </summary>
        All,
        /// <summary>
        /// Gem placement is with no item selected
        /// </summary>
        Gem,
        RepoCollection,
        Repo,
        Bucket,
        Enlistment,
        AfterEnlistmentCreate,
        AfterBucketCreate
    }
}
