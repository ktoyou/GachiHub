using System.Collections.Concurrent;
using GachiHubBackend.Models;

namespace GachiHubBackend.Services;

public class UserService
{
    private readonly ConcurrentBag<User> _users;

    public UserService()
    {
        _users = new ConcurrentBag<User>();
    }
    
    public User? GetUserByConnectionId(string connectionId) 
        => _users.FirstOrDefault(x => x.ConnectionId == connectionId);
    
    public User? GetUserByUserName(string userName) 
        => _users.FirstOrDefault(x => x.UserName == userName);
    
    public void AddUser(User user) => _users.Add(user);
}