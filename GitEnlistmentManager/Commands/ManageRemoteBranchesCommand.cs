using GitEnlistmentManager.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GitEnlistmentManager.Commands
{
    internal class ManageRemoteBranchesCommand : ICommand
    {
        public bool OpenNewWindow { get; set; } = false;

        public string CommandDocumentation { get; set; } = "Shows the manage remote branches window.";

        public void ParseArgs(GemNodeContext nodeContext, Stack<string> arguments)
        {
        }

        public async Task<bool> Execute(GemNodeContext nodeContext, MainWindow mainWindow)
        {
            if (nodeContext.Repo != null)
            {
                await mainWindow.Dispatcher.InvokeAsync(() =>
                {
                    var remoteBranches = new RemoteBranches(nodeContext.Repo, mainWindow);
                    remoteBranches.ShowDialog();
                });
            }

            return true;
        }
    }
}
