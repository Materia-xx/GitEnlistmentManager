using GitEnlistmentManager.DTOs;
using GitEnlistmentManager.Extensions;
using GitEnlistmentManager.Globals;
using System.Threading.Tasks;

namespace GitEnlistmentManager.Commands
{
    internal class CreateEnlistmentAboveCommand : Command
    {
        public CreateEnlistmentAboveCommand()
        {
            Documentation = "Inserts an enlistment above the selected one.";
        }

        public override async Task<bool> Execute()
        {
            if (this.NodeContext.Bucket == null || this.NodeContext.Enlistment == null)
            {
                return false;
            }

            var newEnlistment = new Enlistment(this.NodeContext.Bucket);
            EnlistmentSettings.EnlistmentSettingsDialogResult? result = null;
            await Global.Instance.MainWindow.Dispatcher.InvokeAsync(() =>
            {
                var enlistmentSettingsEditor = new EnlistmentSettings(newEnlistment.Bucket.GemName, newEnlistment.GemName);
                result = enlistmentSettingsEditor.ShowDialog();
                if (result != null)
                {
                    newEnlistment.Bucket.GemName = result.BucketName;
                    newEnlistment.GemName = result.EnlistmentName;
                }
            });
            if (result == null)
            {
                return false;
            }

            // After the editor closes, create the enlistment
            return await newEnlistment.CreateEnlistment(EnlistmentPlacement.PlaceAbove, childEnlistment: this.NodeContext.Enlistment, result.ScopeToBranch, result.GitAutoCrlf).ConfigureAwait(false);
        }
    }
}
