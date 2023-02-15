using System.Collections.Generic;
using System.Threading.Tasks;
using GitEnlistmentManager.DTOs;
using GitEnlistmentManager.Globals;

namespace GitEnlistmentManager.Commands
{
    public class EditGemSettingsCommand : Command
    {
        public EditGemSettingsCommand()
        {
            this.Documentation = "Opens the Gem Settings editor.";
        }

        public override async Task<bool> Execute()
        {
            bool? result = null;
            await Global.Instance.MainWindow.Dispatcher.InvokeAsync(() =>
            {
                var gemSettingsEditor = new GemSettings(Gem.Instance);
                result = gemSettingsEditor.ShowDialog();
            });
            return result.HasValue && result.Value;
        }
    }
}
