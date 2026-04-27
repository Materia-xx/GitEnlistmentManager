using GitEnlistmentManager.ClientServer;
using GitEnlistmentManager.Globals;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace GitEnlistmentManager.Mcp.Tools
{
    public class CreatePullRequestTool : McpTool
    {
        public override string Name => "create_pull_request";

        public override string Description => "Open a pull request for an enlistment. This launches the PR creation page for the enlistment's branch.";

        public override JObject InputSchema => new JObject
        {
            ["type"] = "object",
            ["properties"] = new JObject
            {
                ["enlistmentPath"] = new JObject
                {
                    ["type"] = "string",
                    ["description"] = "Full path to the enlistment directory (e.g. 'F:\\gem\\Halo\\svc\\h5\\acrdocker\\010000.w1')"
                }
            },
            ["required"] = new JArray("enlistmentPath")
        };

        public override async Task<McpToolResult> Execute(JObject? arguments)
        {
            if (arguments == null)
            {
                return McpToolResult.Error("Arguments are required");
            }

            var enlistmentPath = arguments["enlistmentPath"]?.ToString();

            if (string.IsNullOrWhiteSpace(enlistmentPath))
            {
                return McpToolResult.Error("enlistmentPath is required");
            }

            var command = new GemCSCommand
            {
                CommandType = GemCSCommandType.InterpretCommandLine,
                CommandArgs = new object[] { "pr" },
                WorkingDirectory = enlistmentPath
            };

            await Global.Instance.MainWindow.ProcessCSCommand(command).ConfigureAwait(false);

            var result = new
            {
                message = $"Pull request opened for enlistment at '{enlistmentPath}'"
            };
            return McpToolResult.Success(JsonConvert.SerializeObject(result, Formatting.Indented));
        }
    }
}
