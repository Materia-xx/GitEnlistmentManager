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
To get a list of currently available tokens use 'gem listtokens' within a gem directory or 'List Tokens' right click menu.";
        }


        public bool UseShellExecute { get; set; } = false;

        /// <summary>
        /// When true, the spawned process is launched and the command returns immediately
        /// without waiting for it to exit. Use this for long-lived UI launchers (devenv, dev
        /// command prompts, browser-launched URLs, diff GUIs) so that MCP callers don't
        /// block until the user closes the launched program. Only honored when OpenNewWindow
        /// is true (the in-pane execution path always streams output and waits).
        /// </summary>
        public bool FireAndForget { get; set; } = false;

        public string? Program { get; set; }

        public string? Arguments { get; set; }

        public string? WorkingDirectory { get; set; }

        public override async Task<bool> Execute()
        {
            if (FireAndForget && !OpenNewWindow)
            {
                UiMessages.ShowError("RunProgramCommand: FireAndForget=true requires OpenNewWindow=true. The in-pane execution path does not support fire-and-forget. Fix the command set definition.");
                return false;
            }

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
                    workingDirectory: WorkingDirectory ?? this.NodeContext.GetWorkingDirectory(),
                    fireAndForget: FireAndForget
                    ).ConfigureAwait(false);
            }
        }
    }
}
