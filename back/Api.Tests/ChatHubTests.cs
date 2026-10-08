using Api.Chat;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;

namespace Api.Tests;

public class ChatHubTests : IClassFixture<WebApplicationFactory<Program>>, IAsyncDisposable
{
    // One second before a minute boundary, so the ticker test only has to advance a little
    private static readonly DateTimeOffset Start = new(2026, 1, 1, 12, 0, 59, TimeSpan.Zero);
    private static readonly DateTimeOffset StartMinute = new(2026, 1, 1, 12, 0, 0, TimeSpan.Zero);
    private static readonly TimeSpan WaitTimeout = TimeSpan.FromSeconds(5);

    private readonly FakeTimeProvider _time = new(Start);
    private readonly WebApplicationFactory<Program> _app;

    public ChatHubTests(WebApplicationFactory<Program> factory)
    {
        // A fresh host per test, so history and stats never leak between tests
        _app = factory.WithWebHostBuilder(builder => builder.ConfigureTestServices(
            services => services.AddSingleton<TimeProvider>(_time)));
    }

    public ValueTask DisposeAsync() => _app.DisposeAsync();

    public static TheoryData<string, string> InvalidInputs => new()
    {
        { "", "hi" },
        { "   ", "hi" },
        { "alice", "" },
        { "alice", "   " },
        { new string('a', 33), "hi" },
        { "alice", new string('x', 501) },
    };

    [Fact]
    public async Task Client_CanConnectToHub()
    {
        await using var connection = CreateConnection();

        await connection.StartAsync(TestContext.Current.CancellationToken);

        Assert.Equal(HubConnectionState.Connected, connection.State);
    }

    [Fact]
    public async Task Connect_ReceivesEmptyHistoryAndFullStats()
    {
        await using var connection = CreateConnection();
        var history = ListenFor<List<ChatMessage>>(connection, "ReceiveHistory");
        var stats = ListenFor<List<MinuteCount>>(connection, "ReceiveStats");

        await connection.StartAsync(TestContext.Current.CancellationToken);

        Assert.Empty(await Within(history));
        var snapshot = await Within(stats);
        Assert.Equal(15, snapshot.Count);
        Assert.Equal(StartMinute, snapshot[^1].Minute);
        Assert.All(snapshot, entry => Assert.Equal(0, entry.Count));
    }

    [Fact]
    public async Task Connect_ReceivesEarlierMessagesInHistory()
    {
        var ct = TestContext.Current.CancellationToken;
        await using var first = CreateConnection();
        await first.StartAsync(ct);
        await first.InvokeAsync("SendMessage", "alice", "earlier", ct);

        await using var late = CreateConnection();
        var history = ListenFor<List<ChatMessage>>(late, "ReceiveHistory");
        await late.StartAsync(ct);

        var message = Assert.Single(await Within(history));
        Assert.Equal(new ChatMessage("alice", "earlier", Start), message);
    }

    [Fact]
    public async Task SendMessage_BroadcastsMessageToAllClients()
    {
        var ct = TestContext.Current.CancellationToken;
        await using var sender = CreateConnection();
        await using var other = CreateConnection();
        var senderReceived = ListenFor<ChatMessage>(sender, "ReceiveMessage");
        var otherReceived = ListenFor<ChatMessage>(other, "ReceiveMessage");
        await sender.StartAsync(ct);
        await other.StartAsync(ct);

        await sender.InvokeAsync("SendMessage", "alice", "hi", ct);

        var expected = new ChatMessage("alice", "hi", Start);
        Assert.Equal(expected, await Within(senderReceived));
        Assert.Equal(expected, await Within(otherReceived));
    }

    [Fact]
    public async Task SendMessage_TrimsUserAndText()
    {
        var ct = TestContext.Current.CancellationToken;
        await using var connection = CreateConnection();
        var received = ListenFor<ChatMessage>(connection, "ReceiveMessage");
        await connection.StartAsync(ct);

        await connection.InvokeAsync("SendMessage", "  alice ", " hi  ", ct);

        var message = await Within(received);
        Assert.Equal(("alice", "hi"), (message.User, message.Text));
    }

    [Fact]
    public async Task SendMessage_BroadcastsUpdatedStatsToAllClients()
    {
        var ct = TestContext.Current.CancellationToken;
        await using var sender = CreateConnection();
        await using var other = CreateConnection();
        var senderStats = ListenFor<List<MinuteCount>>(sender, "ReceiveStats", s => s[^1].Count == 1);
        var otherStats = ListenFor<List<MinuteCount>>(other, "ReceiveStats", s => s[^1].Count == 1);
        await sender.StartAsync(ct);
        await other.StartAsync(ct);

        await sender.InvokeAsync("SendMessage", "alice", "hi", ct);

        Assert.Equal(StartMinute, (await Within(senderStats))[^1].Minute);
        Assert.Equal(StartMinute, (await Within(otherStats))[^1].Minute);
    }

    [Theory]
    [MemberData(nameof(InvalidInputs))]
    public async Task SendMessage_RejectsInvalidInputWithoutBroadcasting(string user, string text)
    {
        var ct = TestContext.Current.CancellationToken;
        await using var connection = CreateConnection();
        var firstMessage = ListenFor<ChatMessage>(connection, "ReceiveMessage");
        await connection.StartAsync(ct);

        await Assert.ThrowsAsync<HubException>(() => connection.InvokeAsync("SendMessage", user, text, ct));

        // Server messages arrive in order: if the invalid one had been broadcast, it would come first
        await connection.InvokeAsync("SendMessage", "alice", "valid", ct);
        Assert.Equal("valid", (await Within(firstMessage)).Text);
    }

    [Fact]
    public async Task MinuteTicker_BroadcastsStatsAtMinuteBoundary()
    {
        await using var connection = CreateConnection();
        var initial = ListenFor<List<MinuteCount>>(connection, "ReceiveStats");
        var nextMinute = StartMinute.AddMinutes(1);
        var rolled = ListenFor<List<MinuteCount>>(connection, "ReceiveStats", s => s[^1].Minute == nextMinute);
        await connection.StartAsync(TestContext.Current.CancellationToken);
        await Within(initial);

        _time.Advance(TimeSpan.FromSeconds(1));

        var snapshot = await Within(rolled);
        Assert.Equal(15, snapshot.Count);
    }

    private HubConnection CreateConnection()
    {
        // TestServer has no real sockets: route SignalR through its in-memory handler
        var server = _app.Server;
        return new HubConnectionBuilder()
            .WithUrl(new Uri(server.BaseAddress, "hub"), options =>
            {
                options.HttpMessageHandlerFactory = _ => server.CreateHandler();
                options.Transports = HttpTransportType.LongPolling;
            })
            .Build();
    }

    // Completes with the first value received on `method` that satisfies `match`
    private static Task<T> ListenFor<T>(HubConnection connection, string method, Func<T, bool>? match = null)
    {
        var received = new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);
        connection.On<T>(method, value =>
        {
            if (match?.Invoke(value) ?? true)
                received.TrySetResult(value);
        });
        return received.Task;
    }

    private static Task<T> Within<T>(Task<T> task)
        => task.WaitAsync(WaitTimeout, TestContext.Current.CancellationToken);
}
