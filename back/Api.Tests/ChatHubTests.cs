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
        await using var connection = CreateConnection();

        await connection.StartAsync(TestContext.Current.CancellationToken);

        Assert.Equal(HubConnectionState.Connected, connection.State);
    }

    [Fact]
    public async Task SendMessage_BroadcastsToAllClients()
    {
        var ct = TestContext.Current.CancellationToken;
        await using var sender = CreateConnection();
        await using var other = CreateConnection();
        var senderReceived = ListenForMessage(sender);
        var otherReceived = ListenForMessage(other);
        await sender.StartAsync(ct);
        await other.StartAsync(ct);

        await sender.InvokeAsync("SendMessage", "alice", "hi", ct);

        var timeout = TimeSpan.FromSeconds(5);
        Assert.Equal(("alice", "hi"), await senderReceived.WaitAsync(timeout, ct));
        Assert.Equal(("alice", "hi"), await otherReceived.WaitAsync(timeout, ct));
    }

    private HubConnection CreateConnection()
    {
        // TestServer has no real sockets: route SignalR through its in-memory handler
        var server = factory.Server;
        return new HubConnectionBuilder()
            .WithUrl(new Uri(server.BaseAddress, "hub"), options =>
            {
                options.HttpMessageHandlerFactory = _ => server.CreateHandler();
                options.Transports = HttpTransportType.LongPolling;
            })
            .Build();
    }

    private static Task<(string User, string Message)> ListenForMessage(HubConnection connection)
    {
        var received = new TaskCompletionSource<(string User, string Message)>(
            TaskCreationOptions.RunContinuationsAsynchronously);
        connection.On<string, string>("ReceiveMessage",
            (user, message) => received.TrySetResult((user, message)));
        return received.Task;
    }
}
