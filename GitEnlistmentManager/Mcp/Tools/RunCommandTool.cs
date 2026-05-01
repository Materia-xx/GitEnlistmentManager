using GitEnlistmentManager.ClientServer;
using GitEnlistmentManager.DTOs;
using GitEnlistmentManager.Globals;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace GitEnlistmentManager.Mcp.Tools
{
    public class RunCommandTool : McpTool
    {
        public override string Name => "run_command";

        public override string Description => "Run a GEM command verb in the context of a directory within the GEM tree. Use list_commands to discover available verbs. The directory determines the context level (enlistment, bucket, target branch, etc).";

        public override JObject InputSchema => new JObject
        {
            ["type"] = "object",
            ["properties"] = new JObject
            {
                ["directory"] = new JObject
                {
                    ["type"] = "string",
                    ["description"] = "Full path to a directory within the GEM tree. Use the list_tree tool to discover valid paths for repo collections, repos, target branches, buckets, and enlistments."
                },
                ["verb"] = new JObject
                {
                    ["type"] = "string",
                    ["description"] = "The GEM command verb to run (e.g. 'dev2026', 'openrootsolution', 'spawncompare'). Use list_commands to see available verbs."
                },
                ["args"] = new JObject
                {
                    ["type"] = "array",
                    ["items"] = new JObject { ["type"] = "string" },
                    ["description"] = "Optional positional arguments passed to the verb after the verb itself (e.g. for 'archiveenlistment' pass [\"010000.w1\"]; for 'spawncompare' pass two directory paths). Most verbs take no args."
                }
            },
            ["required"] = new JArray("directory", "verb")
        };

        public override async Task<McpToolResult> Execute(JObject? arguments)
        {
            if (arguments == null)
            {
                return McpToolResult.Error("Arguments are required");
            }

            var directory = arguments["directory"]?.ToString();
            var verb = arguments["verb"]?.ToString();
            var argsToken = arguments["args"] as JArray;

            if (string.IsNullOrWhiteSpace(directory) || string.IsNullOrWhiteSpace(verb))
            {
                return McpToolResult.Error("Both directory and verb are required");
            }

            // Check if this specific verb is disabled
            if (Gem.Instance.LocalAppData.DisabledMcpTools.Contains($"run_command:{verb}"))
            {
                return McpToolResult.Error($"Command '{verb}' is currently disabled in GEM settings.");
            }

            // Refuse verbs whose only command sets are hidden from MCP. If at least one
            // matching command set is exposed, allow dispatch and let the placement-based
            // resolution in MainWindow.ProcessCSCommand pick the right one.
            var anyExposed = Gem.Instance.CommandSets.Any(cs =>
                cs.Verb != null
                && cs.Verb.Equals(verb, StringComparison.OrdinalIgnoreCase)
                && cs.ExposeToMcp);
            if (!anyExposed)
            {
                return McpToolResult.Error($"Command '{verb}' is not exposed to MCP.");
            }

            var commandArgs = new System.Collections.Generic.List<object> { verb };
            if (argsToken != null)
            {
                foreach (var token in argsToken)
                {
                    var s = token?.ToString();
                    if (s != null)
                    {
                        commandArgs.Add(s);
                    }
                }
            }

            var command = new GemCSCommand
            {
                CommandType = GemCSCommandType.InterpretCommandLine,
                CommandArgs = commandArgs.ToArray(),
                WorkingDirectory = directory,
                AutoExpandTreeView = true
            };

            var dispatchResult = await Global.Instance.MainWindow.ProcessCSCommand(command).ConfigureAwait(false);

            if (!dispatchResult.Success)
            {
                return McpToolResult.Error(dispatchResult.ErrorMessage);
            }

            var result = new
            {
                message = $"Command '{verb}' executed in '{directory}'",
                info = dispatchResult.InfoMessages
            };
            return McpToolResult.Success(JsonConvert.SerializeObject(result, Formatting.Indented));
        }
    }
}
