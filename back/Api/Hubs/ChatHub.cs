using Microsoft.AspNetCore.SignalR;

namespace Api.Hubs;

public class ChatHub : Hub<IChatClient>
{
    public Task SendMessage(string user, string message)
        => Clients.All.ReceiveMessage(user, message);
}
