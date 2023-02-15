using GitEnlistmentManager.Commands;

namespace GitEnlistmentManager.CommandSets
{
    public class EditGemSettingsCommandSet : CommandSet
    {
        public EditGemSettingsCommandSet()
        {
            Placement = CommandSetPlacement.Gem;
            OverrideKey = "editgemsettings";
            RightClickText = "Edit Gem Settings";
            Verb = string.Empty;
            Filename = "gemeditgemsettings.cmdjson";

            Commands.Add(new EditGemSettingsCommand());
            Commands.Add(new RefreshTreeviewCommand());

            Documentation = "Opens the menu to edit Gem Settings.";
        }
    }
}
