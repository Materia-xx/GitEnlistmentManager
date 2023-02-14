using GitEnlistmentManager.CommandSets;
using GitEnlistmentManager.DTOs;
using GitEnlistmentManager.Extensions;
using GitEnlistmentManager.Globals;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;

namespace GitEnlistmentManager.Commands
{
    public class CreateBucketCommand : Command
    {
        public CreateBucketCommand() 
        {
            this.CommandDocumentation = "Creates a bucket attached to a repository of choice.";
        }

        public string? BucketName { get; set; }

        public Bucket? ResultBucket { get; private set; }

        public override void ParseArgs(Stack<string> arguments)
        {
            if (arguments.Count > 0)
            {
                BucketName = arguments.Pop();
            }
        }

        public override async Task<bool> Execute()
        {
            if (this.NodeContext.Repo == null)
            {
                return false;
            }

            ResultBucket = new Bucket(this.NodeContext.Repo);
            ResultBucket.GemName = BucketName;

            if (string.IsNullOrEmpty(ResultBucket.GemName))
            {
                bool? result = null;
                await Application.Current.Dispatcher.BeginInvoke(() =>
                {
                    var bucketSettingsEditor = new BucketSettings(ResultBucket);
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
                await Global.Instance.MainWindow.RunCommandSets(afterBucketCreateCommandSets, GemNodeContext.GetNodeContext(bucket: ResultBucket)).ConfigureAwait(false);
            }
            return true;
        }
    }
}
