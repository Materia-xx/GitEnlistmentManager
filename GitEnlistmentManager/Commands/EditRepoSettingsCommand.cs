using System.Collections.Generic;
using System.Threading.Tasks;
using GitEnlistmentManager.DTOs;
using GitEnlistmentManager.Globals;

namespace GitEnlistmentManager.Commands
{
    public class EditRepoSettingsCommand : Command
    {
        public EditRepoSettingsCommand()
        {
            this.Documentation = "Opens the Repository settings editor.";
        }

        public override async Task<bool> Execute()
        {
            if (this.NodeContext.Repo == null)
            {
                return false;
            }

            bool? result = null;

            await Global.Instance.MainWindow.Dispatcher.InvokeAsync(() =>
            {
                var repoSettingsEditor = new RepoSettings(this.NodeContext.Repo, isNew: false);
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
