using GitEnlistmentManager.CommandSets;
using GitEnlistmentManager.DTOs;
using GitEnlistmentManager.Extensions;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;

namespace GitEnlistmentManager.Commands
{
    public class CreateBucketCommand : ICommand
    {
        public bool OpenNewWindow { get; set; } = false;

        public string CommandDocumentation { get; set; } = "Creates a bucket attached to a repository of choice.";

        public string? BucketName { get; set; }

        public Bucket? ResultBucket { get; private set; }

        public void ParseArgs(GemNodeContext nodeContext, Stack<string> arguments)
        {
            if (arguments.Count > 0)
            {
                BucketName = arguments.Pop();
            }
        }

        public async Task<bool> Execute(GemNodeContext nodeContext, MainWindow mainWindow)
        {
            if (nodeContext.Repo == null)
            {
                return false;
            }

            ResultBucket = new Bucket(nodeContext.Repo);
            ResultBucket.GemName = BucketName;

            if (string.IsNullOrEmpty(ResultBucket.GemName))
            {
                bool? result = null;
                await Application.Current.Dispatcher.BeginInvoke(() =>
                {
                    var bucketSettingsEditor = new BucketSettings(ResultBucket, mainWindow);
                    result = bucketSettingsEditor.ShowDialog();
                });
                if (!result.HasValue || !result.Value)
                {
                    return false;
                }
            }

            // Force directory for the bucket to be created
            if (ResultBucket.GetDirectoryInfo() != null)
            {
                // Run any "AfterBucketCreate" command sets 
                var afterBucketCreateCommandSets = ResultBucket.Repo.RepoCollection.Gem.GetCommandSets(CommandSetPlacement.AfterBucketCreate, CommandSetMode.Any, ResultBucket.Repo.RepoCollection, ResultBucket.Repo, ResultBucket);
                await mainWindow.RunCommandSets(afterBucketCreateCommandSets, GemNodeContext.GetNodeContext(bucket: ResultBucket)).ConfigureAwait(false);
            }
            return true;
        }
    }
}
