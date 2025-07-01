namespace eStreamChat.Interfaces;

/// <summary>
/// Defines a contract for managing chat users and their permissions
/// </summary>
public interface IChatUserProvider
{
    /// <summary>
    /// Gets the currently logged-in user
    /// </summary>
    /// <returns>The current user or null if no user is logged in</returns>
    User? GetCurrentlyLoggedUser();

    /// <summary>
    /// Gets a user by their unique identifier
    /// </summary>
    /// <param name="userId">The user's unique identifier</param>
    /// <returns>The user or null if not found</returns>
    User? GetUser(string userId);

    /// <summary>
    /// Adds a user to another user's ignore list
    /// </summary>
    /// <param name="userId">The user doing the ignoring</param>
    /// <param name="ignoredUserId">The user being ignored</param>
    void IgnoreUser(string userId, string ignoredUserId);

    /// <summary>
    /// Checks if a user is being ignored by another user
    /// </summary>
    /// <param name="userId">The user who might be ignoring</param>
    /// <param name="ignoredUserId">The user who might be ignored</param>
    /// <returns>True if the user is being ignored, false otherwise</returns>
    bool IsUserIgnored(string userId, string ignoredUserId);

    /// <summary>
    /// Checks if a user has admin privileges in a chat room
    /// </summary>
    /// <param name="userId">The user's unique identifier</param>
    /// <param name="chatRoomId">The chat room identifier</param>
    /// <returns>True if the user is an admin, false otherwise</returns>
    bool IsChatAdmin(string userId, string chatRoomId);

    /// <summary>
    /// Gets the URL for the login page
    /// </summary>
    /// <param name="backURL">The URL to redirect to after login</param>
    /// <returns>The complete login URL</returns>
    string GetLoginUrl(string backURL);
}