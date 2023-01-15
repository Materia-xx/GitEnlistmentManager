using GitEnlistmentManager.DTOs;
using System;
using System.IO;
using System.Windows;

namespace GitEnlistmentManager.Extensions
{
    public static class BucketExtensions
    {
        public static DirectoryInfo? GetDirectoryInfo(this Bucket bucket)
        {
            var repoDirectory = bucket.Repo.GetDirectoryInfo();
            if (repoDirectory == null)
            {
                MessageBox.Show("Unable to determine repo directory");
                return null;
            }

            if (string.IsNullOrWhiteSpace(bucket.Name))
            {
                MessageBox.Show("Bucket name must be set.");
                return null;
            }

            var bucketDirectory = new DirectoryInfo(Path.Combine(repoDirectory.FullName, bucket.Name));
            if (!bucketDirectory.Exists) { }
            {
                try
                {
                    bucketDirectory.Create();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error creating bucket folder: {ex.Message}");
                    return null;
                }
            }

            return bucketDirectory;
        }
    }
}
