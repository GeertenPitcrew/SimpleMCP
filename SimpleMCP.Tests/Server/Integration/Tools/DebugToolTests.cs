using FluentAssertions;
using SimpleMCP.Server.Models;
using System.Text.Json;

namespace SimpleMCP.Tests.Server.Integration.Tools;

public class DebugToolTests(TestFixture fixture) : McpServerTestBase(fixture)
{
    [Fact]
    public async Task Debug_ShouldReturnHelloWorldWithInput()
    {
        var request = new JsonRpcRequest
        {
            Id = "3",
            Method = "tools/call",
            Params = new { name = "debug", arguments = new { inputText = "test" } }
        };

        var response = await InvokeServerMethod(request);

        response.Should().NotBeNull();
        response.Id.Should().Be("3");
        response.Error.Should().BeNull();

        var result = JsonSerializer.Deserialize<CallToolResult>(
            JsonSerializer.Serialize(response.Result, _jsonOptions),
            _jsonOptions);

        result.Should().NotBeNull();
        result.Content.Should().HaveCount(1);
        result.Content[0].Text.Should().Be("Hello World test");
    }
}
