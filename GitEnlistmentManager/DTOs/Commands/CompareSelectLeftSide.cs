using GitEnlistmentManager.Extensions;
using GitEnlistmentManager.Globals;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GitEnlistmentManager.DTOs.Commands
{
    public class CompareSelectLeftSide : ICommand
    {
        public bool OpenNewWindow { get; set; } = false;

        public void ParseArgs(GemNodeContext nodeContext, Stack<string> arguments)
        {
        }

        public async Task<bool> Execute(GemNodeContext nodeContext, MainWindow mainWindow)
        {
            if (nodeContext.Enlistment == null)
            {
                return false;
            }

            var leftFolderCompare = nodeContext.Enlistment.GetDirectoryInfo()?.FullName;
            if (leftFolderCompare != null)
            {
                CommandSetMemory.Memory["LeftFolderCompare"] = leftFolderCompare;
            }
            return await Task.FromResult(true).ConfigureAwait(false);
        }
    }
}
