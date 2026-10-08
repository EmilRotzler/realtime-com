using Api.Chat;

namespace Api.Tests;

public class MessageHistoryTests
{
    private readonly MessageHistory _history = new();

    [Fact]
    public void GetRecent_ReturnsMessagesOldestFirst()
    {
        _history.Add(Message("first"));
        _history.Add(Message("second"));

        Assert.Equal(["first", "second"], _history.GetRecent().Select(m => m.Text));
    }

    [Fact]
    public void Add_KeepsOnlyTheFiftyMostRecent()
    {
        for (var i = 0; i < 60; i++)
            _history.Add(Message(i.ToString()));

        var recent = _history.GetRecent();
        Assert.Equal(50, recent.Count);
        Assert.Equal("10", recent[0].Text);
        Assert.Equal("59", recent[^1].Text);
    }

    private static ChatMessage Message(string text) => new("alice", text, DateTimeOffset.UnixEpoch);
}
