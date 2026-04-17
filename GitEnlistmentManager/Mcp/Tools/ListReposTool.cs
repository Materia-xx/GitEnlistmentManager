using GitEnlistmentManager.DTOs;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GitEnlistmentManager.Mcp.Tools
{
    public class ListReposTool : McpTool
    {
        public override string Name => "list_repos";

        public override string Description => "List all repo collections, repos, target branches, buckets, and enlistments in the GEM tree";

        public override JObject InputSchema => new JObject
        {
            ["type"] = "object",
            ["properties"] = new JObject(),
            ["required"] = new JArray()
        };

        public override Task<McpToolResult> Execute(JObject? arguments)
        {
            var collections = new List<object>();

            foreach (var rc in Gem.Instance.RepoCollections)
            {
                var repos = new List<object>();
                foreach (var repo in rc.Repos)
                {
                    var targetBranches = new List<object>();
                    foreach (var tb in repo.TargetBranches)
                    {
                        var buckets = new List<object>();
                        foreach (var bucket in tb.Buckets)
                        {
                            var enlistments = new List<object>();
                            foreach (var enlistment in bucket.Enlistments)
                            {
                                enlistments.Add(new
                                {
                                    name = enlistment.GemName
                                });
                            }

                            buckets.Add(new
                            {
                                name = bucket.GemName,
                                enlistments
                            });
                        }

                        targetBranches.Add(new
                        {
                            branchFrom = tb.BranchDefinition.BranchFrom,
                            buckets
                        });
                    }

                    repos.Add(new
                    {
                        name = repo.GemName,
                        targetBranches
                    });
                }

                collections.Add(new
                {
                    name = rc.GemName,
                    repos
                });
            }

            var json = JsonConvert.SerializeObject(collections, Formatting.Indented);
            return Task.FromResult(McpToolResult.Success(json));
        }
    }
}
