using System.Collections.Generic;
using System.Threading.Tasks;
using GitEnlistmentManager.DTOs;

namespace GitEnlistmentManager.Commands
{
    internal class CreateRepoCommand : ICommand
    {
        public bool OpenNewWindow { get; set; } = false;

        public string CommandDocumentation { get; set; } = "Creates a Repository attached to a Respository Collection, defined in your settings.";
        public void ParseArgs(GemNodeContext nodeContext, Stack<string> arguments)
        {
        }

        public async Task<bool> Execute(GemNodeContext nodeContext, MainWindow mainWindow)
        {
            if (nodeContext.RepoCollection == null)
            {
                return false;
            }

            bool? result = null;
            await mainWindow.Dispatcher.InvokeAsync(() =>
            {
                var repoSettingsEditor = new RepoSettings(new Repo(nodeContext.RepoCollection), isNew: true);
                result = repoSettingsEditor.ShowDialog();
            });
            if (!result.HasValue || !result.Value)
            {
                return false;
            }
            return true;
        }
    }
}
