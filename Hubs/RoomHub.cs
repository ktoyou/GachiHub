using Microsoft.AspNetCore.SignalR;
using GachiHubBackend.Models;
using GachiHubBackend.Services;
using SignalRSwaggerGen.Attributes;

namespace GachiHubBackend.Hubs;

[SignalRHub]
public class RoomHub : Hub
{
    private readonly UserService _userService;

    public RoomHub(UserService userService)
    {
        _userService = userService;
    }
    
    public async Task SendOffer(object offer, string username)
    {
        await Clients.Others.SendAsync("ReceiveOffer", offer);
    }
    
    public async Task SendAnswer(object answer, string username)
    {
        await Clients.Others.SendAsync("ReceiveAnswer", answer);
    }
    
    public async Task SendIceCandidate(object candidate, string username)
    {
        await Clients.Others.SendAsync("ReceiveIceCandidate", candidate);
    }

    public async Task CallUser(string username)
    {
        var user = _userService.GetUserByUserName(username);
        if (user == null) return;
        
        if(!user.Connected) return;
        
        await Clients.Others.SendAsync("ReceiveCall", user);
    }

    public async Task CreateUser(string username)
    {
        var user = _userService.GetUserByUserName(username);
        if(user != null) return;
        
        if(string.IsNullOrEmpty(username)) return;
        
        var currentUser = _userService.GetUserByConnectionId(Context.ConnectionId);
        if(currentUser == null) return;
        
        currentUser.UserName = username;
        currentUser.Connected = true;
        
        await Clients.Others.SendAsync("CreatedUser", currentUser);
    }

    public async Task GetAllUsers()
    {
        await Clients.Caller.SendAsync("Users", _userService.GetUsers());
    }

    public override Task OnConnectedAsync()
    {
        var user = new User
        {
            ConnectionId = Context.ConnectionId,
            UserName = string.Empty,
            Connected = false
        };
        
        _userService.AddUser(user);
        return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        var user = _userService.GetUserByConnectionId(Context.ConnectionId);
        if(user == null) return Task.CompletedTask;
        
        _userService.RemoveUser(user);
        
        return base.OnDisconnectedAsync(exception);
    }
}