using GitEnlistmentManager.Commands;

namespace GitEnlistmentManager.CommandSets
{
    public class CreateEnlistmentAboveCommandSet : CommandSet
    {
        public CreateEnlistmentAboveCommandSet()
        {
            Placement = CommandSetPlacement.Enlistment;
            OverrideKey = "createenlistmentabove";
            RightClickText = "Create Enlistment Above";
            Verb = "createenlistmentabove";
            Filename = "gemcreateenlistmentabove.cmdjson";

            Commands.Add(new CreateEnlistmentAboveCommand());
            Commands.Add(new RefreshTreeviewCommand());

            Documentation = "Creates a enlistment above the selected one.";
        }
    }
}
