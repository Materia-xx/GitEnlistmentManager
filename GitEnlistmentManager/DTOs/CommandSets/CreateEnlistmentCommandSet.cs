using GitEnlistmentManager.DTOs.Commands;

namespace GitEnlistmentManager.DTOs.CommandSets
{
    public class CreateEnlistmentCommandSet : CommandSet
    {
        public CreateEnlistmentCommandSet()
        {
            Placement = CommandSetPlacement.Bucket;
            OverrideKey = "ce";
            RightClickText = "Create New Enlistment";
            Verb = "ce";
            Filename = "gemce.cmdjson";

            Commands.Add(
                new CreateEnlistmentCommand()
            );
        }
    }
}
