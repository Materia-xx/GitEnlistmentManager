using GitEnlistmentManager.Commands;

namespace GitEnlistmentManager.CommandSets
{
    public class CreateBucketCommandSet : CommandSet
    {
        public CreateBucketCommandSet()
        {
            Placement = CommandSetPlacement.Repo;
            OverrideKey = "createbucket";
            RightClickText = "Create Bucket";
            Verb = "createbucket";
            Filename = "gemcreatebucket.cmdjson";

            Commands.Add(new CreateBucketCommand());
            Commands.Add(new RefreshTreeviewCommand());

            CommandSetDocumentation = "Creates a bucket.";
        }
    }
}
