using GitEnlistmentManager.DTOs.Commands;

namespace GitEnlistmentManager.DTOs.CommandSets
{
    public class ListTokensCommandSet : CommandSet
    {
        public ListTokensCommandSet(CommandSetPlacement placement)
        {
            Placement = placement;
            OverrideKey = "lt";
            RightClickText = "List Tokens";
            Verb = "lt";
            Filename = $"lt{placement}.cmdjson";

            Commands.Add(
                new ListTokensCommand()
            );
        }
    }
}
