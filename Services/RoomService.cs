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
    
    public IEnumerable<Room> GetRooms() => _rooms;

    public Room CreateRoom(string name, bool hasAudio = true, bool hasMessage = true)
    {
        var room = new Room()
        {
            Name = name,
        };
        
        _rooms.Add(room);
        return room;
    }

    public bool AddUserToRoom(string roomId, string userId)
    {
        var room = GetRoomById(roomId);
        if (room == null) return false;

        if (!room.UserIds.Contains(userId))
        {
            room.UserIds.Add(userId);
        }

        return true;
    }
    
    public Room? GetRoomById(string roomId) 
        => _rooms.FirstOrDefault(x => x.Id == roomId);

    public bool RemoveUserFromRoom(string roomId, string userId)
    {
        var room = GetRoomById(roomId);
        if(room == null) return false;
        
        return room.UserIds.Remove(userId);
    }

    public void AddRoom(Room room) => _rooms.Add(room);
}