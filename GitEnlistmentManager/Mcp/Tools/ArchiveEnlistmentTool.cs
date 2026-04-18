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
    public class ArchiveEnlistmentTool : McpTool
    {
        public override string Name => "archive_enlistment";

        public override string Description => "Archive an enlistment (git worktree). This moves the enlistment to an archive directory. Only use this when the PR has been completed or the user specifically asks to archive a particular enlistment by a non-ambiguous name.";

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
                    ["description"] = "Name of the bucket containing the enlistment"
                },
                ["enlistmentName"] = new JObject
                {
                    ["type"] = "string",
                    ["description"] = "Name of the enlistment to archive (e.g. '010000.w1')"
                }
            },
            ["required"] = new JArray("repoCollectionName", "repoName", "branchName", "bucketName", "enlistmentName")
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
            var enlistmentName = arguments["enlistmentName"]?.ToString();

            if (string.IsNullOrWhiteSpace(repoCollectionName) || string.IsNullOrWhiteSpace(repoName) ||
                string.IsNullOrWhiteSpace(branchName) || string.IsNullOrWhiteSpace(bucketName) ||
                string.IsNullOrWhiteSpace(enlistmentName))
            {
                return McpToolResult.Error("All parameters are required: repoCollectionName, repoName, branchName, bucketName, enlistmentName");
            }

            // Find the enlistment through the hierarchy
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

            var enlistment = bucket.Enlistments.FirstOrDefault(
                e => e.GemName != null && e.GemName.Equals(enlistmentName, StringComparison.OrdinalIgnoreCase));
            if (enlistment == null)
            {
                return McpToolResult.Error($"Enlistment '{enlistmentName}' not found in bucket '{bucketName}'");
            }

            try
            {
                var archiveCommand = new ArchiveEnlistmentCommand();
                archiveCommand.NodeContext.SetIfNotNullFrom(
                    GemNodeContext.GetNodeContext(enlistment: enlistment));

                var success = await archiveCommand.Execute().ConfigureAwait(false);
                if (!success)
                {
                    return McpToolResult.Error("Archive failed. The enlistment directory may not exist or an archive slot could not be found.");
                }

                // Refresh the UI tree
                await Global.Instance.MainWindow.ReloadTreeview().ConfigureAwait(false);

                var result = new
                {
                    message = $"Enlistment '{enlistmentName}' archived successfully from bucket '{bucketName}'"
                };
                return McpToolResult.Success(JsonConvert.SerializeObject(result, Formatting.Indented));
            }
            catch (Exception ex)
            {
                return McpToolResult.Error($"Failed to archive enlistment: {ex.Message}");
            }
        }
    }
}
