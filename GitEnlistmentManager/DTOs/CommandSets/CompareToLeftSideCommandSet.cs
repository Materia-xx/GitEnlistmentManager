using GitEnlistmentManager.DTOs.Commands;
using GitEnlistmentManager.DTOs.CommandSetFilters;

namespace GitEnlistmentManager.DTOs.CommandSets
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

            this.Commands.Add(
                new CompareToLeftSide()
            );

            this.Filters.Add(
                new CommandSetFilterGemCompareOptionSet()
            );

            this.Filters.Add(
                new CommandSetFilterCsmMemoryContainsKey()
                {
                    Key = "LeftDirectoryCompare"
                }
            );

            this.CommandSetDocumentation = "Selects the other enlistment to be compared with the first previously selected enlistment";
        }
    }
}
