using GitEnlistmentManager.DTOs;
using GitEnlistmentManager.Extensions;
using GitEnlistmentManager.Globals;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace GitEnlistmentManager.Mcp.Tools
{
    public class CreateBucketTool : McpTool
    {
        public override string Name => "create_bucket";

        public override string Description => "Create a new bucket under a target branch in a repo. A bucket is a container for enlistments (git worktrees).";

        public override JObject InputSchema => new JObject
        {
            ["type"] = "object",
            ["properties"] = new JObject
            {
                ["repoCollectionName"] = new JObject
                {
                    ["type"] = "string",
                    ["description"] = "Name of the repo collection"
                },
                ["repoName"] = new JObject
                {
                    ["type"] = "string",
                    ["description"] = "Name of the repo (use the display name, not the short name)"
                },
                ["branchName"] = new JObject
                {
                    ["type"] = "string",
                    ["description"] = "The target branch name (e.g. 'master', 'main') to create the bucket under"
                },
                ["bucketName"] = new JObject
                {
                    ["type"] = "string",
                    ["description"] = "Name for the new bucket"
                }
            },
            ["required"] = new JArray("repoCollectionName", "repoName", "branchName", "bucketName")
        };

        public override async Task<McpToolResult> Execute(JObject? arguments)
        {
            if (arguments == null)
            {
                return McpToolResult.Error("Arguments are required");
            }

            var repoCollectionName = arguments["repoCollectionName"]?.ToString();
            var repoName = arguments["repoName"]?.ToString();
            var branchName = arguments["branchName"]?.ToString();
            var bucketName = arguments["bucketName"]?.ToString();

            if (string.IsNullOrWhiteSpace(repoCollectionName) || string.IsNullOrWhiteSpace(repoName) ||
                string.IsNullOrWhiteSpace(branchName) || string.IsNullOrWhiteSpace(bucketName))
            {
                return McpToolResult.Error("All parameters are required: repoCollectionName, repoName, branchName, bucketName");
            }

            // Find the repo collection
            var repoCollection = Gem.Instance.RepoCollections.FirstOrDefault(
                rc => rc.GemName != null && rc.GemName.Equals(repoCollectionName, StringComparison.OrdinalIgnoreCase));
            if (repoCollection == null)
            {
                return McpToolResult.Error($"Repo collection '{repoCollectionName}' not found");
            }

            // Find the repo
            var repo = repoCollection.Repos.FirstOrDefault(
                r => r.GemName != null && r.GemName.Equals(repoName, StringComparison.OrdinalIgnoreCase));
            if (repo == null)
            {
                return McpToolResult.Error($"Repo '{repoName}' not found in collection '{repoCollectionName}'");
            }

            // Find the target branch
            var targetBranch = repo.TargetBranches.FirstOrDefault(
                tb => tb.BranchDefinition.BranchFrom != null &&
                      tb.BranchDefinition.BranchFrom.Equals(branchName, StringComparison.OrdinalIgnoreCase));
            if (targetBranch == null)
            {
                return McpToolResult.Error($"Target branch '{branchName}' not found in repo '{repoName}'");
            }

            // Check if bucket already exists
            var existingBucket = targetBranch.Buckets.FirstOrDefault(
                b => b.GemName != null && b.GemName.Equals(bucketName, StringComparison.OrdinalIgnoreCase));
            if (existingBucket != null)
            {
                return McpToolResult.Error($"Bucket '{bucketName}' already exists");
            }

            // Create the bucket
            var bucket = new Bucket(targetBranch) { GemName = bucketName };

            // Create the directory
            var targetBranchDir = targetBranch.GetDirectoryInfo();
            if (targetBranchDir == null)
            {
                return McpToolResult.Error("Unable to determine target branch directory");
            }

            try
            {
                var bucketDir = new DirectoryInfo(Path.Combine(targetBranchDir.FullName, bucketName));
                if (!bucketDir.Exists)
                {
                    bucketDir.Create();
                }

                targetBranch.Buckets.Add(bucket);

                // Refresh the UI tree to show the new bucket
                await Global.Instance.MainWindow.ReloadTreeview().ConfigureAwait(false);

                var result = new
                {
                    message = $"Bucket '{bucketName}' created successfully",
                    path = bucketDir.FullName
                };
                return McpToolResult.Success(JsonConvert.SerializeObject(result, Formatting.Indented));
            }
            catch (Exception ex)
            {
                return McpToolResult.Error($"Failed to create bucket directory: {ex.Message}");
            }
        }
    }
}
