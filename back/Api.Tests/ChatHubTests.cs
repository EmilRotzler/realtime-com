using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.SignalR.Client;

namespace Api.Tests;

public class ChatHubTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    [Fact]
    public async Task Client_CanConnectToHub()
    {
        // TestServer has no real sockets: route SignalR through its in-memory handler
        var server = factory.Server;
        await using var connection = new HubConnectionBuilder()
            .WithUrl(new Uri(server.BaseAddress, "hub"), options =>
            {
                options.HttpMessageHandlerFactory = _ => server.CreateHandler();
                options.Transports = HttpTransportType.LongPolling;
            })
            .Build();

        await connection.StartAsync(TestContext.Current.CancellationToken);

        Assert.Equal(HubConnectionState.Connected, connection.State);
    }
}
