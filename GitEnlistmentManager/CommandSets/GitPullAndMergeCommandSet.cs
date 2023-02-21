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

            Documentation = "Executes 'git pull' then 'git mergetool' in an enlistment"; ;
        }
    }
}
