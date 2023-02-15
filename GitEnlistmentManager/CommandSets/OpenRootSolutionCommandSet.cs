using GitEnlistmentManager.Commands;

namespace GitEnlistmentManager.CommandSets
{
    public class OpenRootSolutionCommandSet : CommandSet
    {
        public OpenRootSolutionCommandSet()
        {
            Placement = CommandSetPlacement.Enlistment;
            OverrideKey = "openrootsolution";
            RightClickText = "Open Root Solution";
            Verb = "ors";
            Filename = "gemopenrootsolution.cmdjson";

            Commands.Add(new OpenRootSolutionCommand());

            Documentation = "Opens the root solution of the selected enlistment.";
        }
    }
}
