using Api.Chat;

namespace Api.Hubs;

public interface IChatClient
{
    Task ReceiveMessage(ChatMessage message);
    Task ReceiveHistory(IReadOnlyList<ChatMessage> messages);
    Task ReceiveStats(IReadOnlyList<MinuteCount> snapshot);
}
