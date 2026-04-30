using GitEnlistmentManager.Commands;

namespace GitEnlistmentManager.CommandSets
{
    public class CreateRepoCommandSet : CommandSet
    {
        public CreateRepoCommandSet()
        {
            Placement = CommandSetPlacement.RepoCollection;
            OverrideKey = "createrepo";
            RightClickText = "Create Repo";
            Verb = "createrepo";
            Filename = "gemcreaterepo.cmdjson";

            Commands.Add(new CreateRepoCommand());
            Commands.Add(new RefreshTreeviewCommand());

            Documentation = "Registers a new repo definition in the selected repo collection and creates its on-disk metadata. Path must resolve to a repo collection. Does NOT clone anything — the repo's clone URL, branch prefix, target branches, etc. must still be configured before any enlistment can be created.";
        }
    }
}
