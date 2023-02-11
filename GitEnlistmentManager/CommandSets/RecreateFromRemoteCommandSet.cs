using GitEnlistmentManager.Commands;

namespace GitEnlistmentManager.CommandSets
{
    public class RecreateFromRemoteCommandSet : CommandSet
    {
        public RecreateFromRemoteCommandSet()
        {
            Placement = CommandSetPlacement.Repo;
            OverrideKey = "recreate";
            RightClickText = "Re-create all from remote";
            Verb = "recreate";
            Filename = "gemrecreate.cmdjson";

            Commands.Add(new RecreateFromRemoteCommand());
            Commands.Add(new RefreshTreeviewCommand());

            CommandSetDocumentation = "Recreates all buckets and enlistments from a git server.";
        }
    }
}
