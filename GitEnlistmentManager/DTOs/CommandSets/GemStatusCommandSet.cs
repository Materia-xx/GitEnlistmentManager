using GitEnlistmentManager.DTOs.Commands;
using GitEnlistmentManager.DTOs.CommandSetFilters;
using System.IO;

namespace GitEnlistmentManager.DTOs.CommandSets
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
        }


    }
}
