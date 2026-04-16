using Newtonsoft.Json;

namespace GitEnlistmentManager.Mcp
{
    public class JsonRpcError
    {
        [JsonProperty("code")]
        public int Code { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; } = string.Empty;

        [JsonProperty("data", NullValueHandling = NullValueHandling.Ignore)]
        public object? Data { get; set; }
    }
}
