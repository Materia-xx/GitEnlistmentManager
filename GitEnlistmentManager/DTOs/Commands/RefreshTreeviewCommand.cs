using GitEnlistmentManager.Extensions;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GitEnlistmentManager.DTOs.Commands
{
    internal class RefreshTreeviewCommand : ICommand
    {
        public bool OpenNewWindow { get; set; } = false;

        public void ParseArgs(GemNodeContext nodeContext, Stack<string> arguments)
        {
        }

        public async Task<bool> Execute(GemNodeContext nodeContext, MainWindow mainWindow)
        {
            if (nodeContext.RepoCollection == null)
            {
                return false;
            }
            if (nodeContext.RepoCollection.Gem.ReloadSettings())
            {
                return true;
            }
            return await Task.FromResult(false).ConfigureAwait(false);
        }
    }
}
