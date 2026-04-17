using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GitEnlistmentManager.Mcp
{
    public class JsonRpcRequest
    {
        [JsonProperty("jsonrpc")]
        public string JsonRpc { get; set; } = "2.0";

        [JsonProperty("id")]
        public object? Id { get; set; }

        [JsonProperty("method")]
        public string Method { get; set; } = string.Empty;

        [JsonProperty("params")]
        public JObject? Params { get; set; }

        public bool IsNotification => this.Id == null;
    }
}
