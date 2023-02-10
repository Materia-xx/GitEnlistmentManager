using GitEnlistmentManager.DTOs.Commands;

namespace GitEnlistmentManager.DTOs.CommandSets
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

            this.Commands.Add(new EditRepoSettingsCommand());
            this.Commands.Add(new RefreshTreeviewCommand());
        }
    }
}
