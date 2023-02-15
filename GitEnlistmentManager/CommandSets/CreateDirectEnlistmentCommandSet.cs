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

            Documentation = "Creates a direct enlistment";
        }
    }
}
