namespace GachiHubBackend.Models;

public class Message
{
    public string Content { get; set; } = string.Empty;

    public DateTime Sended { get; set; } = DateTime.Now;

    public User From { get; set; } = null!;
    
    public User To { get; set; } = null!;
}