using GitEnlistmentManager.DTOs;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GitEnlistmentManager.Mcp.Tools
{
    public class ListCommandsTool : McpTool
    {
        public override string Name => "list_commands";

        public override string Description => "List all available GEM command verbs with their placement level and description. Use this to discover what commands can be run with the run_command tool.";

        public override JObject InputSchema => new JObject
        {
            ["type"] = "object",
            ["properties"] = new JObject(),
            ["required"] = new JArray()
        };

        public override Task<McpToolResult> Execute(JObject? arguments)
        {
            var commands = new List<object>();

            foreach (var commandSet in Gem.Instance.CommandSets)
            {
                if (string.IsNullOrWhiteSpace(commandSet.Verb))
                {
                    continue;
                }

                // Hidden from MCP entirely (e.g. dialog openers, GEM-internal helpers).
                if (!commandSet.ExposeToMcp)
                {
                    continue;
                }

                // Skip verbs that are disabled in settings
                if (Gem.Instance.LocalAppData.DisabledMcpTools.Contains($"run_command:{commandSet.Verb}"))
                {
                    continue;
                }

                commands.Add(new
                {
                    verb = commandSet.Verb,
                    placement = commandSet.Placement.ToString(),
                    description = commandSet.Documentation ?? commandSet.RightClickText ?? string.Empty
                });
            }

            var json = JsonConvert.SerializeObject(commands, Formatting.Indented);
            return Task.FromResult(McpToolResult.Success(json));
        }
    }
}
