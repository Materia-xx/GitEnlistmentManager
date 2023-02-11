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
            Filename = $"gemlt{placement}.cmdjson";

            this.Commands.Add(new ListTokensCommand());

            this.CommandSetDocumentation = "Lists the tokens of the selected.";
        }
    }
}
