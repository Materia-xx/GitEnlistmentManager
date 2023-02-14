using GitEnlistmentManager.DTOs;
using GitEnlistmentManager.Extensions;
using GitEnlistmentManager.Globals;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GitEnlistmentManager.Commands
{
    public class GitCreateBranchCommand : Command
    {
        public GitCreateBranchCommand()
        {
            this.CommandDocumentation = "Creates a branch";
        }

        public string? Branch { get; set; }

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

            // Create the new branch that this directory will represent
            if (!await Global.Instance.MainWindow.RunProgram(
                programPath: Gem.Instance.LocalAppData.GitExePath,
                arguments: $@"checkout -b ""{Branch ?? (this.NodeContext.Enlistment == null ? null : await this.NodeContext.Enlistment.GetFullGitBranch().ConfigureAwait(false))}""",
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
