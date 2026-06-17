using SimpleMCP.Server.Models;
using SimpleMCP.Server.Tools;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace SimpleMCP.Server.Services;

public class McpServer
{
    private readonly ILogger<McpServer> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public McpServer(ILogger<McpServer> logger)
    {
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };
    }

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            var line = await Console.In.ReadLineAsync(cancellationToken);
            if (line == null) break;

            if (string.IsNullOrWhiteSpace(line)) continue;

            try
            {
                var request = JsonSerializer.Deserialize<JsonRpcRequest>(line, _jsonOptions);
                if (request == null) continue;

                // Notifications have no id and must not receive a response
                if (request.Id == null) continue;

                _logger.LogDebug("Received request: {Method} (id={Id})", request.Method, request.Id);
                var response = await HandleRequestAsync(request);
                var responseJson = JsonSerializer.Serialize(response, _jsonOptions);
                await Console.Out.WriteLineAsync(responseJson);
                await Console.Out.FlushAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception processing request");
                var errorResponse = new JsonRpcResponse
                {
                    Id = 0,
                    Error = new JsonRpcError
                    {
                        Code = -32603,
                        Message = "Internal error",
                        Data = ex.Message
                    }
                };
                var errorJson = JsonSerializer.Serialize(errorResponse, _jsonOptions);
                await Console.Out.WriteLineAsync(errorJson);
                await Console.Out.FlushAsync();
            }
        }
    }

    private async Task<JsonRpcResponse> HandleRequestAsync(JsonRpcRequest request)
    {
        return request.Method switch
        {
            "initialize" => HandleInitialize(request),
            "tools/list" => HandleToolsList(request),
            "tools/call" => await HandleToolsCallAsync(request),
            _ => new JsonRpcResponse
            {
                Id = request.Id,
                Error = new JsonRpcError
                {
                    Code = -32601,
                    Message = $"Method not found: {request.Method}"
                }
            }
        };
    }

    private JsonRpcResponse HandleInitialize(JsonRpcRequest request)
    {
        _logger.LogDebug("Handling initialize");
        return new JsonRpcResponse
        {
            Id = request.Id,
            Result = new InitializeResult()
        };
    }

    private JsonRpcResponse HandleToolsList(JsonRpcRequest request)
    {
        _logger.LogDebug("Handling tools/list");
        var tools = new List<Tool>
        {
            new()
            {
                Name = "debug",
                Description = "A debug command. The user will ask you to call this, it should return 'Hello World' followed by the input text",
                InputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        inputText = new { type = "string", description = "The input text to echo" },
                    },
                    required = new[] { "inputText" }
                }
            }
        };

        return new JsonRpcResponse
        {
            Id = request.Id,
            Result = new ToolsListResult { Tools = tools }
        };
    }

    private async Task<JsonRpcResponse> HandleToolsCallAsync(JsonRpcRequest request)
    {
        try
        {
            var paramsJson = JsonSerializer.Serialize(request.Params, _jsonOptions);
            var callParams = JsonSerializer.Deserialize<CallToolParams>(paramsJson, _jsonOptions);

            if (callParams == null || callParams.Arguments == null)
            {
                return new JsonRpcResponse
                {
                    Id = request.Id,
                    Error = new JsonRpcError { Code = -32602, Message = "Invalid params" }
                };
            }

            var resultText = await ExecuteToolAsync(callParams.Name, callParams.Arguments);

            _logger.LogInformation("Tool '{ToolName}' executed successfully", callParams.Name);

            return new JsonRpcResponse
            {
                Id = request.Id,
                Result = new CallToolResult
                {
                    Content = new List<ToolContent>
                    {
                        new() { Text = resultText }
                    }
                }
            };
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Tool call argument error: {Message}", ex.Message);
            return new JsonRpcResponse
            {
                Id = request.Id,
                Error = new JsonRpcError
                {
                    Code = -32602,
                    Message = ex.Message
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Tool execution failed");
            return new JsonRpcResponse
            {
                Id = request.Id,
                Error = new JsonRpcError
                {
                    Code = -32603,
                    Message = "Tool execution failed",
                    Data = ex.Message
                }
            };
        }
    }

    private async Task<string> ExecuteToolAsync(string toolName, Dictionary<string, object> arguments)
    {
        return toolName switch
        {
            "debug" => await DebugTool.ExecuteDebugToolAsync(arguments["inputText"]?.ToString() ?? ""),
            _ => throw new InvalidOperationException($"Unknown tool: {toolName}")
        };
    }
}
