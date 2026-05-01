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

            Documentation = "Lists all available verbs along with their right-click text and documentation. Path level does not matter. Output goes to the GEM command panel and is NOT captured back through MCP — prefer the `list_commands` MCP tool when calling from an AI.";
            ExposeToMcp = false;
        }
    }
}
