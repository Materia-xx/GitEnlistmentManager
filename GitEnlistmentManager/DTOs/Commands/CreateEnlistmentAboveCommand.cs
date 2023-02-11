using GitEnlistmentManager.Extensions;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GitEnlistmentManager.DTOs.Commands
{
    internal class CreateEnlistmentAboveCommand : ICommand
    {
        public bool OpenNewWindow { get; set; } = false;

        public string CommandDocumentation { get; set; } = "Inserts an enlistment above the selected one.";

        public void ParseArgs(GemNodeContext nodeContext, Stack<string> arguments)
        {
        }

        public async Task<bool> Execute(GemNodeContext nodeContext, MainWindow mainWindow)
        {
            if (nodeContext.Bucket == null || nodeContext.Enlistment == null)
            {
                return false;
            }

            var newEnlistment = new Enlistment(nodeContext.Bucket);

            bool? result = null;

            await mainWindow.Dispatcher.InvokeAsync(() =>
            {
                var enlistmentSettingsEditor = new EnlistmentSettings(newEnlistment);
                result = enlistmentSettingsEditor.ShowDialog();
            });
            if (!result.HasValue || !result.Value)
            {
                return false;
            }

            // After the editor closes, create the enlistment
            return await newEnlistment.CreateEnlistment(mainWindow, EnlistmentPlacement.PlaceAbove, childEnlistment: nodeContext.Enlistment).ConfigureAwait(false);
        }
    }
}
