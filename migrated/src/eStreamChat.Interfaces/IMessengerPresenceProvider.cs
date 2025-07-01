namespace eStreamChat.Interfaces;

/// <summary>
/// Defines a contract for managing user presence and chat requests in the messenger system
/// </summary>
public interface IMessengerPresenceProvider
{
    /// <summary>
    /// Updates the last online timestamp for a user
    /// </summary>
    /// <param name="userId">The user's unique identifier</param>
    void UpdateLastOnline(string userId);

    /// <summary>
    /// Gets any pending chat request for a user
    /// </summary>
    /// <param name="toUserId">The target user's unique identifier</param>
    /// <returns>The pending chat request or null if none exists</returns>
    ChatRequest? GetChatRequest(string toUserId);

    /// <summary>
    /// Adds a new chat request
    /// </summary>
    /// <param name="request">The chat request to add</param>
    /// <exception cref="ArgumentNullException">Thrown when request is null</exception>
    void AddChatRequest(ChatRequest request);

    /// <summary>
    /// Removes a chat request between two users
    /// </summary>
    /// <param name="fromUserId">The requesting user's unique identifier</param>
    /// <param name="toUserId">The target user's unique identifier</param>
    void RemoveChatRequest(string fromUserId, string toUserId);
}