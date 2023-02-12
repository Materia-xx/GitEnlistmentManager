using GitEnlistmentManager.DTOs;
using GitEnlistmentManager.Extensions;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GitEnlistmentManager.Commands
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
            bool dialogSuccess = false;
            await mainWindow.Dispatcher.InvokeAsync(() =>
            {
                var enlistmentSettingsEditor = new EnlistmentSettings(newEnlistment.Bucket.GemName, newEnlistment.GemName);
                var result = enlistmentSettingsEditor.ShowDialog();
                if (result.HasValue && result.Value)
                {
                    newEnlistment.Bucket.GemName = enlistmentSettingsEditor.BucketName;
                    newEnlistment.GemName = enlistmentSettingsEditor.EnlistmentName;
                    dialogSuccess = true;
                }
                else
                {
                    dialogSuccess = false;
                }
            });
            if (!dialogSuccess)
            {
                return false;
            }

            // After the editor closes, create the enlistment
            return await newEnlistment.CreateEnlistment(mainWindow, EnlistmentPlacement.PlaceAbove, childEnlistment: nodeContext.Enlistment).ConfigureAwait(false);
        }
    }
}
