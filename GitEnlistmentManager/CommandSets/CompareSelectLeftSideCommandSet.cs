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

            Documentation = "Step 1 of a 2-step diff: stores the enlistment path as the 'left' side for a later compare. Path must resolve to an enlistment. Stateful — must be followed by `comparetoleft` on a different enlistment. Has no observable effect on its own; primarily intended for interactive UI use.";
        }
    }
}
