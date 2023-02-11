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

            CommandSetDocumentation = "Creates a repository attached to the selected repository collection.";
        }
    }
}
