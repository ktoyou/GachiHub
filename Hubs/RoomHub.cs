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

    // Вспомогательные методы для проверок
    private async Task<Room?> GetRoomOrNotify(string roomId)
    {
        var room = _roomService.GetRoomById(roomId);
        if (room == null)
        {
            await Clients.Caller.SendAsync("RoomNotFound", roomId);
        }
        return room;
    }

    private async Task<User?> GetUserOrNotify(string username)
    {
        var user = _userService.GetUserByUserName(username);
        if (user == null)
        {
            await Clients.Caller.SendAsync("UserNotFound", username);
        }
        return user;
    }

    private async Task<User?> GetUserByConnectionIdOrNotify(string userId)
    {
        var user = _userService.GetUserByConnectionId(userId);
        if (user == null)
        {
            await Clients.Caller.SendAsync("UserNotFound", userId);
        }
        return user;
    }

    // Основные методы хаба
    public async Task SendAudioChunk(string roomId, string userId, byte[] audioChunk)
    {
        var room = await GetRoomOrNotify(roomId);
        if (room == null) return;
        
        var user = await GetUserByConnectionIdOrNotify(userId);
        if (user == null) return;
        
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
        
        var newUser = new User
        {
            UserName = username,
            ConnectionId = Context.ConnectionId
        };
        
        _userService.AddUser(newUser);
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
        
        var user = await GetUserOrNotify(username);
        if (user == null) return;
        
        var room = new Room
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
        var room = await GetRoomOrNotify(roomId);
        if (room == null) return;
        
        var user = await GetUserOrNotify(username);
        if (user == null) return;
        
        await Groups.AddToGroupAsync(Context.ConnectionId, roomId);
        await Clients.All.SendAsync("UserJoinedRoom", roomId, Context.ConnectionId);
        _roomService.AddUserToRoom(roomId, user);
    }

    public async Task LeaveRoom(string roomId, string userId)
    {
        var room = await GetRoomOrNotify(roomId);
        if (room == null) return;
        
        var user = await GetUserByConnectionIdOrNotify(userId);
        if (user == null) return;
        
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomId);
        await Clients.All.SendAsync("UserLeftRoom", roomId);
        _roomService.RemoveUserFromRoom(roomId, user);
    }

    public async Task SendMessageToRoom(string roomId, string userId, string message)
    {
        var room = await GetRoomOrNotify(roomId);
        if (room == null) return;

        var user = room.Users.FirstOrDefault(u => u.ConnectionId == userId);
        if (user != null)
        {
            await Clients.Others.SendAsync("ReceiveMessage", userId, message);   
        }
        else
        {
            await Clients.Caller.SendAsync("UserIsNotInRoom", userId, roomId);
        }
    }
    
    // Добавляем обработку подключения/отключения
    public override async Task OnConnectedAsync()
    {
        await Clients.All.SendAsync("UserConnected", Context.ConnectionId);
        await base.OnConnectedAsync();
    }
    
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        // Находим все комнаты, в которых есть пользователь
        var user = _userService.GetUserByConnectionId(Context.ConnectionId);
        if (user != null)
        {
            foreach (var room in _roomService.GetRooms())
            {
                if (room.Users.Any(u => u.ConnectionId == Context.ConnectionId))
                {
                    await Groups.RemoveFromGroupAsync(Context.ConnectionId, room.Id);
                    _roomService.RemoveUserFromRoom(room.Id, user);
                    await Clients.All.SendAsync("UserLeftRoom", room.Id);
                }
            }
        }
        
        await Clients.All.SendAsync("UserDisconnected", Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }
}