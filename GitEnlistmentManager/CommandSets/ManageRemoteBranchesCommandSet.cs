using GitEnlistmentManager.Commands;

namespace GitEnlistmentManager.CommandSets
{
    public class ManageRemoteBranchesCommandSet : CommandSet
    {
        public ManageRemoteBranchesCommandSet()
        {
            Placement = CommandSetPlacement.Repo;
            OverrideKey = "remotebranches";
            RightClickText = "Manage Remote Branches";
            Verb = "remotebranches";
            Filename = "gemremotebranches.cmdjson";

            Commands.Add(new ManageRemoteBranchesCommand());
            Commands.Add(new RefreshTreeviewCommand());

            CommandSetDocumentation = "Shows the manage remote branches window.";
        }
    }
}
