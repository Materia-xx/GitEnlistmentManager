using GitEnlistmentManager.DTOs;
using GitEnlistmentManager.Extensions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;

namespace GitEnlistmentManager.Commands
{
    public class CreateEnlistmentCommand : ICommand
    {
        public bool OpenNewWindow { get; set; } = false;

        public string CommandDocumentation { get; set; } = "Creates an Enlistment attached to a Bucket of choice.";

        public void ParseArgs(GemNodeContext nodeContext, Stack<string> arguments)
        {
        }

        public async Task<bool> Execute(GemNodeContext nodeContext, MainWindow mainWindow)
        {
            if (nodeContext.Bucket != null)
            {
                var enlistment = new Enlistment(nodeContext.Bucket);
                bool dialogSuccess = false;
                await Application.Current.Dispatcher.BeginInvoke(() => 
                {
                    var enlistmentSettingsEditor = new EnlistmentSettings(enlistment.Bucket.GemName, enlistment.GemName);
                    var result = enlistmentSettingsEditor.ShowDialog();
                    if (result.HasValue && result.Value)
                    {
                        enlistment.Bucket.GemName = enlistmentSettingsEditor.BucketName;
                        enlistment.GemName = enlistmentSettingsEditor.EnlistmentName;
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
                return await enlistment.CreateEnlistment(mainWindow, EnlistmentPlacement.PlaceAtEnd).ConfigureAwait(false);
            }
            return false;
        }
    }
}
