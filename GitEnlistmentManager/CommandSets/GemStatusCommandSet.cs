using GitEnlistmentManager.Commands;
using GitEnlistmentManager.CommandSetFilters;

namespace GitEnlistmentManager.CommandSets
{
    public class GemStatusCommandSet : CommandSet
    {
        public GemStatusCommandSet()
        {
            Placement = CommandSetPlacement.Enlistment;
            OverrideKey = "Status";
            RightClickText = "Status";
            Verb = "Status";
            Filename = "gemstatus.cmdjson";

            Commands.Add(
                new RunProgramCommand()
                {
                    Program = "{GitExePath}",
                    Arguments = "status"
                }
            );
            Filters.Add(
                new CommandSetFilterCloneUrlContains()
                {
                    SearchFor = "GitEnlistmentManager"
                }
            );
            Documentation = "Runs `git status` in the enlistment. Restricted to GEM's own development repo (only available when the repo's clone URL contains 'GitEnlistmentManager'); not a general-purpose git-status command. Path must resolve to an enlistment. Output goes to the GEM command panel and is NOT captured back through MCP.";
            ExposeToMcp = false;
        }
    }
}
