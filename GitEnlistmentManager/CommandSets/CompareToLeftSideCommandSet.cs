using GitEnlistmentManager.Commands;
using GitEnlistmentManager.CommandSetFilters;

namespace GitEnlistmentManager.CommandSets
{
    public class CompareToLeftSideCommandSet : CommandSet
    {
        public CompareToLeftSideCommandSet()
        {
            Placement = CommandSetPlacement.Enlistment;
            OverrideKey = "comparetoleft";
            RightClickText = "Compare: To Left";
            Verb = "comparetoleft";
            Filename = "gemcomparetoleft.cmdjson";

            Commands.Add(
                new CompareToLeftSideCommand()
            );

            Filters.Add(
                new CommandSetFilterGemCompareOptionSet()
            );

            Filters.Add(
                new CommandSetFilterCsmMemoryContainsKey()
                {
                    Key = "LeftDirectoryCompare"
                }
            );

            Documentation = "Step 2 of a 2-step diff: opens the user's configured diff tool comparing the previously selected 'left' enlistment (set via `compareselectleft`) against this enlistment. Path must resolve to an enlistment. Errors if no left side has been selected. Side effect: launches an external diff GUI. Returns immediately after launching; MCP success only indicates the diff tool started.";
        }
    }
}
