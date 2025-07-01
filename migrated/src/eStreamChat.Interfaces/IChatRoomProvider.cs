namespace eStreamChat.Interfaces;

/// <summary>
/// Defines a contract for managing chat rooms and user access
/// </summary>
public interface IChatRoomProvider
{
    /// <summary>
    /// Gets all available chat rooms
    /// </summary>
    /// <returns>A collection of chat rooms</returns>
    IEnumerable<Room> GetChatRooms();

    /// <summary>
    /// Gets a specific chat room by its identifier
    /// </summary>
    /// <param name="chatRoomId">The chat room identifier. Use "-2" for messenger communication.</param>
    /// <returns>The requested chat room or null if not found</returns>
    Room? GetChatRoom(string chatRoomId);

    /// <summary>
    /// Bans a user from a chat room
    /// </summary>
    /// <param name="chatRoomId">The chat room identifier</param>
    /// <param name="userId">The user performing the ban</param>
    /// <param name="bannedUserId">The user being banned</param>
    /// <exception cref="UnauthorizedAccessException">Thrown when the user doesn't have permission to ban</exception>
    void BanUser(string chatRoomId, string userId, string bannedUserId);

    /// <summary>
    /// Checks if a user has access to a chat room
    /// </summary>
    /// <param name="userId">The user's unique identifier</param>
    /// <param name="chatRoomId">The chat room identifier</param>
    /// <param name="reason">When access is denied, contains the reason; otherwise, null</param>
    /// <returns>True if the user has access, false otherwise</returns>
    bool HasChatAccess(string userId, string chatRoomId, out string? reason);
}