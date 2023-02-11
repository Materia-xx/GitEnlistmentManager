using GitEnlistmentManager.DTOs.Commands;

namespace GitEnlistmentManager.DTOs.CommandSets
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
            
            this.CommandSetDocumentation = "Opens the root solution of the selected enlistment.";
        }
    }
}
