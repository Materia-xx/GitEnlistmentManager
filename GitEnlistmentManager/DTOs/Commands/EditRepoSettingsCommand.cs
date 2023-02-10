using System.Collections.Generic;
using System.Threading.Tasks;

namespace GitEnlistmentManager.DTOs.Commands
{
    public class EditRepoSettingsCommand : ICommand
    {
        public bool OpenNewWindow { get; set; } = false;

        public void ParseArgs(GemNodeContext nodeContext, Stack<string> arguments)
        {
        }

        public async Task<bool> Execute(GemNodeContext nodeContext, MainWindow mainWindow)
        {
            if (nodeContext.Repo == null)
            {
                return false;
            }

            bool? result = null;

            await mainWindow.Dispatcher.InvokeAsync(() =>
            {
                var repoSettingsEditor = new RepoSettings(nodeContext.Repo, isNew: false);
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
