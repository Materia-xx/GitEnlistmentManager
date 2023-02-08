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

            Commands.Add(
                new DeleteBucketCommand()
            );
        }
    }
}
