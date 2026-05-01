using GitEnlistmentManager.Commands;

namespace GitEnlistmentManager.CommandSets
{
    public class GitPullAndMergeCommandSet : CommandSet
    {
        public GitPullAndMergeCommandSet()
        {
            Placement = CommandSetPlacement.Enlistment;
            OverrideKey = "gitpullandmerge";
            RightClickText = "Pull and Merge";
            Verb = "pullandmerge";
            Filename = "gemgitpullandmerge.cmdjson";

            Commands.Add(new GitPullCommand());
            Commands.Add(new GitMergeToolCommand());

            Documentation = "Runs `git pull` followed by `git mergetool` in the enlistment. Path must resolve to an enlistment. The `git mergetool` step opens the user's configured GUI merge tool when there are conflicts and BLOCKS waiting for the user to resolve them — interactive. Use sparingly from MCP; prefer asking the user to resolve manually.";
        }
    }
}
