using GitEnlistmentManager.DTOs;
using GitEnlistmentManager.Globals;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace GitEnlistmentManager.Commands
{
    /// <summary>
    /// Launches the user's configured diff tool comparing two arbitrary directories supplied
    /// as positional arguments. Unlike <see cref="CompareSelectLeftSideCommand"/> +
    /// <see cref="CompareToLeftSideCommand"/> (a stateful 2-step pair driven by the right-click
    /// UI), this command takes both sides in a single call. Intended for MCP/AI use where a
    /// single invocation with two paths is more natural than the stateful UI flow.
    /// </summary>
    public class SpawnCompareCommand : Command
    {
        private string? leftPath;
        private string? rightPath;

        public SpawnCompareCommand()
        {
            this.Documentation = "Launches the configured diff tool comparing two directories supplied as args.";
        }

        public override void ParseArgs(Stack<string> arguments)
        {
            if (arguments.Count > 0)
            {
                leftPath = arguments.Pop();
            }
            if (arguments.Count > 0)
            {
                rightPath = arguments.Pop();
            }
        }

        public override async Task<bool> Execute()
        {
            if (string.IsNullOrWhiteSpace(leftPath) || string.IsNullOrWhiteSpace(rightPath))
            {
                UiMessages.ShowError("spawncompare requires two arguments: <leftPath> <rightPath>.");
                return false;
            }

            if (!Directory.Exists(leftPath) && !File.Exists(leftPath))
            {
                UiMessages.ShowError($"Left path does not exist: {leftPath}");
                return false;
            }

            if (!Directory.Exists(rightPath) && !File.Exists(rightPath))
            {
                UiMessages.ShowError($"Right path does not exist: {rightPath}");
                return false;
            }

            var tokens = new Dictionary<string, string>
            {
                ["LEFT"] = leftPath,
                ["RIGHT"] = rightPath
            };

            return await ProgramHelper.RunProgram(
                programPath: Gem.Instance.LocalAppData.CompareProgram,
                arguments: Gem.Instance.LocalAppData.CompareArguments,
                tokens: tokens,
                useShellExecute: false,
                openNewWindow: true,
                workingDirectory: null,
                fireAndForget: true
                ).ConfigureAwait(false);
        }
    }
}
