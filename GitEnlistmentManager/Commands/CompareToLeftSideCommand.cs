using GitEnlistmentManager.DTOs;
using GitEnlistmentManager.Extensions;
using GitEnlistmentManager.Globals;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GitEnlistmentManager.Commands
{
    public class CompareToLeftSideCommand : Command
    {
        public CompareToLeftSideCommand()
        {
            this.CommandDocumentation = "Selects the other side to compare to the left.";
        }

        public override async Task<bool> Execute()
        {
            if (this.NodeContext.Enlistment == null)
            {
                return false;
            }

            if (!CommandSetMemory.Memory.ContainsKey("LeftDirectoryCompare"))
            {
                return false;
            }

            var tokens = new Dictionary<string, string>();
            tokens["LEFT"] = CommandSetMemory.Memory["LeftDirectoryCompare"];
            var rightDirectoryCompare = this.NodeContext.Enlistment.GetDirectoryInfo()?.FullName;
            if (rightDirectoryCompare != null)
            {
                tokens["RIGHT"] = rightDirectoryCompare;
                await ProgramHelper.RunProgram(
                    programPath: Gem.Instance.LocalAppData.CompareProgram,
                    arguments: Gem.Instance.LocalAppData.CompareArguments,
                    tokens: tokens,
                    useShellExecute: false,
                    openNewWindow: true,
                    workingDirectory: null
                    ).ConfigureAwait(false);
            }

            CommandSetMemory.Memory.Remove("LeftDirectoryCompare");
            return await Task.FromResult(true).ConfigureAwait(false);
        }
    }
}
