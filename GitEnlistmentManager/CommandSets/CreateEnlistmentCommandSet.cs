using GitEnlistmentManager.Commands;

namespace GitEnlistmentManager.CommandSets
{
    public class CreateEnlistmentCommandSet : CommandSet
    {
        public CreateEnlistmentCommandSet()
        {
            Placement = CommandSetPlacement.Bucket;
            OverrideKey = "createenlistment";
            RightClickText = "Create Enlistment";
            Verb = "createenlistment";
            Filename = "gemcreateenlistment.cmdjson";

            Commands.Add(new CreateEnlistmentCommand());
            Commands.Add(new RefreshTreeviewCommand());

            Documentation = "Creates a new enlistment as a git worktree branched off the bucket's most recent enlistment (or, if the bucket is empty, off the bucket's target branch). Path must resolve to a bucket. Preconditions: the repo must have a clone URL, branch prefix, and user name + email configured in repo settings. Side effects: creates a directory, runs git operations, refreshes the GEM tree. The new branch name is derived from the bucket name and a numeric prefix.";
        }
    }
}
