using GitEnlistmentManager.Commands;

namespace GitEnlistmentManager.CommandSets
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
                    Program = "{EnlistmentPullRequestUrl}",
                    FireAndForget = true
                }
            );

            Documentation = "Opens the configured Git hosting platform's pull-request creation page in the user's default browser for the enlistment's branch (against its parent enlistment's branch, or the bucket's target branch if no parent). Path must resolve to an enlistment. Side effect: launches the browser. Does NOT push commits — assumes the branch is already pushed remotely. Returns immediately; the actual PR is created by the user in the browser.";
        }
    }
}
