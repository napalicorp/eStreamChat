using eStreamChat.Interfaces;

namespace eStreamChat.Common.Services;

public class ChatRoomProvider : IChatRoomProvider
{
    private readonly Dictionary<string, Room> _rooms = new();
    private readonly HashSet<(string RoomId, string UserId)> _bannedUsers = new();

    public ChatRoomProvider()
    {
        // Add a default public room
        _rooms.Add("-1", new Room
        {
            Id = "-1",
            Name = "Public Room",
            Topic = "General Discussion",
            MaxUsers = 100
        });

        // Add a default messenger room
        _rooms.Add("-2", new Room
        {
            Id = "-2",
            Name = "Messenger",
            Topic = "Private Messages",
            MaxUsers = 2
        });
    }

    public Room? GetChatRoom(string roomId)
    {
        return _rooms.TryGetValue(roomId, out var room) ? room : null;
    }

    public IEnumerable<Room> GetChatRooms()
    {
        return _rooms.Values;
    }

    public bool HasChatAccess(string userId, string roomId, out string? reason)
    {
        reason = null;

        if (_bannedUsers.Contains((roomId, userId)))
        {
            reason = "You have been banned from this chat room.";
            return false;
        }

        return true;
    }

    public void BanUser(string roomId, string adminUserId, string targetUserId)
    {
        _bannedUsers.Add((roomId, targetUserId));
    }
}