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
    
    private readonly UserService _userService;
    
    public RoomHub(RoomService roomService, UserService userService)
    {
        _roomService = roomService;
        _userService = userService;
    }

    public async Task SendAudioChunk(string roomId, string userId, byte[] audioChunk)
    {
        var room = _roomService.GetRoomById(roomId);
        if (room == null)
        {
            await Clients.Caller.SendAsync("RoomNotFound", roomId);
            return;
        }
        
        var user = _userService.GetUserByConnectionId(userId);
        if (user == null)
        {
            await Clients.Caller.SendAsync("UserNotFound", userId);
            return;
        }
        
        await Clients.All.SendAsync("ReceiveAudioChunk", userId, audioChunk);
    }

    public async Task CreateUser(string username)
    {
        var user = _userService.GetUserByUserName(username);
        if (user != null)
        {
            await Clients.Caller.SendAsync("UserAlreadyExists", username);
            return;
        }
        
        _userService.AddUser(new User()
        {
            UserName = username,
            ConnectionId = Context.ConnectionId
        });
        
        await Clients.Caller.SendAsync("CreatedUser", username, Context.ConnectionId);
    }
    
    public async Task CreateRoom(string name, string username)
    {
        var existsRoom = _roomService.GetRoomByName(name);
        if (existsRoom != null)
        {
            await Clients.Caller.SendAsync("RoomAlreadyExists", existsRoom.Name);
            return;
        }
        
        var user = _userService.GetUserByUserName(username);
        if (user == null)
        {
            await Clients.Caller.SendAsync("UserNotFound", name);
            return;
        }
        
        var room = new Room()
        {
            CreatedAt = DateTime.Now,
            Name = name,
            Id = Guid.NewGuid().ToString(),
            Users = [],
            Owner = user
        };
        
        _roomService.AddRoom(room);
        await JoinRoom(room.Id, username);
    }

    public async Task JoinRoom(string roomId, string username)
    {
        var room = _roomService.GetRoomById(roomId);
        if (room == null)
        {
            await Clients.Caller.SendAsync("RoomNotFound", roomId);
            return;
        }
        
        var user = _userService.GetUserByUserName(username);
        if (user == null)
        {
            await Clients.Caller.SendAsync("UserNotFound", username);
            return;
        }
        
        await Groups.AddToGroupAsync(Context.ConnectionId, roomId);
        await Clients.All.SendAsync("UserJoinedRoom", roomId, Context.ConnectionId);
        _roomService.AddUserToRoom(roomId, user);
    }

    public async Task LeaveRoom(string roomId, string userId)
    {
        var room = _roomService.GetRoomById(roomId);
        if (room == null)
        {
            await Clients.Caller.SendAsync("RoomNotFound", roomId);
            return;
        }
        
        var user = _userService.GetUserByConnectionId(userId);
        if (user == null)
        {
            await Clients.Caller.SendAsync("UserNotFound", userId);
            return;
        }
        
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomId);
        await Clients.All.SendAsync("UserLeftRoom", roomId);
        _roomService.RemoveUserFromRoom(roomId, user);
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