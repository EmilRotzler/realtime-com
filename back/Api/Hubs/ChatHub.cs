using Api.Chat;
using Microsoft.AspNetCore.SignalR;

namespace Api.Hubs;

public class ChatHub(MessageHistory history, MessageStats stats, TimeProvider time) : Hub<IChatClient>
{
    public const int MaxUserLength = 32;
    public const int MaxTextLength = 500;

    public override async Task OnConnectedAsync()
    {
        await Clients.Caller.ReceiveHistory(history.GetRecent());
        await Clients.Caller.ReceiveStats(stats.GetSnapshot());
        await base.OnConnectedAsync();
    }

    public async Task SendMessage(string? user, string? text)
    {
        user = (user ?? "").Trim();
        text = (text ?? "").Trim();
        if (user.Length == 0)
            throw new HubException("Name is required.");
        if (user.Length > MaxUserLength)
            throw new HubException($"Name can be at most {MaxUserLength} characters.");
        if (text.Length == 0)
            throw new HubException("Message is empty.");
        if (text.Length > MaxTextLength)
            throw new HubException($"Message can be at most {MaxTextLength} characters.");

        var message = new ChatMessage(user, text, time.GetUtcNow());
        history.Add(message);
        stats.AddMessage();

        await Clients.All.ReceiveMessage(message);
        await Clients.All.ReceiveStats(stats.GetSnapshot());
    }
}
