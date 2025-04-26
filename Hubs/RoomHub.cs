using System.Collections.Concurrent;
using Microsoft.AspNetCore.SignalR;
using GachiHubBackend.Models;
using GachiHubBackend.Services;
using SignalRSwaggerGen.Attributes;

namespace GachiHubBackend.Hubs;

[SignalRHub]
public class RoomHub : Hub
{
    private readonly UserService _userService;
    
    private readonly CallService _callService;

    public RoomHub(UserService userService, CallService callService)
    {
        _userService = userService;
        _callService = callService;
    }
    
    public async Task SendOffer(object offer, string callId)
    {
        var call = _callService.GetCallById(callId);
        if(call == null) return;
        
        await Clients.Client(call.From.ConnectionId).SendAsync("ReceiveOffer", new
        {
            Offer = offer,
            Call = call
        });
    }
    
    public async Task SendAnswer(object answer, string callId)
    {
        var call = _callService.GetCallById(callId);
        if(call == null) return;
        
        await Clients.Client(call.From.ConnectionId).SendAsync("ReceiveAnswer", new
        {
            Answer = answer,
            Call = call
        });
    }
    
    public async Task SendIceCandidate(object candidate, string callId)
    {
        var call = _callService.GetCallById(callId);
        if(call == null) return;
        
        await Clients.Client(call.From.ConnectionId).SendAsync("ReceiveIceCandidate", new
        {
            Candidate = candidate,
            Call = call
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

        var call = new Call()
        {
            From = from,
            To = to,
            CallId = Guid.NewGuid().ToString()
        };
        _callService.AddCall(call);
        
        await Clients.Client(to.ConnectionId).SendAsync("CallingUser", call);
    }

    public async Task AcceptCall(string callId)
    {
        var call = _callService.GetCallById(callId);
        if(call == null) return;
        
        await Clients.Client(call.From.ConnectionId).SendAsync("AcceptedCall", call);
    }

    public async Task DeclineCall(string callId)
    {
        var call = _callService.GetCallById(callId);
        if(call == null) return;
        
        _callService.RemoveCall(call);
        
        await Clients.Client(call.From.ConnectionId).SendAsync("DeclinedCall", call);
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

        var call = _callService.GetCallByUserConnectionId(Context.ConnectionId);
        if (call != null)
        {
            _callService.RemoveCall(call);
            await Clients.Client(call.To.ConnectionId).SendAsync("DeclinedCall", call);
            await Clients.Client(call.From.ConnectionId).SendAsync("DeclinedCall", call);
        }
        
        _userService.RemoveUser(user);
        
        await Clients.All.SendAsync("UserDisconnected", user);
        await base.OnDisconnectedAsync(exception);
    }
}