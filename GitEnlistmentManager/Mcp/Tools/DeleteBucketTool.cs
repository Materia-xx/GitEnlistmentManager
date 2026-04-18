using GitEnlistmentManager.Commands;
using GitEnlistmentManager.DTOs;
using GitEnlistmentManager.Globals;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace GitEnlistmentManager.Mcp.Tools
{
    public class DeleteBucketTool : McpTool
    {
        public override string Name => "delete_bucket";

        public override string Description => "Delete an empty bucket. The bucket must have no enlistments - archive them individually first.";

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
                    ["description"] = "Name of the repo"
                },
                ["branchName"] = new JObject
                {
                    ["type"] = "string",
                    ["description"] = "The target branch name (e.g. 'master', 'main')"
                },
                ["bucketName"] = new JObject
                {
                    ["type"] = "string",
                    ["description"] = "Name of the bucket to delete"
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

            // Find the bucket through the hierarchy
            var repoCollection = Gem.Instance.RepoCollections.FirstOrDefault(
                rc => rc.GemName != null && rc.GemName.Equals(repoCollectionName, StringComparison.OrdinalIgnoreCase));
            if (repoCollection == null)
            {
                return McpToolResult.Error($"Repo collection '{repoCollectionName}' not found");
            }

            var repo = repoCollection.Repos.FirstOrDefault(
                r => r.GemName != null && r.GemName.Equals(repoName, StringComparison.OrdinalIgnoreCase));
            if (repo == null)
            {
                return McpToolResult.Error($"Repo '{repoName}' not found");
            }

            var targetBranch = repo.TargetBranches.FirstOrDefault(
                tb => tb.BranchDefinition.BranchFrom != null &&
                      tb.BranchDefinition.BranchFrom.Equals(branchName, StringComparison.OrdinalIgnoreCase));
            if (targetBranch == null)
            {
                return McpToolResult.Error($"Target branch '{branchName}' not found");
            }

            var bucket = targetBranch.Buckets.FirstOrDefault(
                b => b.GemName != null && b.GemName.Equals(bucketName, StringComparison.OrdinalIgnoreCase));
            if (bucket == null)
            {
                return McpToolResult.Error($"Bucket '{bucketName}' not found");
            }

            try
            {
                // Refuse to delete buckets that still have enlistments
                if (bucket.Enlistments.Count > 0)
                {
                    var enlistmentNames = string.Join(", ", bucket.Enlistments.Select(e => e.GemName));
                    return McpToolResult.Error($"Bucket '{bucketName}' still has {bucket.Enlistments.Count} enlistment(s): {enlistmentNames}. Do NOT automatically archive these. Only archive an enlistment when the user has confirmed the PR is completed or the user specifically asks to archive a particular enlistment by a non-ambiguous name.");
                }

                // Delete the empty bucket
                var deleteCommand = new DeleteBucketCommand();
                deleteCommand.NodeContext.SetIfNotNullFrom(
                    GemNodeContext.GetNodeContext(bucket: bucket));

                var deleteSuccess = await deleteCommand.Execute().ConfigureAwait(false);
                if (!deleteSuccess)
                {
                    return McpToolResult.Error("Failed to delete bucket. The directory may not be empty.");
                }

                // Remove bucket from the target branch
                targetBranch.Buckets.Remove(bucket);

                // Refresh the UI tree
                await Global.Instance.MainWindow.ReloadTreeview().ConfigureAwait(false);

                var result = new
                {
                    message = $"Bucket '{bucketName}' deleted successfully"
                };
                return McpToolResult.Success(JsonConvert.SerializeObject(result, Formatting.Indented));
            }
            catch (Exception ex)
            {
                return McpToolResult.Error($"Failed to delete bucket: {ex.Message}");
            }
        }
    }
}
