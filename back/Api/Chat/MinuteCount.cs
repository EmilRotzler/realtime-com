namespace Api.Chat;

// Minute is the bucket start, truncated to the minute, in UTC
public record MinuteCount(DateTimeOffset Minute, int Count);
