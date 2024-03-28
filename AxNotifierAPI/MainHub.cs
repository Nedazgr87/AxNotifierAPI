using Microsoft.AspNetCore.SignalR;

namespace AxNotifierAPI
{
    public class MainHub : Hub
    {
        public async Task SendMessageToAll(string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", message);
        }

        public string GetConnectionId()
        {
            return Context.ConnectionId;
        }
    }
}
