using Microsoft.AspNetCore.SignalR;

namespace WebApplication1.Hub;

public class ChatHub(ILogger<ChatHub> logger) : Microsoft.AspNetCore.SignalR.Hub
{
    public override Task OnConnectedAsync()
    {
        logger.LogInformation("新客户端连接:{0}", Context.ConnectionId);
        return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        logger.LogInformation("客户端断开:{0}", Context.ConnectionId);
        return base.OnDisconnectedAsync(exception);
    }

    [HubMethodName("SendMessage")]
    public async Task SendMessage(string user, string content)
    {
        await Clients.Others.SendAsync("ReceiveMessage", user, content);
    }
}