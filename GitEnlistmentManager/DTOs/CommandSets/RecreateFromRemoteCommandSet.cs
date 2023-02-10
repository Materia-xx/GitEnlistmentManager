using GitEnlistmentManager.DTOs.Commands;

namespace GitEnlistmentManager.DTOs.CommandSets
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

            this.Commands.Add(new RecreateFromRemoteCommand());
            this.Commands.Add(new RefreshTreeviewCommand());
        }
    }
}
