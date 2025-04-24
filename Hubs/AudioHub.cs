using Microsoft.AspNetCore.SignalR;

namespace GachiHubBackend.Hubs;

public class AudioHub : Hub
{
    public async Task SendAudioChunk(byte[] audioChunk)
        => await Clients.Others.SendAsync("ReceiveAudioChunk", audioChunk);

    public async Task JoinAudioRoom(Guid roomId)
        => await Groups.AddToGroupAsync(Context.ConnectionId, roomId.ToString());
    
    public async Task LeaveAudioRoom(Guid roomId) 
        => await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomId.ToString());

    public async Task SendAudioToRoom(Guid roomId, byte[] audioChunk)
        => await Clients.Others.SendAsync("ReceiveAudioToRoom", roomId, audioChunk);

    public override async Task OnConnectedAsync()
    {
        await Clients.All.SendAsync("UserConnected", Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await Clients.All.SendAsync("UserDisconnected", Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }
}