using GitEnlistmentManager.DTOs;
using GitEnlistmentManager.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GitEnlistmentManager.Mcp.Tools
{
    public class ListTreeTool : McpTool
    {
        public override string Name => "list_tree";

        public override string Description => "List the entire GEM tree: repo collections, repos, target branches, buckets, and enlistments.";

        public override JObject InputSchema => new JObject
        {
            ["type"] = "object",
            ["properties"] = new JObject(),
            ["required"] = new JArray()
        };

        public override Task<McpToolResult> Execute(JObject? arguments)
        {
            var collections = new List<object>();
            var reposDirectory = Gem.Instance.LocalAppData.ReposDirectory;

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
                                    name = enlistment.GemName,
                                    path = enlistment.GetDirectoryInfo()?.FullName
                                });
                            }

                            buckets.Add(new
                            {
                                name = bucket.GemName,
                                path = bucket.GetDirectoryInfo()?.FullName,
                                enlistments
                            });
                        }

                        targetBranches.Add(new
                        {
                            branchFrom = tb.BranchDefinition.BranchFrom,
                            folderName = tb.BranchDefinition.FolderName,
                            path = tb.GetDirectoryInfo()?.FullName,
                            buckets
                        });
                    }

                    repos.Add(new
                    {
                        name = repo.GemName,
                        shortName = repo.Metadata.ShortName,
                        path = repo.GetDirectoryInfo()?.FullName,
                        targetBranches
                    });
                }

                collections.Add(new
                {
                    name = rc.GemName,
                    path = string.IsNullOrWhiteSpace(reposDirectory)
                        ? rc.RepoCollectionDirectoryPath
                        : System.IO.Path.Combine(reposDirectory, rc.GemName),
                    repos
                });
            }

            var json = JsonConvert.SerializeObject(collections, Formatting.Indented);
            return Task.FromResult(McpToolResult.Success(json));
        }
    }
}
