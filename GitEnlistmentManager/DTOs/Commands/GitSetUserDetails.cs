using GitEnlistmentManager.Extensions;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GitEnlistmentManager.DTOs.Commands
{
    public class GitSetUserDetails : ICommand
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

            // Set the user name
            if (!await mainWindow.RunProgram(
                programPath: nodeContext.Repo.RepoCollection.Gem.LocalAppData.GitExePath,
                arguments: $@"config --local user.name ""{nodeContext.Repo.Metadata.UserName}""",
                tokens: null, // There are no tokens in the above programPath/arguments
                workingFolder: enlistmentDirectory.FullName
                ).ConfigureAwait(false))
            {
                return false;
            }

            // Set the user email
            if (!await mainWindow.RunProgram(
                programPath: nodeContext.Repo.RepoCollection.Gem.LocalAppData.GitExePath,
                arguments: $@"config --local user.email ""{nodeContext.Repo.Metadata.UserEmail}""",
                tokens: null, // There are no tokens in the above programPath/arguments
                workingFolder: enlistmentDirectory.FullName
                ).ConfigureAwait(false))
            {
                return false;
            }

            return true;
        }
    }
}
