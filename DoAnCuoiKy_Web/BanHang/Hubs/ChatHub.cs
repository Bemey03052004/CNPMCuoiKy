using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

public class ChatHub : Hub
{
    public async Task SendMessage(string receiverId, string message)
    {
        var senderId = Context.UserIdentifier;
        if (!string.IsNullOrEmpty(senderId))
        {
            await Clients.User(receiverId).SendAsync("ReceiveMessage", senderId, message);
            await Clients.User(senderId).SendAsync("ReceiveMessage", senderId, message); // Gửi cho chính mình để hiển thị luôn
        }
    }
}
