using GitEnlistmentManager.DTOs.Commands;

namespace GitEnlistmentManager.DTOs.CommandSets
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

            this.Commands.Add(new CreateEnlistmentCommand());
            this.Commands.Add(new RefreshTreeviewCommand());
        }
    }
}
