using GitEnlistmentManager.Extensions;
using GitEnlistmentManager.Globals;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GitEnlistmentManager.DTOs.Commands
{
    public class CompareToLeftSide : ICommand
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

            if (!CommandSetMemory.Memory.ContainsKey("LeftFolderCompare"))
            {
                return false;
            }

            var tokens = new Dictionary<string, string>();
            tokens["LEFT"] = CommandSetMemory.Memory["LeftFolderCompare"];
            var rightFolderCompare = nodeContext.Enlistment.GetDirectoryInfo()?.FullName;
            if (rightFolderCompare != null)
            {
                tokens["RIGHT"] = rightFolderCompare;
                await ProgramHelper.RunProgram(
                    programPath: nodeContext.Enlistment.Bucket.Repo.RepoCollection.Gem.LocalAppData.CompareProgram,
                    arguments: nodeContext.Enlistment.Bucket.Repo.RepoCollection.Gem.LocalAppData.CompareArguments,
                    tokens: tokens,
                    useShellExecute: false,
                    openNewWindow: true,
                    workingFolder: null
                    ).ConfigureAwait(false);
            }

            CommandSetMemory.Memory.Remove("LeftFolderCompare");
            return await Task.FromResult(true).ConfigureAwait(false);
        }
    }
}
