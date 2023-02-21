using GitEnlistmentManager.DTOs;
using GitEnlistmentManager.Extensions;
using GitEnlistmentManager.Globals;
using System.Threading.Tasks;

namespace GitEnlistmentManager.Commands
{
    public class GitPullCommand : Command
    {
        public GitPullCommand()
        {
            this.Documentation = "Executes 'git pull' in an enlistment";
        }

        public override async Task<bool> Execute()
        {
            if (this.NodeContext.Enlistment == null)
            {
                return false;
            }

            var enlistmentDirectory = this.NodeContext.Enlistment?.GetDirectoryInfo();
            if (enlistmentDirectory == null)
            {
                return false;
            }

            if (!await Global.Instance.MainWindow.RunProgram(
                programPath: Gem.Instance.LocalAppData.GitExePath,
                arguments: $@"pull",
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
