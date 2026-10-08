namespace Api.Chat;

public class MessageStats(TimeProvider time)
{
    public const int WindowMinutes = 15;

    private readonly Dictionary<DateTimeOffset, int> _counts = [];
    private readonly Lock _lock = new();

    public void AddMessage()
    {
        var current = CurrentMinute();
        var oldest = current.AddMinutes(-(WindowMinutes - 1));
        lock (_lock)
        {
            _counts[current] = _counts.GetValueOrDefault(current) + 1;
            foreach (var minute in _counts.Keys.Where(minute => minute < oldest).ToList())
                _counts.Remove(minute);
        }
    }

    // Exactly WindowMinutes entries, oldest first, ending with the current minute
    public IReadOnlyList<MinuteCount> GetSnapshot()
    {
        var current = CurrentMinute();
        lock (_lock)
        {
            return Enumerable.Range(0, WindowMinutes)
                .Select(i => current.AddMinutes(i - (WindowMinutes - 1)))
                .Select(minute => new MinuteCount(minute, _counts.GetValueOrDefault(minute)))
                .ToList();
        }
    }

    public static DateTimeOffset TruncateToMinute(DateTimeOffset time)
    {
        var utc = time.ToUniversalTime();
        return new DateTimeOffset(utc.Year, utc.Month, utc.Day, utc.Hour, utc.Minute, 0, TimeSpan.Zero);
    }

    private DateTimeOffset CurrentMinute() => TruncateToMinute(time.GetUtcNow());
}
