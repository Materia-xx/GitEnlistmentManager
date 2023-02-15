using System.Collections.Generic;
using System.Threading.Tasks;
using GitEnlistmentManager.DTOs;
using GitEnlistmentManager.Globals;

namespace GitEnlistmentManager.Commands
{
    internal class CreateRepoCommand : Command
    {
        public CreateRepoCommand()
        {
            this.Documentation = "Creates a Repository attached to a Repository Collection, defined in your settings.";
        }

        public override async Task<bool> Execute()
        {
            if (this.NodeContext.RepoCollection == null)
            {
                return false;
            }

            bool? result = null;
            await Global.Instance.MainWindow.Dispatcher.InvokeAsync(() =>
            {
                var repoSettingsEditor = new RepoSettings(new Repo(this.NodeContext.RepoCollection), isNew: true);
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
