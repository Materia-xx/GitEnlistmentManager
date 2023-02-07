using GitEnlistmentManager.Extensions;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GitEnlistmentManager.DTOs.Commands
{
    public class GitSetPullDetails : ICommand
    {
        public bool OpenNewWindow { get; set; } = false;

        public void ParseArgs(GemNodeContext nodeContext, Stack<string> arguments)
        {
        }

        public async Task<bool> Execute(GemNodeContext nodeContext, MainWindow mainWindow)
        {
            if (nodeContext.Enlistment == null || nodeContext.Repo == null)
            {
                return false;
            }

            var enlistmentDirectory = nodeContext.Enlistment?.GetDirectoryInfo();
            if (enlistmentDirectory == null)
            {
                return false;
            }
            var parentEnlistment = nodeContext.Enlistment?.GetParentEnlistment();

            // Set these based on if the enlistment is a child of another directory or the main repo
            string? originUrl = parentEnlistment?.GetDirectoryInfo()?.FullName ?? nodeContext.Repo.Metadata.CloneUrl;
            string? pullFromBranch = parentEnlistment != null ? await parentEnlistment.GetFullGitBranch().ConfigureAwait(false) : null ?? nodeContext.Repo.Metadata.BranchFrom;

            if (string.IsNullOrWhiteSpace(originUrl) || string.IsNullOrWhiteSpace(pullFromBranch))
            {
                return false;
            }

            // This will set the "URL" that the enlistment pulls from.
            if (!await mainWindow.RunProgram(
                programPath: nodeContext.Enlistment?.Bucket.Repo.RepoCollection.Gem.LocalAppData.GitExePath,
                arguments: $@"remote set-url origin {originUrl}",
                tokens: null, // There are no tokens in the above programPath/arguments
                workingFolder: enlistmentDirectory.FullName
                ).ConfigureAwait(false))
            {
                return false;
            }

            // This sets the *branch* that the reference enlistment will pull from
            if (nodeContext.Enlistment != null)
            {
                var enlistmentBranch = await nodeContext.Enlistment.GetFullGitBranch().ConfigureAwait(false);

                // Make sure the branch we will pull from is already fetched locally before setting the upstream branch
                // If that branch is not already local git will reply with:
                // fatal: the requested upstream branch 'origin/master' does not exist
                // Git pull to verify we just set things up correctly
                if (!await mainWindow.RunProgram(
                    programPath: nodeContext.Enlistment?.Bucket.Repo.RepoCollection.Gem.LocalAppData.GitExePath,
                    arguments: $@"fetch origin {pullFromBranch}",
                    tokens: null, // There are no tokens in the above programPath/arguments
                    workingFolder: enlistmentDirectory.FullName
                    ).ConfigureAwait(false))
                {
                    return false;
                }

                if (!await mainWindow.RunProgram(
                    programPath: nodeContext.Enlistment?.Bucket.Repo.RepoCollection.Gem.LocalAppData.GitExePath,
                    arguments: $@"branch --set-upstream-to=origin/{pullFromBranch} {enlistmentBranch}",
                    tokens: null, // There are no tokens in the above programPath/arguments
                    workingFolder: enlistmentDirectory.FullName
                    ).ConfigureAwait(false))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
