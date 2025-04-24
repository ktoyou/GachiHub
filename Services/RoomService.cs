using System.Collections.Concurrent;
using GachiHubBackend.Models;

namespace GachiHubBackend.Services;

public class RoomService
{
    private readonly ConcurrentBag<Room> _rooms;

    public RoomService()
    {
        _rooms = new ConcurrentBag<Room>();
    }

    public Room CreateRoom(string name, bool hasAudio = true, bool hasMessage = true)
    {
        var room = new Room()
        {
            Name = name,
        };
        
        _rooms.Add(room);
        return room;
    }

    public bool AddUserToRoom(string roomId, string userId, string username)
    {
        var room = GetRoomById(roomId);
        if (room == null) return false;
        
        var user = room.Users.FirstOrDefault(u => u.UserName == userId);

        if (user == null)
        {
            room.Users.Add(new User()
            {
                UserName = username,
                ConnectionId = userId
            });
        }

        return true;
    }
    
    public bool RemoveUserFromRoom(string roomId, string userId)
    {
        var room = GetRoomById(roomId);
        if(room == null) return false;

        var user = room.Users.FirstOrDefault(u => u.ConnectionId == userId);
        
        return room.Users.Remove(user!);
    }
    
    public void AddRoom(Room room) 
        => _rooms.Add(room);
    
    public Room? GetRoomById(string roomId) 
        => _rooms.FirstOrDefault(x => x.Id == roomId);

    public Room? GetRoomByName(string name) 
        => _rooms.FirstOrDefault(x => x.Name == name);
    
    public IEnumerable<Room> GetRooms() 
        => _rooms;
}