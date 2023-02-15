using GitEnlistmentManager.DTOs;
using GitEnlistmentManager.Globals;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GitEnlistmentManager.Commands
{
    public class RunProgramCommand : Command
    {
        public RunProgramCommand()
        {
            this.Documentation = @"Runs a program. The Program, Arguments and WorkingDirectory properties can contain tokens in the form of {token}.
The tokens that are available depend on where the command set is being run from.
To get a list of currently available tokens use 'gem lt' within a gem directory or 'List Tokens' right click menu.";
        }


        public bool UseShellExecute { get; set; } = false;

        public string? Program { get; set; }

        public string? Arguments { get; set; }

        public string? WorkingDirectory { get; set; }

        public override async Task<bool> Execute()
        {
            if (!OpenNewWindow)
            {
                return await Global.Instance.MainWindow.RunProgram(
                    programPath: Program,
                    arguments: Arguments,
                    tokens: await this.NodeContext.GetTokens().ConfigureAwait(false),
                    workingDirectory: WorkingDirectory ?? this.NodeContext.GetWorkingDirectory()
                    ).ConfigureAwait(false);
            }
            else
            {
                return await ProgramHelper.RunProgram(
                    programPath: Program,
                    arguments: Arguments,
                    tokens: await this.NodeContext.GetTokens().ConfigureAwait(false),
                    useShellExecute: UseShellExecute,
                    openNewWindow: true,
                    workingDirectory: WorkingDirectory ?? this.NodeContext.GetWorkingDirectory()
                    ).ConfigureAwait(false);
            }
        }
    }
}
