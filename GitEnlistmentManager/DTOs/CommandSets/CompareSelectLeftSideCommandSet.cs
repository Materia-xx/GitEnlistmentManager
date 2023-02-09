using GitEnlistmentManager.DTOs.Commands;
using GitEnlistmentManager.DTOs.CommandSetFilters;

namespace GitEnlistmentManager.DTOs.CommandSets
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
                new CompareSelectLeftSide()
            );

            this.Filters.Add(
                new CommandSetFilterGemCompareOptionSet()
            );
        }
    }
}
