using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace GitEnlistmentManager.Mcp
{
    public class McpToolResult
    {
        public List<McpContentItem> Content { get; set; } = new();
        public bool IsError { get; set; }

        public static McpToolResult Success(string text)
        {
            return new McpToolResult
            {
                Content = new List<McpContentItem>
                {
                    new McpContentItem { Type = "text", Text = text }
                }
            };
        }

        public static McpToolResult Error(string text)
        {
            return new McpToolResult
            {
                Content = new List<McpContentItem>
                {
                    new McpContentItem { Type = "text", Text = text }
                },
                IsError = true
            };
        }

        public JObject ToJson()
        {
            var contentArray = new JArray();
            foreach (var item in this.Content)
            {
                contentArray.Add(new JObject
                {
                    ["type"] = item.Type,
                    ["text"] = item.Text
                });
            }

            return new JObject
            {
                ["content"] = contentArray,
                ["isError"] = this.IsError
            };
        }
    }
}
