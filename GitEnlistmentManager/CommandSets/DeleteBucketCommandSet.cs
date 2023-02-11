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
            Verb = string.Empty;
            Filename = "gemdeletebucketui.cmdjson";

            Commands.Add(new DeleteBucketCommand());
            Commands.Add(new RefreshTreeviewCommand());

            CommandSetDocumentation = "Deletes an empty bucket.";
        }
    }
}
