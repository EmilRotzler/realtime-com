namespace Api.Chat;

public class MessageHistory
{
    public const int Capacity = 50;

    private readonly Queue<ChatMessage> _messages = new();
    private readonly Lock _lock = new();

    public void Add(ChatMessage message)
    {
        lock (_lock)
        {
            _messages.Enqueue(message);
            if (_messages.Count > Capacity)
                _messages.Dequeue();
        }
    }

    // Oldest first
    public IReadOnlyList<ChatMessage> GetRecent()
    {
        lock (_lock)
            return _messages.ToList();
    }
}
