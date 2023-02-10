using GitEnlistmentManager.DTOs.Commands;

namespace GitEnlistmentManager.DTOs.CommandSets
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

            this.Commands.Add(new CreateRepoCommand());
            this.Commands.Add(new RefreshTreeviewCommand());
        }
    }
}
