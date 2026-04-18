using GitEnlistmentManager.Commands;

namespace GitEnlistmentManager.CommandSets
{
    public class DeleteBucketCommandSet : CommandSet
    {
        public DeleteBucketCommandSet()
        {
            Placement = CommandSetPlacement.Bucket;
            OverrideKey = "deletebucket";
            RightClickText = "Delete bucket";
            Verb = "deletebucket";
            Filename = "gemdeletebucketui.cmdjson";

            Commands.Add(new DeleteBucketCommand());
            Commands.Add(new RefreshTreeviewCommand());

            Documentation = "Deletes an empty bucket. The bucket must have no enlistments. Do NOT automatically archive enlistments to make a bucket empty. Only archive an enlistment when the user has confirmed the PR is completed or the user specifically asks to archive a particular enlistment by a non-ambiguous name.";
        }
    }
}
