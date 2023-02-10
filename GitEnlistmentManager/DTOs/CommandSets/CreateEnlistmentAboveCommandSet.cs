using GitEnlistmentManager.DTOs.Commands;

namespace GitEnlistmentManager.DTOs.CommandSets
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

            this.Commands.Add(new CreateEnlistmentAboveCommand());
            this.Commands.Add(new RefreshTreeviewCommand());
        }
    }
}
