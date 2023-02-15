using GitEnlistmentManager.DTOs;
using GitEnlistmentManager.Extensions;
using GitEnlistmentManager.Globals;
using System.Threading.Tasks;

namespace GitEnlistmentManager.Commands
{
    public class GitSetPullDetailsCommand : Command
    {
        public GitSetPullDetailsCommand()
        {
            this.Documentation = "Sets the pull details.";
        }

        public string? FetchFilterBranch { get; set; }

        public bool ScopeToBranch { get; set; }

        public override async Task<bool> Execute()
        {
            if (this.NodeContext.Repo == null || this.NodeContext.Enlistment == null)
            {
                return false;
            }

            var enlistmentDirectory = this.NodeContext.Enlistment.GetDirectoryInfo();
            if (enlistmentDirectory == null)
            {
                return false;
            }
            var parentEnlistment = this.NodeContext.Enlistment.GetParentEnlistment();

            // Set these based on if the enlistment is a child of another directory or the main repo
            string? originUrl = parentEnlistment?.GetDirectoryInfo()?.FullName ?? this.NodeContext.Repo.Metadata.CloneUrl;
            string? pullFromBranch = parentEnlistment != null ? await parentEnlistment.GetFullGitBranch().ConfigureAwait(false) : null ?? this.NodeContext.Repo.Metadata.BranchFrom;

            if (string.IsNullOrWhiteSpace(originUrl) || string.IsNullOrWhiteSpace(pullFromBranch))
            {
                return false;
            }

            // This will set the "URL" that the enlistment pulls from.
            if (!await Global.Instance.MainWindow.RunProgram(
                programPath: Gem.Instance.LocalAppData.GitExePath,
                arguments: $@"remote set-url origin {originUrl}",
                tokens: null, // There are no tokens in the above programPath/arguments
                workingDirectory: enlistmentDirectory.FullName
                ).ConfigureAwait(false))
            {
                return false;
            }

            // This sets the *branch* that the reference enlistment will pull from
            if (this.NodeContext.Enlistment != null)
            {
                var enlistmentBranch = await this.NodeContext.Enlistment.GetFullGitBranch().ConfigureAwait(false);

                // using --branch x (during clone) will get you a local enlistment that is checked out to branch x, but it also has the downside of setting the config 
                // up in a way where it will only be able to fetch from the x branch too. This ends up so that later on when we tell the branch to pull
                // from the master branch instead of the x branch, it is unable to do so because it can only see x in the remote due to the fetch settings
                var fetchFilter = !this.ScopeToBranch ? "*" : this.FetchFilterBranch ?? "*";
                var fetchSettings = $"+refs/heads/{fetchFilter}:refs/remotes/origin/{fetchFilter}";
                if (!await Global.Instance.MainWindow.RunProgram(
                    programPath: Gem.Instance.LocalAppData.GitExePath,
                    arguments: $"config remote.origin.fetch {fetchSettings}",
                    tokens: null, // There are no tokens in the above programPath/arguments - If we did supply tokens here it would supply an invalid enlistmentBranch because it's not made yet.
                    workingDirectory: enlistmentDirectory.FullName
                    ).ConfigureAwait(false))
                {
                    return false;
                }

                // Make sure the branch we will pull from is already fetched locally before setting the upstream branch
                // If that branch is not already local git will reply with:
                // fatal: the requested upstream branch 'origin/master' does not exist
                // Git pull to verify we just set things up correctly
                if (!await Global.Instance.MainWindow.RunProgram(
                    programPath: Gem.Instance.LocalAppData.GitExePath,
                    arguments: $@"fetch origin {pullFromBranch}",
                    tokens: null, // There are no tokens in the above programPath/arguments
                    workingDirectory: enlistmentDirectory.FullName
                    ).ConfigureAwait(false))
                {
                    return false;
                }

                if (!await Global.Instance.MainWindow.RunProgram(
                    programPath: Gem.Instance.LocalAppData.GitExePath,
                    arguments: $@"branch --set-upstream-to=origin/{pullFromBranch} {enlistmentBranch}",
                    tokens: null, // There are no tokens in the above programPath/arguments
                    workingDirectory: enlistmentDirectory.FullName
                    ).ConfigureAwait(false))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
