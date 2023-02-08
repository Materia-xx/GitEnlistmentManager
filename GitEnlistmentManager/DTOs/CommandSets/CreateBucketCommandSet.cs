using GitEnlistmentManager.DTOs.Commands;

namespace GitEnlistmentManager.DTOs.CommandSets
{
    public class CreateBucketCommandSet : CommandSet
    {
        public CreateBucketCommandSet()
        {
            Placement = CommandSetPlacement.Repo;
            OverrideKey = "createbucket";
            RightClickText = "Create bucket";
            Verb = "createbucket";
            Filename = "gemcreatebucket.cmdjson";

            Commands.Add(
                new CreateBucketCommand()
            );
        }
    }
}
