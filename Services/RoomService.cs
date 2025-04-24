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

    public bool AddUserToRoom(string roomId, User user)
    {
        var room = GetRoomById(roomId);
        if (room == null) return false;

        room.Users.Add(user);

        return true;
    }
    
    public bool RemoveUserFromRoom(string roomId, User user)
    {
        var room = GetRoomById(roomId);
        if(room == null) return false;
        
        return room.Users.Remove(user);
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