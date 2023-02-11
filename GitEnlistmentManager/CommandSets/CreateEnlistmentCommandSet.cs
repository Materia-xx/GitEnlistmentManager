using GitEnlistmentManager.Commands;

namespace GitEnlistmentManager.CommandSets
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

            Commands.Add(new CreateEnlistmentCommand());
            Commands.Add(new RefreshTreeviewCommand());

            CommandSetDocumentation = "Creates an enlistment";
        }
    }
}
