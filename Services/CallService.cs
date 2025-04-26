using GachiHubBackend.Models;

namespace GachiHubBackend.Services;

public class CallService
{
    private readonly List<Call> _calls;

    public CallService()
    {
        _calls = new List<Call>();
    }
    
    public void RemoveCall(Call call) => _calls.Remove(call);
    
    public void AddCall(Call call) => _calls.Add(call);
    
    public Call? GetCallById(string callId) => _calls.FirstOrDefault(c => c.CallId == callId);

    public Call? GetCallByUserConnectionId(string connectionId)
    {
        var call = _calls.SingleOrDefault(c => c.To.ConnectionId == connectionId || c.From.ConnectionId == connectionId);
        return call;
    }
}