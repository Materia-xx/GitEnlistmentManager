using GitEnlistmentManager.Extensions;
using GitEnlistmentManager.Globals;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GitEnlistmentManager.DTOs.Commands
{
    public class CompareSelectLeftSideCommand : ICommand
    {
        public bool OpenNewWindow { get; set; } = false;

        public string CommandDocumentation { get; set; } = "Selects the left side of the comparison";

        public void ParseArgs(GemNodeContext nodeContext, Stack<string> arguments)
        {
        }

        public async Task<bool> Execute(GemNodeContext nodeContext, MainWindow mainWindow)
        {
            if (nodeContext.Enlistment == null)
            {
                return false;
            }

            var leftDirectoryCompare = nodeContext.Enlistment.GetDirectoryInfo()?.FullName;
            if (leftDirectoryCompare != null)
            {
                CommandSetMemory.Memory["LeftDirectoryCompare"] = leftDirectoryCompare;
            }
            return await Task.FromResult(true).ConfigureAwait(false);
        }
    }
}
