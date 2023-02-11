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

            CommandSetDocumentation = "Opens the menu to edit Repo Settings.";
        }
    }
}
