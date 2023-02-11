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
    public class DeleteBucketCommand : ICommand
    {
        public bool OpenNewWindow { get; set; } = false;

        public string CommandDocumentation { get; set; } = "Deletes an empty bucket.";

        public string? BucketNameToDelete { get; set; }

        public void ParseArgs(GemNodeContext nodeContext, Stack<string> arguments)
        {
            if (arguments.Count > 0)
            {
                BucketNameToDelete = arguments.Pop();
            }
        }

        public async Task<bool> Execute(GemNodeContext nodeContext, MainWindow mainWindow)
        {
            if (nodeContext.Repo == null)
            {
                return false;
            }

            // If bucket wasn't passed in on the node context then try to parse it from properties
            if (nodeContext.Bucket == null && !string.IsNullOrWhiteSpace(BucketNameToDelete))
            {
                nodeContext.Bucket = nodeContext.Repo.Buckets.FirstOrDefault(b => b.GemName != null && b.GemName.Equals(BucketNameToDelete, StringComparison.OrdinalIgnoreCase));
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
