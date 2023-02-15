using GitEnlistmentManager.Commands;
using GitEnlistmentManager.CommandSetFilters;

namespace GitEnlistmentManager.CommandSets
{
    public class CompareSelectLeftSideCommandSet : CommandSet
    {
        public CompareSelectLeftSideCommandSet()
        {
            Placement = CommandSetPlacement.Enlistment;
            OverrideKey = "compareselectleft";
            RightClickText = "Compare: Select Left";
            Verb = "compareselectleft";
            Filename = "gemcompareselectleft.cmdjson";

            Commands.Add(
                new CompareSelectLeftSideCommand()
            );

            Filters.Add(new CommandSetFilterGemCompareOptionSet());

            Documentation = "Selects the first enlistment to compare to another one that will be selected next.";
        }
    }
}
