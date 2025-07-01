namespace eStreamChat.Interfaces;

/// <summary>
/// Defines a contract for managing chat room state and message storage
/// </summary>
public interface IChatRoomStorage
{
    /// <summary>
    /// Generates a unique token for a user
    /// </summary>
    /// <param name="userId">The user's unique identifier</param>
    /// <returns>A unique token string</returns>
    string GenerateUserToken(string userId);

    /// <summary>
    /// Retrieves the user ID associated with a token
    /// </summary>
    /// <param name="token">The token to look up</param>
    /// <returns>The associated user ID</returns>
    string? GetUserIdByToken(string token);

    /// <summary>
    /// Adds a user to a chat room
    /// </summary>
    /// <param name="chatRoomId">The chat room identifier</param>
    /// <param name="userId">The user's unique identifier</param>
    void AddUserToRoom(string chatRoomId, string userId);

    /// <summary>
    /// Removes a user from a chat room
    /// </summary>
    /// <param name="chatRoomId">The chat room identifier</param>
    /// <param name="userId">The user's unique identifier</param>
    void RemoveUserFromRoom(string chatRoomId, string userId);

    /// <summary>
    /// Gets all users currently in a chat room
    /// </summary>
    /// <param name="chatRoomId">The chat room identifier</param>
    /// <returns>An array of user IDs</returns>
    string[] GetUsersInRoom(string chatRoomId);

    /// <summary>
    /// Checks if a user is in a specific chat room
    /// </summary>
    /// <param name="chatRoomId">The chat room identifier</param>
    /// <param name="userId">The user's unique identifier</param>
    /// <returns>True if the user is in the room, false otherwise</returns>
    bool IsUserInRoom(string chatRoomId, string userId);

    /// <summary>
    /// Updates the last online timestamp for a user in a room
    /// </summary>
    /// <param name="chatRoomId">The chat room identifier</param>
    /// <param name="userId">The user's unique identifier</param>
    void UpdateOnline(string chatRoomId, string userId);

    /// <summary>
    /// Removes users who have been inactive for longer than the specified duration
    /// </summary>
    /// <param name="timeOfInactivity">The duration of inactivity after which users should be removed</param>
    /// <returns>A dictionary mapping chat room IDs to user IDs of removed users</returns>
    Dictionary<string, string> RemoveInactiveUsers(TimeSpan timeOfInactivity);

    /// <summary>
    /// Adds a message to a chat room
    /// </summary>
    /// <param name="chatRoomId">The chat room identifier</param>
    /// <param name="message">The message to add</param>
    void AddMessage(string chatRoomId, Message message);

    /// <summary>
    /// Retrieves messages from a chat room after a specific timestamp
    /// </summary>
    /// <param name="chatRoomId">The chat room identifier</param>
    /// <param name="userId">The user requesting the messages</param>
    /// <param name="fromTimestamp">The timestamp to retrieve messages from</param>
    /// <returns>An array of messages</returns>
    Message[] GetMessages(string chatRoomId, string userId, DateTimeOffset fromTimestamp);

    /// <summary>
    /// Deletes all messages for a specific user in a chat room
    /// </summary>
    /// <param name="chatRoomId">The chat room identifier</param>
    /// <param name="userId">The user's unique identifier</param>
    void DeleteAllMessagesFor(string chatRoomId, string userId);

    /// <summary>
    /// Registers a video broadcast session
    /// </summary>
    /// <param name="chatRoomId">The chat room identifier</param>
    /// <param name="userId">The broadcasting user's ID</param>
    /// <param name="targetUserId">The target user's ID</param>
    /// <param name="guid">The unique identifier for the broadcast session</param>
    void RegisterBroadcast(string chatRoomId, string userId, string targetUserId, string guid);

    /// <summary>
    /// Unregisters all broadcast sessions for a user in a chat room
    /// </summary>
    /// <param name="chatRoomId">The chat room identifier</param>
    /// <param name="userId">The user's unique identifier</param>
    /// <returns>True if any broadcasts were unregistered, false otherwise</returns>
    bool UnregisterUserBroadcasts(string chatRoomId, string userId);

    /// <summary>
    /// Gets all active broadcasts for a specific receiver in a chat room
    /// </summary>
    /// <param name="chatRoomId">The chat room identifier</param>
    /// <param name="receiverId">The receiver's user ID</param>
    /// <returns>A dictionary mapping sender IDs to broadcast GUIDs</returns>
    Dictionary<string, string> GetBroadcasts(string chatRoomId, string receiverId);
}