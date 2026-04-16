using GitEnlistmentManager.DTOs;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace GitEnlistmentManager.Mcp.Tools
{
    public class GetStatusTool : McpTool
    {
        public override string Name => "get_status";

        public override string Description => "Get a summary of the current GEM tree state including counts of repo collections, repos, target branches, buckets, and enlistments";

        public override JObject InputSchema => new JObject
        {
            ["type"] = "object",
            ["properties"] = new JObject(),
            ["required"] = new JArray()
        };

        public override Task<McpToolResult> Execute(JObject? arguments)
        {
            int repoCollectionCount = 0;
            int repoCount = 0;
            int targetBranchCount = 0;
            int bucketCount = 0;
            int enlistmentCount = 0;

            foreach (var rc in Gem.Instance.RepoCollections)
            {
                repoCollectionCount++;
                foreach (var repo in rc.Repos)
                {
                    repoCount++;
                    foreach (var tb in repo.TargetBranches)
                    {
                        targetBranchCount++;
                        foreach (var bucket in tb.Buckets)
                        {
                            bucketCount++;
                            enlistmentCount += bucket.Enlistments.Count;
                        }
                    }
                }
            }

            var status = new
            {
                reposDirectory = Gem.Instance.LocalAppData.ReposDirectory,
                repoCollections = repoCollectionCount,
                repos = repoCount,
                targetBranches = targetBranchCount,
                buckets = bucketCount,
                enlistments = enlistmentCount
            };

            var json = JsonConvert.SerializeObject(status, Formatting.Indented);
            return Task.FromResult(McpToolResult.Success(json));
        }
    }
}
