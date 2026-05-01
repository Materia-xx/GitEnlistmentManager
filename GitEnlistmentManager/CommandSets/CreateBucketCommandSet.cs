using GitEnlistmentManager.Commands;

namespace GitEnlistmentManager.CommandSets
{
    public class CreateBucketCommandSet : CommandSet
    {
        public CreateBucketCommandSet()
        {
            Placement = CommandSetPlacement.TargetBranch;
            OverrideKey = "createbucket";
            RightClickText = "Create Bucket";
            Verb = "createbucket";
            Filename = "gemcreatebucket.cmdjson";

            Commands.Add(new CreateBucketCommand());
            Commands.Add(new RefreshTreeviewCommand());

            Documentation = "Creates a new bucket directory under a target branch. Path must resolve to a target branch. A bucket is a container for a stack of related enlistments (worktrees) targeting the same branch. Refreshes the GEM tree on success.";
        }
    }
}
