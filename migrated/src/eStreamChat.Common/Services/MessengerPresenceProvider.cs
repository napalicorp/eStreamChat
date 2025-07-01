using eStreamChat.Interfaces;

namespace eStreamChat.Common.Services;

public class MessengerPresenceProvider : IMessengerPresenceProvider
{
    private readonly Dictionary<string, bool> _onlineStatus = new();
    private readonly Dictionary<string, DateTimeOffset> _lastOnline = new();
    private readonly Dictionary<string, ChatRequest> _chatRequests = new();
    private readonly Dictionary<ChatRequest, string> _requestGuids = new();

    public bool IsUserOnline(string userId)
    {
        return _onlineStatus.TryGetValue(userId, out var isOnline) && isOnline;
    }

    public void SetUserOnline(string userId, bool isOnline)
    {
        _onlineStatus[userId] = isOnline;
        if (isOnline)
        {
            UpdateLastOnline(userId);
        }
    }

    public void UpdateLastOnline(string userId)
    {
        _lastOnline[userId] = DateTimeOffset.UtcNow;
    }

    public ChatRequest? GetChatRequest(string guid)
    {
        return _chatRequests.TryGetValue(guid, out var request) ? request : null;
    }

    public void AddChatRequest(ChatRequest request)
    {
        var guid = Guid.NewGuid().ToString();
        _chatRequests[guid] = request;
        _requestGuids[request] = guid;
    }

    public void RemoveChatRequest(string fromUserId, string toUserId)
    {
        var request = _chatRequests.Values.FirstOrDefault(r =>
            r.FromUserId == fromUserId && r.ToUserId == toUserId);
        
        if (request != null && _requestGuids.TryGetValue(request, out var guid))
        {
            _chatRequests.Remove(guid);
            _requestGuids.Remove(request);
        }
    }
}