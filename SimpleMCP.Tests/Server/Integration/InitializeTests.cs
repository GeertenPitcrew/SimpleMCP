using FluentAssertions;
using SimpleMCP.Server.Models;

namespace SimpleMCP.Tests.Server.Integration;

public class InitializeTests(TestFixture fixture) : McpServerTestBase(fixture)
{
    [Fact]
    public async Task Initialize_ShouldReturnSuccessResponse()
    {
        var request = new JsonRpcRequest
        {
            Id = "1",
            Method = "initialize",
            Params = new { }
        };

        var response = await InvokeServerMethod(request);

        response.Should().NotBeNull();
        response.Id.Should().Be("1");
        response.Error.Should().BeNull();
        response.Result.Should().NotBeNull();
    }
}
