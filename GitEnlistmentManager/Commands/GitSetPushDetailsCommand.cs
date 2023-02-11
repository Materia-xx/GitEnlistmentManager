using GitEnlistmentManager.DTOs;
using GitEnlistmentManager.Extensions;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GitEnlistmentManager.Commands
{
    public class GitSetPushDetailsCommand : ICommand
    {
        public bool OpenNewWindow { get; set; } = false;

        public string CommandDocumentation { get; set; } = "Sets the push details.";

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

            // This will make it so 'git push' always pushes to a branch in the main repo
            // i.e. child branch e3 will not push to child branch e2, but rather to the original repo
            if (!await mainWindow.RunProgram(
                programPath: nodeContext.Repo.RepoCollection.Gem.LocalAppData.GitExePath,
                arguments: $@"remote set-url --push origin {nodeContext.Repo.Metadata.CloneUrl}",
                tokens: null, // There are no tokens in the above programPath/arguments
                workingDirectory: enlistmentDirectory.FullName
                ).ConfigureAwait(false))
            {
                return false;
            }

            // This is the branch that 'git push' will publish to
            // It is set to publish a branch with the same name on the remote
            if (!await mainWindow.RunProgram(
                programPath: nodeContext.Repo.RepoCollection.Gem.LocalAppData.GitExePath,
                arguments: $@"config push.default current",
                tokens: null, // There are no tokens in the above programPath/arguments
                workingDirectory: enlistmentDirectory.FullName
                ).ConfigureAwait(false))
            {
                return false;
            }
            return true;
        }
    }
}
