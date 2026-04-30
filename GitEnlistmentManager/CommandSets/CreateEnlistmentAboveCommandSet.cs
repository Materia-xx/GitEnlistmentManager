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

            Documentation = "Inserts a new enlistment as the PARENT of the existing one at the path, restructuring the bucket's enlistment stack so the new one branches off the old parent and the existing one re-bases onto the new one. Path must resolve to an enlistment. This changes branching topology — only use when the user explicitly asks to insert a parent above an enlistment.";
        }
    }
}
