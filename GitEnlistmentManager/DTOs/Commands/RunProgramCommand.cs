using System.Collections.Generic;
using System;
using System.Threading.Tasks;

namespace GitEnlistmentManager.DTOs.Commands
{
    public class RunProgramCommand : ICommand
    {
        public bool OpenNewWindow { get; set; } = false;

        public string? Program { get; set; }

        public string? Arguments { get; set; }

        public void ParseArgs(GemNodeContext nodeContext, Stack<string> arguments)
        {
        }

        public async Task<bool> Execute(GemNodeContext nodeContext, MainWindow mainWindow)
        {
            return await mainWindow.RunProgram(
                programPath: this.Program,
                arguments: this.Arguments,
                tokens: await nodeContext.GetTokens().ConfigureAwait(false),
                openNewWindow: this.OpenNewWindow,
                workingFolder: nodeContext.GetWorkingFolder()
                ).ConfigureAwait(false);
        }
    }
}
