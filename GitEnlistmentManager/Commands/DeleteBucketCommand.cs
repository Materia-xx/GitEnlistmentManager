using GitEnlistmentManager.DTOs;
using GitEnlistmentManager.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace GitEnlistmentManager.Commands
{
    public class DeleteBucketCommand : Command
    {
        public DeleteBucketCommand() 
        {
            this.Documentation = "Deletes an empty bucket.";
        }

        public string? BucketNameToDelete { get; set; }

        public override void ParseArgs(Stack<string> arguments)
        {
            if (arguments.Count > 0)
            {
                BucketNameToDelete = arguments.Pop();
            }
        }

        public override async Task<bool> Execute()
        {
            if (this.NodeContext.Repo == null)
            {
                return false;
            }

            // If bucket wasn't passed in on the node context then try to parse it from properties
            if (this.NodeContext.Bucket == null && !string.IsNullOrWhiteSpace(BucketNameToDelete))
            {
                this.NodeContext.BaseNodeContext.Bucket = this.NodeContext.Repo.Buckets.FirstOrDefault(b => b.GemName != null && b.GemName.Equals(BucketNameToDelete, StringComparison.OrdinalIgnoreCase));
            }

            // If bucket still isn't set then we don't know what bucket to delete
            if (this.NodeContext.Bucket == null)
            {
                return false;
            }

            var bucketDirectory = this.NodeContext.Bucket.GetDirectoryInfo();
            if (bucketDirectory == null)
            {
                return false;
            }

            if (bucketDirectory.GetFiles("*", SearchOption.TopDirectoryOnly).Length
                + bucketDirectory.GetDirectories("*", SearchOption.TopDirectoryOnly).Length > 0)
            {
                MessageBox.Show("Files or directories still exist in this bucket. Clean those up first and try again.");
                return false;
            }

            try
            {
                bucketDirectory.Delete();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return false;
            }

            return await Task.FromResult(true).ConfigureAwait(false);
        }
    }
}
