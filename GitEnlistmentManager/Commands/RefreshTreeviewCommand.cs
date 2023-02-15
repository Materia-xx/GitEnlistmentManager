using GitEnlistmentManager.DTOs;
using GitEnlistmentManager.Extensions;
using GitEnlistmentManager.Globals;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GitEnlistmentManager.Commands
{
    internal class RefreshTreeviewCommand : Command
    {
        public RefreshTreeviewCommand()
        {
            this.Documentation = "Refreshes the Gem Treeview.";
        }

        public override async Task<bool> Execute()
        {
            return await Global.Instance.MainWindow.ReloadTreeview().ConfigureAwait(false);
        }
    }
}
