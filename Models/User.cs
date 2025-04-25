namespace GachiHubBackend.Models;

public class User
{
    public string? UserName { get; set; } = string.Empty;
    
    public bool Connected { get; set; } = false;

    public string ConnectionId { get; set; } = string.Empty;
}