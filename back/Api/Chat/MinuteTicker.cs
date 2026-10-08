using Api.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace Api.Chat;

// Moves every client's chart window forward on each minute boundary, even when nobody is typing
public class MinuteTicker(IHubContext<ChatHub, IChatClient> hub, MessageStats stats, TimeProvider time)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var now = time.GetUtcNow();
            var nextMinute = MessageStats.TruncateToMinute(now).AddMinutes(1);
            await Task.Delay(nextMinute - now, time, stoppingToken);
            await hub.Clients.All.ReceiveStats(stats.GetSnapshot());
        }
    }
}
