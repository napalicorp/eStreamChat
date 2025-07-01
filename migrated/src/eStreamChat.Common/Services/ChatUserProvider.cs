using eStreamChat.Interfaces;

namespace eStreamChat.Common.Services;

public class ChatUserProvider : IChatUserProvider
{
    private readonly Dictionary<string, User> _users = new();
    private readonly HashSet<(string UserId, string IgnoredUserId)> _ignoredUsers = new();
    private readonly HashSet<(string UserId, string RoomId)> _admins = new();

    public User? GetCurrentlyLoggedUser()
    {
        // For demo purposes, return a test user
        var userId = "test-user";
        if (!_users.ContainsKey(userId))
        {
            _users[userId] = new User
            {
                Id = userId,
                DisplayName = "Test User",
                PhotoUrl = "https://example.com/photo.jpg",
                ThumbnailUrl = "https://example.com/thumbnail.jpg",
                ProfileUrl = "https://example.com/profile"
            };
        }
        return _users[userId];
    }

    public User GetUser(string userId)
    {
        if (!_users.TryGetValue(userId, out var user))
        {
            user = new User
            {
                Id = userId,
                DisplayName = $"User {userId}"
            };
            _users[userId] = user;
        }
        return user;
    }

    public bool IsChatAdmin(string userId, string roomId)
    {
        return _admins.Contains((userId, roomId));
    }

    public void IgnoreUser(string userId, string ignoredUserId)
    {
        _ignoredUsers.Add((userId, ignoredUserId));
    }

    public bool IsUserIgnored(string userId, string ignoredUserId)
    {
        return _ignoredUsers.Contains((userId, ignoredUserId));
    }

    public string GetLoginUrl(string backURL)
    {
        // For demo purposes, no login required
        return null;
    }
}