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
            Verb = "listtokens";
            Filename = $"gemlt{placement}.cmdjson";
            ExposeToMcp = false;

            Commands.Add(new ListTokensCommand());

            Documentation = "Lists all token names and current values available at the path's level (e.g. {EnlistmentDirectory}, {RepoBranchPrefix}). Useful for diagnosing token expansion in command-set configurations. Output goes to the GEM command panel and is NOT captured back through MCP — UI/CLI use only.";
        }
    }
}
