using System.Collections.Generic;

namespace GitEnlistmentManager.ClientServer
{
    /// <summary>
    /// Result of dispatching a <see cref="GemCSCommand"/> through MainWindow.ProcessCSCommand.
    /// Carries any dispatch-level errors (working directory invalid, unknown verb, missing repo
    /// collection, etc.) back to the caller so they can decide how to surface them — the CLI
    /// shows them as a MessageBox, while MCP returns them inside the tool response.
    /// </summary>
    public class CommandDispatchResult
    {
        public bool Success => Errors.Count == 0;

        public List<string> Errors { get; } = new List<string>();

        /// <summary>
        /// Informational messages produced during dispatch (e.g. "Launched devenv.exe (pid 1234)").
        /// Surfaces in the MCP response. Not shown to GUI/CLI users.
        /// </summary>
        public List<string> InfoMessages { get; } = new List<string>();

        public void AddError(string message)
        {
            Errors.Add(message);
        }

        public void AddInfo(string message)
        {
            InfoMessages.Add(message);
        }

        public string ErrorMessage => string.Join(System.Environment.NewLine, Errors);

        public string InfoMessage => string.Join(System.Environment.NewLine, InfoMessages);
    }
}
