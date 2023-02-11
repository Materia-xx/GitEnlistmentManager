using GitEnlistmentManager.DTOs.Commands;

namespace GitEnlistmentManager.DTOs.CommandSets
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

            this.Commands.Add(new DeleteBucketCommand());
            this.Commands.Add(new RefreshTreeviewCommand());

            this.CommandSetDocumentation = "Deletes an empty bucket.";
        }
    }
}
