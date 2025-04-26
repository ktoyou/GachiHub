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
        var to = _userService.GetUserByUserName(username);
        if (to == null) return;
        if(!to.Connected) return;
        
        var from = _userService.GetUserByConnectionId(Context.ConnectionId);
        if(from == null) return;
        if(!from.Connected) return;
        
        await Clients.Client(to.ConnectionId).SendAsync("ReceiveOffer", new
        {
            Offer = offer,
            From = from,
        });
    }
    
    public async Task SendAnswer(object answer, string username)
    {
        var to = _userService.GetUserByUserName(username);
        if (to == null) return;
        if(!to.Connected) return;
        
        var from = _userService.GetUserByConnectionId(Context.ConnectionId);
        if(from == null) return;
        if(!from.Connected) return;
        
        await Clients.Client(to.ConnectionId).SendAsync("ReceiveAnswer", new
        {
            Answer = answer,
            From = from,
        });
    }
    
    public async Task SendIceCandidate(object candidate, string username)
    {
        var to = _userService.GetUserByUserName(username);
        if (to == null) return;
        if(!to.Connected) return;
        
        var from = _userService.GetUserByConnectionId(Context.ConnectionId);
        if(from == null) return;
        if(!from.Connected) return;
        
        await Clients.Client(to.ConnectionId).SendAsync("ReceiveIceCandidate", new
        {
            Candidate = candidate,
            From = from,
        });
    }

    public async Task CallUser(string username)
    {
        var to = _userService.GetUserByUserName(username);
        if (to == null) return;
        if(!to.Connected) return;
        
        var from = _userService.GetUserByConnectionId(Context.ConnectionId);
        if(from == null) return;
        if(!from.Connected) return;
        
        await Clients.Client(to.ConnectionId).SendAsync("ReceiveCall", new
        {
            From = from,
            To = to,
        });
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

    public override async Task OnConnectedAsync()
    {
        var user = new User
        {
            ConnectionId = Context.ConnectionId,
            UserName = string.Empty,
            Connected = false
        };
        
        _userService.AddUser(user);
        await base.OnConnectedAsync();
    }

    public async override Task OnDisconnectedAsync(Exception? exception)
    {
        var user = _userService.GetUserByConnectionId(Context.ConnectionId);
        if (user == null) return;
        
        _userService.RemoveUser(user);
        
        await Clients.All.SendAsync("UserDisconnected", user);
        await base.OnDisconnectedAsync(exception);
    }
}