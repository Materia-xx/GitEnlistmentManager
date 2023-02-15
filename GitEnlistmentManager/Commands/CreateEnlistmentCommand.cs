using GitEnlistmentManager.DTOs;
using GitEnlistmentManager.Extensions;
using GitEnlistmentManager.Globals;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;

namespace GitEnlistmentManager.Commands
{
    public class CreateEnlistmentCommand : Command
    {
        public CreateEnlistmentCommand()
        {
            this.Documentation = "Creates an Enlistment attached to a Bucket of choice.";
        }

        public override async Task<bool> Execute()
        {
            if (this.NodeContext.Bucket != null)
            {
                var enlistment = new Enlistment(this.NodeContext.Bucket);
                EnlistmentSettings.EnlistmentSettingsDialogResult? result = null;

                await Application.Current.Dispatcher.BeginInvoke(() => 
                {
                    var enlistmentSettingsEditor = new EnlistmentSettings(enlistment.Bucket.GemName, enlistment.GemName);
                    result = enlistmentSettingsEditor.ShowDialog();
                    if (result != null)
                    {
                        enlistment.Bucket.GemName = result.BucketName;
                        enlistment.GemName = result.EnlistmentName;
                    }
                });
                if (result == null)
                {
                    return false;
                }

                // After the editor closes, create the enlistment
                return await enlistment.CreateEnlistment(EnlistmentPlacement.PlaceAtEnd, childEnlistment: null, result.ScopeToBranch).ConfigureAwait(false);
            }
            return false;
        }
    }
}
