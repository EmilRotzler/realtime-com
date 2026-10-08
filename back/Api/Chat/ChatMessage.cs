namespace Api.Chat;

public record ChatMessage(string User, string Text, DateTimeOffset SentAt);
