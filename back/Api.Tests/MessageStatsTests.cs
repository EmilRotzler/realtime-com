using Api.Chat;
using Microsoft.Extensions.Time.Testing;

namespace Api.Tests;

public class MessageStatsTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 12, 7, 42, TimeSpan.Zero);
    private static readonly DateTimeOffset CurrentMinute = new(2026, 1, 1, 12, 7, 0, TimeSpan.Zero);

    private readonly FakeTimeProvider _time = new(Now);
    private readonly MessageStats _stats;

    public MessageStatsTests() => _stats = new MessageStats(_time);

    [Fact]
    public void Snapshot_HasFifteenEmptyMinutesEndingWithCurrentMinute()
    {
        var snapshot = _stats.GetSnapshot();

        Assert.Equal(15, snapshot.Count);
        Assert.Equal(CurrentMinute.AddMinutes(-14), snapshot[0].Minute);
        Assert.Equal(CurrentMinute, snapshot[^1].Minute);
        Assert.All(snapshot, entry => Assert.Equal(0, entry.Count));
        for (var i = 1; i < snapshot.Count; i++)
            Assert.Equal(snapshot[i - 1].Minute.AddMinutes(1), snapshot[i].Minute);
    }

    [Fact]
    public void AddMessage_CountsInCurrentMinute()
    {
        _stats.AddMessage();
        _stats.AddMessage();

        Assert.Equal(2, _stats.GetSnapshot()[^1].Count);
    }

    [Fact]
    public void AddMessage_SeparatesMinutes()
    {
        _stats.AddMessage();
        _time.Advance(TimeSpan.FromMinutes(1));
        _stats.AddMessage();
        _stats.AddMessage();
        _stats.AddMessage();

        var snapshot = _stats.GetSnapshot();
        Assert.Equal(1, snapshot[^2].Count);
        Assert.Equal(3, snapshot[^1].Count);
    }

    [Fact]
    public void Snapshot_ZeroFillsGaps()
    {
        _stats.AddMessage();
        _time.Advance(TimeSpan.FromMinutes(3));
        _stats.AddMessage();

        var counts = _stats.GetSnapshot().Select(entry => entry.Count).TakeLast(4);
        Assert.Equal([1, 0, 0, 1], counts);
    }

    [Fact]
    public void Message_StaysInWindowForFifteenMinutesThenFallsOut()
    {
        _stats.AddMessage();

        _time.Advance(TimeSpan.FromMinutes(14));
        Assert.Equal(1, _stats.GetSnapshot()[0].Count);

        _time.Advance(TimeSpan.FromMinutes(1));
        Assert.All(_stats.GetSnapshot(), entry => Assert.Equal(0, entry.Count));
    }
}
