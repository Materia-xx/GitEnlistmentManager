using GitEnlistmentManager.ClientServer;
using GitEnlistmentManager.DTOs;
using GitEnlistmentManager.Globals;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
                    ["description"] = "Full path to a directory within the GEM tree (e.g. an enlistment path like 'F:\\gem\\Halo\\svc\\m\\acrdocker\\010000.w1')"
                },
                ["verb"] = new JObject
                {
                    ["type"] = "string",
                    ["description"] = "The GEM command verb to run (e.g. 'dev2026', 'ors', 'lt'). Use list_commands to see available verbs."
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

            if (string.IsNullOrWhiteSpace(directory) || string.IsNullOrWhiteSpace(verb))
            {
                return McpToolResult.Error("Both directory and verb are required");
            }

            // Check if this specific verb is disabled
            if (Gem.Instance.LocalAppData.DisabledMcpTools.Contains($"run_command:{verb}"))
            {
                return McpToolResult.Error($"Command '{verb}' is currently disabled in GEM settings.");
            }

            var command = new GemCSCommand
            {
                CommandType = GemCSCommandType.InterpretCommandLine,
                CommandArgs = new object[] { verb },
                WorkingDirectory = directory
            };

            await Global.Instance.MainWindow.ProcessCSCommand(command).ConfigureAwait(false);

            var result = new
            {
                message = $"Command '{verb}' executed in '{directory}'"
            };
            return McpToolResult.Success(JsonConvert.SerializeObject(result, Formatting.Indented));
        }
    }
}
