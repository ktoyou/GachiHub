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
        var existsRoom = _roomService.GetRoomByName(name);
        if (existsRoom != null)
        {
            await Clients.Caller.SendAsync("RoomAlreadyExists", existsRoom.Name);
            return;
        }
        
        var room = new Room()
        {
            CreatedAt = DateTime.Now,
            Name = name,
            Id = Guid.NewGuid().ToString(),
            Users = []
        };
        
        _roomService.AddRoom(room);
    }

    public async Task JoinRoom(string roomId, string username)
    {
        var room = _roomService.GetRoomById(roomId);
        if (room == null)
        {
            await Clients.Caller.SendAsync("RoomNotFound", roomId);
            return;
        }
        
        await Groups.AddToGroupAsync(Context.ConnectionId, roomId);
        await Clients.All.SendAsync("UserJoinedRoom", roomId, Context.ConnectionId);
        _roomService.AddUserToRoom(roomId, Context.ConnectionId, username);
    }

    public async Task LeaveRoom(string roomId, string userId)
    {
        var room = _roomService.GetRoomById(roomId);
        if (room == null)
        {
            await Clients.Caller.SendAsync("RoomNotFound", roomId);
            return;
        }
        
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomId);
        await Clients.All.SendAsync("UserLeftRoom", roomId);
        _roomService.RemoveUserFromRoom(roomId, userId);
    }

    public async Task SendMessageToRoom(string roomId, string userId, string message)
    {
        var room = _roomService.GetRoomById(roomId);
        if (room == null)
        {
            await Clients.Caller.SendAsync("RoomNotFound", roomId);
            return;
        }

        var user = room!.Users.FirstOrDefault(u => u.ConnectionId == userId);
        if (user != null)
        {
            await Clients.Others.SendAsync("ReceiveMessage", userId, message);   
        }
        else
        {
            await Clients.Caller.SendAsync("UserIsNotInRoom", userId, roomId);
        }
    }
}