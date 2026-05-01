using GitEnlistmentManager.Commands;

namespace GitEnlistmentManager.CommandSets
{
    public class ManageRemoteBranchesCommandSet : CommandSet
    {
        public ManageRemoteBranchesCommandSet()
        {
            Placement = CommandSetPlacement.TargetBranch;
            OverrideKey = "remotebranches";
            RightClickText = "Manage Remote Branches";
            Verb = "remotebranches";
            Filename = "gemremotebranches.cmdjson";

            Commands.Add(new ManageRemoteBranchesCommand());
            Commands.Add(new RefreshTreeviewCommand());

            Documentation = "Opens the modal Manage Remote Branches dialog for the target branch at the path. Path must resolve to a target branch. INTERACTIVE — intended for UI use; not useful from MCP.";
            ExposeToMcp = false;
        }
    }
}
