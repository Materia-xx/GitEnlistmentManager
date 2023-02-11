using GitEnlistmentManager.DTOs.Commands;

namespace GitEnlistmentManager.DTOs.CommandSets
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

            this.Commands.Add(new CreateBucketCommand());
            this.Commands.Add(new RefreshTreeviewCommand());

            this.CommandSetDocumentation = "Creates a bucket.";
        }
    }
}
