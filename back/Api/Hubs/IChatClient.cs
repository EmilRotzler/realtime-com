namespace Api.Hubs;

public interface IChatClient
{
    Task ReceiveMessage(string user, string message);
}
