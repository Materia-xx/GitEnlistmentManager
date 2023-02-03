using GitEnlistmentManager.Extensions;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;

namespace GitEnlistmentManager.DTOs.Commands
{
    public class CreateEnlistmentCommand : ICommand
    {
        public bool OpenNewWindow { get; set; } = false;

        public void ParseArgs(GemNodeContext nodeContext, Stack<string> arguments)
        {
        }

        public async Task<bool> Execute(GemNodeContext nodeContext, MainWindow mainWindow)
        {
            if (nodeContext.Bucket != null)
            {
                bool? result = null;
                var enlistment = new Enlistment(nodeContext.Bucket);
                await Application.Current.Dispatcher.BeginInvoke(() =>
                {
                    var enlistmentSettingsEditor = new EnlistmentSettings(enlistment);
                    result = enlistmentSettingsEditor.ShowDialog();
                });
                if (result.HasValue && !result.Value) 
                {
                    return false;
                }

                // After the editor closes, create the enlistment
                return await enlistment.CreateEnlistment(mainWindow, EnlistmentPlacement.PlaceAtEnd).ConfigureAwait(false);
            }
            return false;
        }
    }
}
