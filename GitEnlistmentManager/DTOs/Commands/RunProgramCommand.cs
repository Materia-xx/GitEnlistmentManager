using GitEnlistmentManager.Globals;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GitEnlistmentManager.DTOs.Commands
{
    public class RunProgramCommand : ICommand
    {
        public bool OpenNewWindow { get; set; } = false;

        public string CommandDocumentation { get; set; } = "Runs a program.";

        public bool UseShellExecute { get; set; } = false;

        public string? Program { get; set; }

        public string? Arguments { get; set; }

        public string? WorkingDirectory { get; set; }

        public void ParseArgs(GemNodeContext nodeContext, Stack<string> arguments)
        {
        }

        public async Task<bool> Execute(GemNodeContext nodeContext, MainWindow mainWindow)
        {
            if (!this.OpenNewWindow)
            {
                return await mainWindow.RunProgram(
                    programPath: this.Program,
                    arguments: this.Arguments,
                    tokens: await nodeContext.GetTokens().ConfigureAwait(false),
                    workingDirectory: WorkingDirectory ?? nodeContext.GetWorkingDirectory()
                    ).ConfigureAwait(false);
            }
            else
            {
                return await ProgramHelper.RunProgram(
                    programPath: this.Program,
                    arguments: this.Arguments,
                    tokens: await nodeContext.GetTokens().ConfigureAwait(false),
                    useShellExecute: this.UseShellExecute,
                    openNewWindow: true,
                    workingDirectory: WorkingDirectory ?? nodeContext.GetWorkingDirectory()
                    ).ConfigureAwait(false);
            }
        }
    }
}
