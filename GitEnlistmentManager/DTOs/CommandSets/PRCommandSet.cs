using GitEnlistmentManager.DTOs.Commands;

namespace GitEnlistmentManager.DTOs.CommandSets
{
    public class PRCommandSet : CommandSet
    {
        public PRCommandSet()
        {
            Placement = CommandSetPlacement.Enlistment;
            OverrideKey = "pr";
            RightClickText = "Pull Request";
            Verb = "pr";
            Filename = "gempr.cmdjson";

            Commands.Add(
                new RunProgramCommand()
                {
                    OpenNewWindow = true,
                    UseShellExecute = true,
                    Program = "{EnlistmentPullRequestUrl}"
                }
            );
        }
    }
}
