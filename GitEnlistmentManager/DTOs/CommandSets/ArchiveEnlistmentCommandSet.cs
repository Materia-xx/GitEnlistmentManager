using GitEnlistmentManager.DTOs.Commands;

namespace GitEnlistmentManager.DTOs.CommandSets
{
    public class ArchiveEnlistmentCommandSet : CommandSet
    {
        public ArchiveEnlistmentCommandSet(CommandSetMode mode)
        {
            Placement = mode == CommandSetMode.UserInterface ? CommandSetPlacement.Enlistment : CommandSetPlacement.Bucket;
            OverrideKey = mode == CommandSetMode.UserInterface ? "aeui" : "aecmd";
            RightClickText = mode == CommandSetMode.UserInterface ? "Archive Enlistment" : string.Empty;
            Verb = mode == CommandSetMode.UserInterface ? string.Empty : "ae";
            Filename = mode == CommandSetMode.UserInterface ? "gemaeui.cmdjson" : "gemaecmd.cmdjson";

            Commands.Add(
                new ArchiveEnlistmentCommand()
            );
        }
    }
}
