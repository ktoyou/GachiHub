namespace GachiHubBackend.Models;

public class Call
{
    public string CallId { get; set; } = string.Empty;

    public User From { get; set; } = null!;

    public User To { get; set; } = null!;

    public List<Message> Messages { get; set; } = null!;
}