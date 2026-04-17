using GitEnlistmentManager.DTOs;
using GitEnlistmentManager.Extensions;
using GitEnlistmentManager.Globals;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace GitEnlistmentManager.Mcp.Tools
{
    public class CreateEnlistmentTool : McpTool
    {
        public override string Name => "create_enlistment";

        public override string Description => "Create a new enlistment (git clone/worktree) in a bucket. This clones the repo and sets up a new working copy.";

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
                    ["description"] = "Name of the bucket to create the enlistment in"
                },
                ["enlistmentName"] = new JObject
                {
                    ["type"] = "string",
                    ["description"] = "Name for the new enlistment"
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
                return McpToolResult.Error("All required parameters must be provided");
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

            // Create the enlistment using the existing extension method
            var enlistment = new Enlistment(bucket) { GemName = enlistmentName };

            try
            {
                var success = await enlistment.CreateEnlistment(
                    enlistmentPlacement: EnlistmentPlacement.PlaceAtEnd,
                    childEnlistment: null,
                    scopeToBranch: true,
                    gitAutoCrlf: false).ConfigureAwait(false);

                if (!success)
                {
                    return McpToolResult.Error("Enlistment creation failed. Check that the repo has a valid clone URL, branch prefix, and user settings configured.");
                }

                var enlistmentDir = enlistment.GetDirectoryInfo();
                var result = new
                {
                    message = $"Enlistment '{enlistmentName}' created successfully",
                    path = enlistmentDir?.FullName
                };

                // Refresh the UI tree to show the new enlistment
                await Global.Instance.MainWindow.ReloadTreeview().ConfigureAwait(false);

                return McpToolResult.Success(JsonConvert.SerializeObject(result, Formatting.Indented));
            }
            catch (Exception ex)
            {
                return McpToolResult.Error($"Failed to create enlistment: {ex.Message}");
            }
        }
    }
}
