using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using GachiHubBackend.Models;
using GachiHubBackend.Services;
using SignalRSwaggerGen.Attributes;

namespace GachiHubBackend.Hubs;

[SignalRHub]
public class RoomHub : Hub
{
    private readonly RoomService _roomService;
    
    public RoomHub(RoomService roomService)
    {
        _roomService = roomService;
    }

    public async Task CreateRoom(string name)
    {
        var room = new Room()
        {
            CreatedAt = DateTime.Now,
            Name = name,
            Id = Guid.NewGuid().ToString(),
            UserIds = []
        };
        
        _roomService.AddRoom(room);
        await JoinRoom(room.Id);
    }

    public async Task JoinRoom(string roomId)
    {
        var room = _roomService.GetRoomById(roomId);
        if (room == null)
        {
            await Clients.Caller.SendAsync("RoomNotFound", roomId);
        }
        
        await Groups.AddToGroupAsync(Context.ConnectionId, roomId);
        await Clients.Others.SendAsync("UserJoinedRoom", roomId);
        _roomService.AddUserToRoom(roomId, Context.ConnectionId);
    }

    public async Task LeaveRoom(string roomId, string userId)
    {
        var room = _roomService.GetRoomById(roomId);
        if (room == null)
        {
            await Clients.Caller.SendAsync("RoomNotFound", roomId);
        }
        
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomId);
        await Clients.Others.SendAsync("UserLeftRoom", roomId);
        _roomService.RemoveUserFromRoom(roomId, userId);
    }

    public async Task SendMessageToRoom(string roomId, string clientId, string message)
    {
        var room = _roomService.GetRoomById(roomId);
        if (room == null)
        {
            await Clients.Caller.SendAsync("RoomNotFound", roomId);
        }

        if (room!.UserIds.Contains(clientId))
        {
            await Clients.Others.SendAsync("ReceiveMessage", clientId, message);   
        }
        else
        {
            await Clients.Caller.SendAsync("UserIsNotInRoom", clientId, roomId);
        }
    }
}