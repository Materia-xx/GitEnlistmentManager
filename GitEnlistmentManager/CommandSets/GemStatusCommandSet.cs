﻿using GitEnlistmentManager.Commands;
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
            Documentation = "Checks the status of the enlistment compared to the main branch.";
        }
    }
}
