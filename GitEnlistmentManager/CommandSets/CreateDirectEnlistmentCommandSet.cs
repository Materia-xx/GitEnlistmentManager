using GitEnlistmentManager.Commands;

namespace GitEnlistmentManager.CommandSets
{
    public class CreateDirectEnlistmentCommandSet : CommandSet
    {
        public CreateDirectEnlistmentCommandSet()
        {
            Placement = CommandSetPlacement.Bucket;
            OverrideKey = "createdirectenlistment";
            RightClickText = "Create Direct Enlistment";
            Verb = "createdirectenlistment";
            Filename = "gemcreatedirectenlistment.cmdjson";

            Commands.Add(new CreateDirectEnlistmentCommand());
            Commands.Add(new RefreshTreeviewCommand());

            Documentation = "Creates a new enlistment by performing a fresh `git clone` of the repo's clone URL directly (no parent worktree, no branch stacking). Path must resolve to a bucket. Use this instead of `createenlistment` when the user does NOT want the new enlistment branched off an existing one in the bucket. Preconditions: clone URL, user name and email configured. Side effects: full clone on disk, GEM tree refresh.";
        }
    }
}
