using GitEnlistmentManager.Extensions;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;

namespace GitEnlistmentManager.DTOs.Commands
{
    public class CreateBucketCommand : ICommand
    {
        public bool OpenNewWindow { get; set; } = false;

        public string? BucketName { get; set; }

        public Bucket? ResultBucket { get; private set; }

        public void ParseArgs(GemNodeContext nodeContext, Stack<string> arguments)
        {
            if (arguments.Count > 0)
            {
                this.BucketName = arguments.Pop();
            }
        }

        public async Task<bool> Execute(GemNodeContext nodeContext, MainWindow mainWindow)
        {
            if (nodeContext.Repo != null)
            {
                this.ResultBucket = new Bucket(nodeContext.Repo);
                this.ResultBucket.GemName = this.BucketName;

                if (string.IsNullOrEmpty(this.ResultBucket.GemName))
                {
                    bool? result = null;
                    await Application.Current.Dispatcher.BeginInvoke(() =>
                    {
                        var bucketSettingsEditor = new BucketSettings(this.ResultBucket, mainWindow);
                        result = bucketSettingsEditor.ShowDialog();
                    });
                    if (result.HasValue && !result.Value)
                    {
                        return false;
                    }
                }

                // Force directory for the bucket to be created
                if (this.ResultBucket.GetDirectoryInfo() != null)
                {
                    // Run any "AfterBucketCreate" command sets 
                    var afterBucketCreateCommandSets = this.ResultBucket.Repo.RepoCollection.Gem.GetCommandSets(CommandSetPlacement.AfterBucketCreate, CommandSetMode.Any, this.ResultBucket.Repo.RepoCollection, this.ResultBucket.Repo, this.ResultBucket);
                    await mainWindow.RunCommandSets(afterBucketCreateCommandSets, GemNodeContext.GetNodeContext(bucket: this.ResultBucket)).ConfigureAwait(false);
                }
                return true;
            }
            return false;
        }
    }
}
