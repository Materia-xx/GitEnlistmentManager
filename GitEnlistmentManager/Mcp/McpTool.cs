using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace GitEnlistmentManager.Mcp
{
    public abstract class McpTool
    {
        public abstract string Name { get; }

        public abstract string Description { get; }

        public abstract JObject InputSchema { get; }

        public abstract Task<McpToolResult> Execute(JObject? arguments);

        public JObject GetToolDefinition()
        {
            return new JObject
            {
                ["name"] = this.Name,
                ["description"] = this.Description,
                ["inputSchema"] = this.InputSchema
            };
        }
    }
}
