using GitEnlistmentManager.DTOs;
using GitEnlistmentManager.Extensions;
using GitEnlistmentManager.Globals;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GitEnlistmentManager.Commands
{
    public class CompareSelectLeftSideCommand : Command
    {
        public CompareSelectLeftSideCommand() 
        {
            this.CommandDocumentation = "Selects the left side of the comparison";
        }

        public override async Task<bool> Execute()
        {
            if (this.NodeContext.Enlistment == null)
            {
                return false;
            }

            var leftDirectoryCompare = this.NodeContext.Enlistment.GetDirectoryInfo()?.FullName;
            if (leftDirectoryCompare != null)
            {
                CommandSetMemory.Memory["LeftDirectoryCompare"] = leftDirectoryCompare;
            }
            return await Task.FromResult(true).ConfigureAwait(false);
        }
    }
}
