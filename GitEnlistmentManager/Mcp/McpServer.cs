using GitEnlistmentManager.DTOs;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GitEnlistmentManager.Mcp
{
    public class McpServer
    {
        private const string ProtocolVersion = "2025-11-25";
        private readonly HttpListener listener;
        private readonly Dictionary<string, McpTool> tools = new();
        private readonly CancellationTokenSource cts = new();
        private string? sessionId;

        public McpServer(int port)
        {
            this.listener = new HttpListener();
            this.listener.Prefixes.Add($"http://127.0.0.1:{port}/");
        }

        public void RegisterTool(McpTool tool)
        {
            this.tools[tool.Name] = tool;
        }

        public IEnumerable<string> GetToolNames()
        {
            return this.tools.Keys;
        }

        public string? GetToolDescription(string toolName)
        {
            if (this.tools.TryGetValue(toolName, out var tool))
            {
                return tool.Description;
            }

            return null;
        }

        public void Start()
        {
            this.listener.Start();
            Task.Run(() => this.ListenLoop(this.cts.Token));
        }

        public void Stop()
        {
            this.cts.Cancel();
            this.listener.Stop();
        }

        private async Task ListenLoop(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var context = await this.listener.GetContextAsync().ConfigureAwait(false);
                    _ = Task.Run(() => this.HandleRequest(context), cancellationToken);
                }
                catch (HttpListenerException) when (cancellationToken.IsCancellationRequested)
                {
                    break;
                }
                catch (ObjectDisposedException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"MCP listener error: {ex}");
                }
            }
        }

        private async Task HandleRequest(HttpListenerContext context)
        {
            try
            {
                var request = context.Request;
                var response = context.Response;

                // Validate Origin header to prevent DNS rebinding
                var origin = request.Headers["Origin"];
                if (origin != null)
                {
                    // Only allow localhost origins
                    if (!origin.StartsWith("http://127.0.0.1", StringComparison.OrdinalIgnoreCase) &&
                        !origin.StartsWith("http://localhost", StringComparison.OrdinalIgnoreCase) &&
                        !origin.StartsWith("https://127.0.0.1", StringComparison.OrdinalIgnoreCase) &&
                        !origin.StartsWith("https://localhost", StringComparison.OrdinalIgnoreCase))
                    {
                        response.StatusCode = 403;
                        response.Close();
                        return;
                    }
                }

                if (request.HttpMethod == "DELETE")
                {
                    // Client terminating session
                    response.StatusCode = 202;
                    response.Close();
                    return;
                }

                if (request.HttpMethod == "GET")
                {
                    // We don't support server-initiated SSE streams
                    response.StatusCode = 405;
                    response.Close();
                    return;
                }

                if (request.HttpMethod != "POST")
                {
                    response.StatusCode = 405;
                    response.Close();
                    return;
                }

                // Read the request body
                string body;
                using (var reader = new StreamReader(request.InputStream, Encoding.UTF8))
                {
                    body = await reader.ReadToEndAsync().ConfigureAwait(false);
                }

                // Parse as JSON-RPC
                JsonRpcRequest? rpcRequest;
                try
                {
                    rpcRequest = JsonConvert.DeserializeObject<JsonRpcRequest>(body);
                }
                catch
                {
                    await WriteJsonResponse(response, JsonRpcResponse.ErrorResponse(
                        null, JsonRpcErrorCodes.ParseError, "Parse error")).ConfigureAwait(false);
                    return;
                }

                if (rpcRequest == null)
                {
                    await WriteJsonResponse(response, JsonRpcResponse.ErrorResponse(
                        null, JsonRpcErrorCodes.InvalidRequest, "Invalid request")).ConfigureAwait(false);
                    return;
                }

                // Handle notifications (no id) — accept with 202
                if (rpcRequest.IsNotification)
                {
                    response.StatusCode = 202;
                    response.Close();
                    return;
                }

                // Validate MCP-Protocol-Version on non-initialize requests
                if (rpcRequest.Method != "initialize")
                {
                    var protocolVersionHeader = request.Headers["MCP-Protocol-Version"];
                    if (protocolVersionHeader != null && protocolVersionHeader != ProtocolVersion)
                    {
                        response.StatusCode = 400;
                        await WriteJsonResponse(response, JsonRpcResponse.ErrorResponse(
                            rpcRequest.Id, JsonRpcErrorCodes.InvalidRequest,
                            $"Unsupported protocol version: {protocolVersionHeader}")).ConfigureAwait(false);
                        return;
                    }
                }

                // Dispatch the method
                var rpcResponse = await this.DispatchMethod(rpcRequest).ConfigureAwait(false);

                // Add session header if we have one
                if (this.sessionId != null)
                {
                    response.Headers["MCP-Session-Id"] = this.sessionId;
                }

                await WriteJsonResponse(response, rpcResponse).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"MCP request error: {ex}");
                try
                {
                    context.Response.StatusCode = 500;
                    context.Response.Close();
                }
                catch { }
            }
        }

        private async Task<JsonRpcResponse> DispatchMethod(JsonRpcRequest request)
        {
            switch (request.Method)
            {
                case "initialize":
                    return this.HandleInitialize(request);

                case "tools/list":
                    return this.HandleToolsList(request);

                case "tools/call":
                    return await this.HandleToolsCall(request).ConfigureAwait(false);

                case "ping":
                    return JsonRpcResponse.Success(request.Id, new JObject());

                default:
                    return JsonRpcResponse.ErrorResponse(
                        request.Id, JsonRpcErrorCodes.MethodNotFound,
                        $"Method not found: {request.Method}");
            }
        }

        private JsonRpcResponse HandleInitialize(JsonRpcRequest request)
        {
            this.sessionId = Guid.NewGuid().ToString();

            var result = new JObject
            {
                ["protocolVersion"] = ProtocolVersion,
                ["capabilities"] = new JObject
                {
                    ["tools"] = new JObject
                    {
                        ["listChanged"] = false
                    }
                },
                ["serverInfo"] = new JObject
                {
                    ["name"] = "GitEnlistmentManager",
                    ["version"] = "1.0.0"
                },
                ["instructions"] = "GEM (Git Enlistment Manager) manages git enlistments. Use tools to list, create, and manage the different aspects of these resources."
            };

            return JsonRpcResponse.Success(request.Id, result);
        }

        private JsonRpcResponse HandleToolsList(JsonRpcRequest request)
        {
            var toolsArray = new JArray();
            foreach (var tool in this.tools.Values)
            {
                toolsArray.Add(tool.GetToolDefinition());
            }

            var result = new JObject
            {
                ["tools"] = toolsArray
            };

            return JsonRpcResponse.Success(request.Id, result);
        }

        private async Task<JsonRpcResponse> HandleToolsCall(JsonRpcRequest request)
        {
            var toolName = request.Params?["name"]?.ToString();
            if (string.IsNullOrWhiteSpace(toolName))
            {
                return JsonRpcResponse.ErrorResponse(
                    request.Id, JsonRpcErrorCodes.InvalidParams, "Missing tool name");
            }

            if (!this.tools.TryGetValue(toolName, out var tool))
            {
                return JsonRpcResponse.ErrorResponse(
                    request.Id, JsonRpcErrorCodes.InvalidParams,
                    $"Unknown tool: {toolName}");
            }

            if (Gem.Instance.LocalAppData.DisabledMcpTools.Contains(toolName))
            {
                return JsonRpcResponse.Success(request.Id,
                    McpToolResult.Error($"Tool '{toolName}' is currently disabled in GEM settings.").ToJson());
            }

            var arguments = request.Params?["arguments"] as JObject;

            try
            {
                var result = await tool.Execute(arguments).ConfigureAwait(false);
                return JsonRpcResponse.Success(request.Id, result.ToJson());
            }
            catch (Exception ex)
            {
                return JsonRpcResponse.Success(request.Id, McpToolResult.Error($"Tool execution failed: {ex.Message}").ToJson());
            }
        }

        private static async Task WriteJsonResponse(HttpListenerResponse response, JsonRpcResponse rpcResponse)
        {
            response.ContentType = "application/json";
            response.StatusCode = 200;
            var json = JsonConvert.SerializeObject(rpcResponse);
            var bytes = Encoding.UTF8.GetBytes(json);
            response.ContentLength64 = bytes.Length;
            await response.OutputStream.WriteAsync(bytes, 0, bytes.Length).ConfigureAwait(false);
            response.Close();
        }
    }
}
