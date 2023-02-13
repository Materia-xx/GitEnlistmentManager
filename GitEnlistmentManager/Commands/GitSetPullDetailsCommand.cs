using GitEnlistmentManager.DTOs;
using GitEnlistmentManager.Extensions;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GitEnlistmentManager.Commands
{
    public class GitSetPullDetailsCommand : ICommand
    {
        public bool OpenNewWindow { get; set; } = false;

        public string CommandDocumentation { get; set; } = "Sets the pull details.";

        public Enlistment? EnlistmentOverride { get; set; } // TODO: instead of iCommand make a base class that has these (Name it FocusEnlistment) and no longer pass nodeContext. Then there is also no need to call this one Override.

        /// <summary>
        /// This is the branch that it will pull from when you type 'git pull'
        /// </summary>
        public string? PullFromBranch { get; set; }

        public void ParseArgs(GemNodeContext nodeContext, Stack<string> arguments)
        {
        }

        public async Task<bool> Execute(GemNodeContext nodeContext, MainWindow mainWindow)
        {
            if (nodeContext.Repo == null)
            {
                return false;
            }

            var enlistment = this.EnlistmentOverride ?? nodeContext.Enlistment;
            if (enlistment == null)
            {
                return false;
            }

            var enlistmentDirectory = enlistment?.GetDirectoryInfo();
            if (enlistmentDirectory == null)
            {
                return false;
            }
            var parentEnlistment = enlistment?.GetParentEnlistment();

            // Set these based on if the enlistment is a child of another directory or the main repo
            string? originUrl = parentEnlistment?.GetDirectoryInfo()?.FullName ?? nodeContext.Repo.Metadata.CloneUrl;
            string? pullFromBranch = parentEnlistment != null ? await parentEnlistment.GetFullGitBranch().ConfigureAwait(false) : null ?? nodeContext.Repo.Metadata.BranchFrom;

            if (string.IsNullOrWhiteSpace(originUrl) || string.IsNullOrWhiteSpace(pullFromBranch))
            {
                return false;
            }

            // This will set the "URL" that the enlistment pulls from.
            if (!await mainWindow.RunProgram(
                programPath: enlistment?.Bucket.Repo.RepoCollection.Gem.LocalAppData.GitExePath,
                arguments: $@"remote set-url origin {originUrl}",
                tokens: null, // There are no tokens in the above programPath/arguments
                workingDirectory: enlistmentDirectory.FullName
                ).ConfigureAwait(false))
            {
                return false;
            }

            // This sets the *branch* that the reference enlistment will pull from
            if (enlistment != null)
            {
                var enlistmentBranch = await enlistment.GetFullGitBranch().ConfigureAwait(false);

                // using --branch x (during clone) will get you a local enlistment that is checked out to branch x, but it also has the downside of setting the config 
                // up in a way where it will only be able to fetch from the x branch too. This ends up so that later on when we tell the branch to pull
                // from the master branch instead of the x branch, it is unable to do so because it can only see x in the remote due to the fetch settings
                PullFromBranch ??= "*";
                var fetchSettings = $"+refs/heads/{PullFromBranch}:refs/remotes/origin/{PullFromBranch}";
                if (!await mainWindow.RunProgram(
                    programPath: Gem.Instance.LocalAppData.GitExePath, // TODO: look for things like nodeContext.RepoCollection.Gem.LocalAppData.GitExePath, make them all direct gem instance refs
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
                if (!await mainWindow.RunProgram(
                    programPath: enlistment?.Bucket.Repo.RepoCollection.Gem.LocalAppData.GitExePath,
                    arguments: $@"fetch origin {pullFromBranch}",
                    tokens: null, // There are no tokens in the above programPath/arguments
                    workingDirectory: enlistmentDirectory.FullName
                    ).ConfigureAwait(false))
                {
                    return false;
                }

                if (!await mainWindow.RunProgram(
                    programPath: enlistment?.Bucket.Repo.RepoCollection.Gem.LocalAppData.GitExePath,
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
