using GitEnlistmentManager.DTOs;
using GitEnlistmentManager.Globals;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GitEnlistmentManager.Commands
{
    internal class ManageRemoteBranchesCommand : Command
    {
        public ManageRemoteBranchesCommand()
        {
            this.Documentation = "Shows the manage remote branches window.";
        }

        public override async Task<bool> Execute()
        {
            if (this.NodeContext.Repo != null)
            {
                await Global.Instance.MainWindow.Dispatcher.InvokeAsync(() =>
                {
                    var remoteBranches = new RemoteBranches(this.NodeContext.Repo);
                    remoteBranches.ShowDialog();
                });
            }

            return true;
        }
    }
}
