using GitEnlistmentManager.DTOs;
using GitEnlistmentManager.Extensions;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GitEnlistmentManager.Commands
{
    internal class RefreshTreeviewCommand : ICommand
    {
        public bool OpenNewWindow { get; set; } = false;

        public string CommandDocumentation { get; set; } = "Refreshes the Gem Treeview.";

        public void ParseArgs(GemNodeContext nodeContext, Stack<string> arguments)
        {
        }

        public async Task<bool> Execute(GemNodeContext nodeContext, MainWindow mainWindow)
        {
            return await mainWindow.ReloadTreeview().ConfigureAwait(false);
        }
    }
}
