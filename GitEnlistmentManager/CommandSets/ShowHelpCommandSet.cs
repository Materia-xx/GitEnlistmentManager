using GitEnlistmentManager.Commands;

namespace GitEnlistmentManager.CommandSets
{
    internal class ShowHelpCommandSet : CommandSet
    {
        public ShowHelpCommandSet()
        {
            Placement = CommandSetPlacement.Gem;
            OverrideKey = "help";
            RightClickText = "Help";
            Verb = "help";
            Filename = "gemhelp.cmdjson";

            Commands.Add(new ShowHelpCommand());

            Documentation = "Shows help.";
        }
    }
}
