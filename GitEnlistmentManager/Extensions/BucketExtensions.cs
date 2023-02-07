using GitEnlistmentManager.DTOs;
using System;
using System.Collections.Generic;
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

            if (string.IsNullOrWhiteSpace(bucket.GemName))
            {
                MessageBox.Show("Bucket name must be set.");
                return null;
            }

            var bucketDirectory = new DirectoryInfo(Path.Combine(repoDirectory.FullName, bucket.GemName));
            if (!bucketDirectory.Exists)
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

        public static Dictionary<string, string> GetTokens(this Bucket bucket)
        {
            var tokens = bucket.Repo.GetTokens();

            if (bucket.GemName != null)
            {
                tokens["BucketName"] = bucket.GemName;
            }

            var bucketDirectory = bucket.GetDirectoryInfo();
            if (bucketDirectory != null)
            {
                tokens["BucketDirectory"] = bucketDirectory.FullName;
            }

            return tokens;
        }
    }
}
