using GitEnlistmentManager.DTOs;
using GitEnlistmentManager.Globals;
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
            var targetBranchDirectory = bucket.TargetBranch.GetDirectoryInfo();
            if (targetBranchDirectory == null)
            {
                UiMessages.ShowError("Unable to determine target branch directory");
                return null;
            }

            if (string.IsNullOrWhiteSpace(bucket.GemName))
            {
                UiMessages.ShowError("Bucket name must be set.");
                return null;
            }

            var bucketDirectory = new DirectoryInfo(Path.Combine(targetBranchDirectory.FullName, bucket.GemName));
            if (!bucketDirectory.Exists)
            {
                try
                {
                    bucketDirectory.Create();
                }
                catch (Exception ex)
                {
                    UiMessages.ShowError($"Error creating bucket directory: {ex.Message}");
                    return null;
                }
            }

            return bucketDirectory;
        }

        public static Dictionary<string, string> GetTokens(this Bucket bucket)
        {
            var tokens = bucket.TargetBranch.GetTokens();

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
