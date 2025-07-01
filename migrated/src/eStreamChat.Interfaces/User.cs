namespace eStreamChat.Interfaces;

/// <summary>
/// Represents a chat user with their profile information
/// </summary>
public sealed record User
{
    /// <summary>
    /// Gets the unique identifier for the user
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// Gets the display name shown in the chat interface
    /// </summary>
    public required string DisplayName { get; init; }

    /// <summary>
    /// Gets the URL to the user's full-size photo
    /// </summary>
    public string? PhotoUrl { get; init; }

    /// <summary>
    /// Gets the URL to the user's thumbnail photo
    /// </summary>
    public string? ThumbnailUrl { get; init; }

    /// <summary>
    /// Gets the URL to the user's profile page
    /// </summary>
    public string? ProfileUrl { get; init; }
}