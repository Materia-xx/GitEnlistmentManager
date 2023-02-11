using GitEnlistmentManager.Commands;

namespace GitEnlistmentManager.CommandSets
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

            CommandSetDocumentation = "Shows help.";
        }
    }
}
