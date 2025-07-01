using eStreamChat.Interfaces;

namespace eStreamChat.Common.Services;

public class ChatRoomStorage : IChatRoomStorage
{
    private readonly Dictionary<string, HashSet<string>> _roomUsers = new();
    private readonly Dictionary<string, List<Message>> _roomMessages = new();
    private readonly Dictionary<string, string> _userTokens = new();
    private readonly Dictionary<string, Dictionary<string, string>> _roomBroadcasts = new();

    public void AddUserToRoom(string roomId, string userId)
    {
        if (!_roomUsers.ContainsKey(roomId))
            _roomUsers[roomId] = new HashSet<string>();
        
        _roomUsers[roomId].Add(userId);
    }

    public void RemoveUserFromRoom(string roomId, string userId)
    {
        if (_roomUsers.ContainsKey(roomId))
            _roomUsers[roomId].Remove(userId);
    }

    public string[] GetUsersInRoom(string roomId)
    {
        return _roomUsers.TryGetValue(roomId, out var users) 
            ? users.ToArray() 
            : Array.Empty<string>();
    }

    public bool IsUserInRoom(string roomId, string userId)
    {
        return _roomUsers.TryGetValue(roomId, out var users) && users.Contains(userId);
    }

    public void AddMessage(string roomId, Message message)
    {
        if (!_roomMessages.ContainsKey(roomId))
            _roomMessages[roomId] = new List<Message>();
        
        _roomMessages[roomId].Add(message);
    }

    public Message[] GetMessages(string roomId, string userId, DateTimeOffset fromTimestamp)
    {
        if (!_roomMessages.TryGetValue(roomId, out var messages))
            return Array.Empty<Message>();

        return messages.Where(m => m.Timestamp >= fromTimestamp).ToArray();
    }

    public Message[] GetMessages(string roomId, string userId, DateTimeOffset? fromTimestamp = null)
    {
        if (!_roomMessages.TryGetValue(roomId, out var messages))
            return Array.Empty<Message>();

        return fromTimestamp.HasValue
            ? messages.Where(m => m.Timestamp >= fromTimestamp.Value).ToArray()
            : messages.ToArray();
    }

    public string GenerateUserToken(string userId)
    {
        var token = Guid.NewGuid().ToString();
        _userTokens[token] = userId;
        return token;
    }

    public string? GetUserIdByToken(string token)
    {
        return _userTokens.TryGetValue(token, out var userId) ? userId : null;
    }

    public void RegisterBroadcast(string roomId, string userId, string targetUserId, string guid)
    {
        if (!_roomBroadcasts.ContainsKey(roomId))
            _roomBroadcasts[roomId] = new Dictionary<string, string>();

        _roomBroadcasts[roomId][userId] = guid;
    }

    public bool UnregisterUserBroadcasts(string roomId, string userId)
    {
        if (_roomBroadcasts.TryGetValue(roomId, out var broadcasts))
        {
            return broadcasts.Remove(userId);
        }
        return false;
    }

    public Dictionary<string, string> GetBroadcasts(string roomId, string userId)
    {
        return _roomBroadcasts.TryGetValue(roomId, out var broadcasts)
            ? new Dictionary<string, string>(broadcasts)
            : new Dictionary<string, string>();
    }

    public void UpdateOnline(string roomId, string userId)
    {
        // Implement online status tracking if needed
    }

    public Dictionary<string, string> RemoveInactiveUsers(TimeSpan inactivityThreshold)
    {
        var now = DateTimeOffset.UtcNow;
        var removedUsers = new Dictionary<string, string>();

        foreach (var roomId in _roomUsers.Keys.ToList())
        {
            var inactiveUsers = _roomUsers[roomId]
                .Where(userId => now - GetLastActivity(userId) > inactivityThreshold)
                .ToList();

            foreach (var userId in inactiveUsers)
            {
                RemoveUserFromRoom(roomId, userId);
                removedUsers[roomId] = userId;
            }
        }

        return removedUsers;
    }

    public void DeleteAllMessagesFor(string roomId, string userId)
    {
        if (_roomMessages.TryGetValue(roomId, out var messages))
        {
            _roomMessages[roomId] = messages
                .Where(m => m.FromUserId != userId && m.ToUserId != userId)
                .ToList();
        }
    }

    private DateTimeOffset GetLastActivity(string userId)
    {
        // For demo purposes, return current time
        // In a real implementation, track user activity timestamps
        return DateTimeOffset.UtcNow;
    }
}