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
            this.Documentation = "Selects the other side to compare to the left.";
        }

        public override async Task<bool> Execute()
        {
            if (this.NodeContext.Enlistment == null)
            {
                UiMessages.ShowError("Compare-to-left requires an enlistment in context.");
                return false;
            }

            if (!CommandSetMemory.Memory.ContainsKey("LeftDirectoryCompare"))
            {
                UiMessages.ShowError("No left side has been selected for compare. Run 'compareselectleft' on an enlistment first.");
                return false;
            }

            var tokens = new Dictionary<string, string>();
            tokens["LEFT"] = CommandSetMemory.Memory["LeftDirectoryCompare"];
            var rightDirectoryCompare = this.NodeContext.Enlistment.GetDirectoryInfo()?.FullName;
            if (rightDirectoryCompare == null)
            {
                UiMessages.ShowError("Could not resolve the right-side enlistment directory.");
                return false;
            }

            tokens["RIGHT"] = rightDirectoryCompare;
            var launched = await ProgramHelper.RunProgram(
                programPath: Gem.Instance.LocalAppData.CompareProgram,
                arguments: Gem.Instance.LocalAppData.CompareArguments,
                tokens: tokens,
                useShellExecute: false,
                openNewWindow: true,
                workingDirectory: null,
                fireAndForget: true
                ).ConfigureAwait(false);

            if (!launched)
            {
                return false;
            }

            CommandSetMemory.Memory.Remove("LeftDirectoryCompare");
            return true;
        }
    }
}
