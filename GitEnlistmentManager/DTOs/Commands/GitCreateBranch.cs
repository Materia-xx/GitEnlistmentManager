using GitEnlistmentManager.Extensions;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GitEnlistmentManager.DTOs.Commands
{
    public class GitCreateBranch : ICommand
    {
        public bool OpenNewWindow { get; set; } = false;

        public string? Branch { get; set; }

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

            // TODO: delete this if all cases are working
            //// Make sure this branch has knowledge of the branch we are branching from, otherwise it will error out
            //// If we are branching from a local repo/folder then use the branch there as the branch to pull from
            //// Otherwise use the branch from the remote repo
            //if (!await mainWindow.RunProgram(
            //    programPath: enlistment.Bucket.Repo.RepoCollection.Gem.LocalAppData.GitExePath,
            //    arguments: $"checkout {parentEnlistment?.GetFullGitBranch() ?? enlistment.Bucket.Repo.Metadata.BranchFrom}",
            //    tokens: null, // There are no tokens in the above programPath/arguments
            //    openNewWindow: false,
            //    workingFolder: enlistmentDirectory.FullName
            //    ).ConfigureAwait(false))
            //{
            //    return false;
            //}

            // Create the new branch that this folder will represent
            if (!await mainWindow.RunProgram(
                programPath: nodeContext.Repo.RepoCollection.Gem.LocalAppData.GitExePath,
                arguments: $@"checkout -b ""{this.Branch ?? (nodeContext.Enlistment == null ? null : await nodeContext.Enlistment.GetFullGitBranch().ConfigureAwait(false))}""",
                tokens: null, // There are no tokens in the above programPath/arguments
                openNewWindow: false,
                workingFolder: enlistmentDirectory.FullName
                ).ConfigureAwait(false))
            {
                return false;
            }
            return true;
        }
    }
}
