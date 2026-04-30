using GitEnlistmentManager.Commands;

namespace GitEnlistmentManager.CommandSets
{
    public class ArchiveEnlistmentCommandSet : CommandSet
    {
        public ArchiveEnlistmentCommandSet(CommandSetMode mode)
        {
            Placement = mode == CommandSetMode.UserInterface ? CommandSetPlacement.Enlistment : CommandSetPlacement.Bucket;
            OverrideKey = mode == CommandSetMode.UserInterface ? "aeui" : "aecmd";
            RightClickText = mode == CommandSetMode.UserInterface ? "Archive Enlistment" : string.Empty;
            Verb = mode == CommandSetMode.UserInterface ? string.Empty : "archiveenlistment";
            Filename = mode == CommandSetMode.UserInterface ? "gemaeui.cmdjson" : "gemaecmd.cmdjson";

            Commands.Add(new ArchiveEnlistmentCommand());
            Commands.Add(new RefreshTreeviewCommand());

            Documentation = "Archives the enlistment specified by the first argument (e.g. `archiveenlistment 010000.w1`) into a free archive slot configured in GEM settings. Path must resolve to a bucket. DESTRUCTIVE: moves the enlistment directory out of the live tree. Do NOT run automatically — only run when the user has explicitly confirmed the PR is completed or asks to archive a specific enlistment by an unambiguous name. Errors if no free archive slot is available.";
        }
    }
}
