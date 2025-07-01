namespace eStreamChat.Interfaces;

/// <summary>
/// Represents a request to initiate a private chat session between users
/// </summary>
public sealed record ChatRequest
{
    /// <summary>
    /// Gets the unique identifier of the user initiating the chat request
    /// </summary>
    public required string FromUserId { get; init; }

    /// <summary>
    /// Gets the display name of the user initiating the chat request
    /// </summary>
    public required string FromUsername { get; init; }

    /// <summary>
    /// Gets the URL to the thumbnail image of the requesting user
    /// </summary>
    public string? FromThumbnailUrl { get; init; }

    /// <summary>
    /// Gets the URL to the profile page of the requesting user
    /// </summary>
    public string? FromProfileUrl { get; init; }

    /// <summary>
    /// Gets the unique identifier of the user receiving the chat request
    /// </summary>
    public required string ToUserId { get; init; }

    /// <summary>
    /// Gets the display name of the user receiving the chat request
    /// </summary>
    public required string ToUsername { get; init; }

    /// <summary>
    /// Gets the timestamp when the request was created
    /// </summary>
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Gets the URL where the private chat session will take place
    /// </summary>
    public string? MessengerUrl { get; init; }

    /// <summary>
    /// Gets the optional message included with the chat request
    /// </summary>
    public string? ChatRequestMessage { get; init; }
}