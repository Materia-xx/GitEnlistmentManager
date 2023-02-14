using GitEnlistmentManager.DTOs;
using GitEnlistmentManager.Extensions;
using GitEnlistmentManager.Globals;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GitEnlistmentManager.Commands
{
    public class GitSetPushDetailsCommand : Command
    {
        public GitSetPushDetailsCommand() 
        {
            this.CommandDocumentation = "Sets the push details.";
        }

        public override async Task<bool> Execute()
        {
            if (this.NodeContext.Enlistment == null || this.NodeContext.Repo == null)
            {
                return false;
            }
            var enlistmentDirectory = this.NodeContext.Enlistment?.GetDirectoryInfo();
            if (enlistmentDirectory == null)
            {
                return false;
            }

            // This will make it so 'git push' always pushes to a branch in the main repo
            // i.e. child branch e3 will not push to child branch e2, but rather to the original repo
            if (!await Global.Instance.MainWindow.RunProgram(
                programPath: Gem.Instance.LocalAppData.GitExePath,
                arguments: $@"remote set-url --push origin {this.NodeContext.Repo.Metadata.CloneUrl}",
                tokens: null, // There are no tokens in the above programPath/arguments
                workingDirectory: enlistmentDirectory.FullName
                ).ConfigureAwait(false))
            {
                return false;
            }

            // This is the branch that 'git push' will publish to
            // It is set to publish a branch with the same name on the remote
            if (!await Global.Instance.MainWindow.RunProgram(
                programPath: Gem.Instance.LocalAppData.GitExePath,
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
