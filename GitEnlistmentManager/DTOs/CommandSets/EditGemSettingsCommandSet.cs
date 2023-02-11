using GitEnlistmentManager.DTOs.Commands;

namespace GitEnlistmentManager.DTOs.CommandSets
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

            this.Commands.Add(new EditGemSettingsCommand());
            this.Commands.Add(new RefreshTreeviewCommand());

            this.CommandSetDocumentation = "Opens the menu to edit Gem Settings.";
        }
    }
}
