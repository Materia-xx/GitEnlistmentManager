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

            CommandSetDocumentation = "Selects the other enlistment to be compared with the first previously selected enlistment";
        }
    }
}
