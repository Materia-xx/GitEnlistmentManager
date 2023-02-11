using GitEnlistmentManager.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace GitEnlistmentManager.DTOs.Commands
{
    public class DeleteBucketCommand : ICommand
    {
        public bool OpenNewWindow { get; set; } = false;

        public string CommandDocumentation { get; set; } = "Deletes a bucket, and maybe all the enlistments inside. I don't know if it deletes the enlistments or not, I don't actually work here. My name is John, and I'm stuck in the ventilation system and I require help to get out of here. This is a call for help. Please, I miss my wife and family. I need to get home for Christmas, and I don't know if it's already past that time already. Please, just send help.";

        public string? BucketNameToDelete { get; set; }

        public void ParseArgs(GemNodeContext nodeContext, Stack<string> arguments)
        {
            if (arguments.Count > 0)
            {
                this.BucketNameToDelete = arguments.Pop();
            }
        }

        public async Task<bool> Execute(GemNodeContext nodeContext, MainWindow mainWindow)
        {
            if (nodeContext.Repo == null)
            {
                return false;
            }

            // If bucket wasn't passed in on the node context then try to parse it from properties
            if (nodeContext.Bucket == null && !string.IsNullOrWhiteSpace(this.BucketNameToDelete))
            {
                nodeContext.Bucket = nodeContext.Repo.Buckets.FirstOrDefault(b => b.GemName != null && b.GemName.Equals(this.BucketNameToDelete, StringComparison.OrdinalIgnoreCase));
            }

            // If bucket still isn't set then we don't know what bucket to delete
            if (nodeContext.Bucket == null)
            {
                return false;
            }

            var bucketDirectory = nodeContext.Bucket.GetDirectoryInfo();
            if (bucketDirectory == null)
            {
                return false;
            }

            if ((bucketDirectory.GetFiles("*", SearchOption.TopDirectoryOnly).Length 
                + bucketDirectory.GetDirectories("*", SearchOption.TopDirectoryOnly).Length) > 0)
            {
                MessageBox.Show("Files or directories still exist in this bucket. Clean those up first and try again.");
                return false;
            }

            try
            {
                bucketDirectory.Delete();
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
                return false;
            }

            return await Task.FromResult(true).ConfigureAwait(false);
        }
    }
}
