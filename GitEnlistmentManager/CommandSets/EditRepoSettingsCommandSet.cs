using GitEnlistmentManager.Commands;

namespace GitEnlistmentManager.CommandSets
{
    public class EditRepoSettingsCommandSet : CommandSet
    {
        public EditRepoSettingsCommandSet()
        {
            Placement = CommandSetPlacement.Repo;
            OverrideKey = "editreposettings";
            RightClickText = "Edit Repo Settings";
            Verb = "editreposettings";
            Filename = "gemeditreposettings.cmdjson";

            Commands.Add(new EditRepoSettingsCommand());
            Commands.Add(new RefreshTreeviewCommand());

            Documentation = "Opens the modal Repo Settings dialog for the repo at the path. Path must resolve to a repo. INTERACTIVE — intended for UI use; from MCP this opens a dialog the user must complete or cancel.";
            ExposeToMcp = false;
        }
    }
}
