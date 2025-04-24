namespace GachiHubBackend.Models;

public class Room
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    public string Name { get; set; } = string.Empty;

    public List<User> Users { get; set; } = [];

    public User Owner { get; set; } = null!;
    
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}