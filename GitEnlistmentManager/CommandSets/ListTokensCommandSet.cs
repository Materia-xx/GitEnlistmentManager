using GitEnlistmentManager.Commands;

namespace GitEnlistmentManager.CommandSets
{
    public class ListTokensCommandSet : CommandSet
    {
        public ListTokensCommandSet(CommandSetPlacement placement)
        {
            Placement = placement;
            OverrideKey = "lt";
            RightClickText = "List Tokens";
            Verb = "lt";
            Filename = $"gemlt{placement}.cmdjson";

            Commands.Add(new ListTokensCommand());

            Documentation = "Lists the tokens of the selected.";
        }
    }
}
