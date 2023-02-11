using GitEnlistmentManager.DTOs.Commands;

namespace GitEnlistmentManager.DTOs.CommandSets
{
    internal class ShowHelpCommandSet : CommandSet
    {
        public ShowHelpCommandSet()
        {
            Placement = CommandSetPlacement.All;
            OverrideKey = "help";
            RightClickText = "Help";
            Verb = "help";
            Filename = "gemhelp.cmdjson";

            Commands.Add(new ShowHelpCommand());

            this.CommandSetDocumentation = "Shows help.";
        }
    }
}
