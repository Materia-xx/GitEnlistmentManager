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
                    SearchFor = "GitEnlistmentManager" // TODO: will need some good documentation around how to set these command sets and filters up.
                }
            );
            CommandSetDocumentation = "Checks the status of the enlistment compared to the main branch.";
        }


    }
}
