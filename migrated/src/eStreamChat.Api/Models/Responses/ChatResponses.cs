using eStreamChat.Interfaces;

namespace eStreamChat.Api.Models.Responses;

/// <summary>
/// Response model for joining a chat room
/// </summary>
public record JoinChatRoomResponse
{
    /// <summary>
    /// Error message if the join operation failed
    /// </summary>
    public string? Error { get; init; }

    /// <summary>
    /// URL to redirect to (e.g., login page)
    /// </summary>
    public string? RedirectUrl { get; init; }

    /// <summary>
    /// The name of the chat room
    /// </summary>
    public string? ChatRoomName { get; init; }

    /// <summary>
    /// The topic/description of the chat room
    /// </summary>
    public string? ChatRoomTopic { get; init; }

    /// <summary>
    /// Authentication token for subsequent requests
    /// </summary>
    public string? Token { get; init; }

    /// <summary>
    /// The ID of the user who joined
    /// </summary>
    public string? UserId { get; init; }

    /// <summary>
    /// List of users currently in the room
    /// </summary>
    public required User[] Users { get; init; }

    /// <summary>
    /// Whether the user has admin privileges
    /// </summary>
    public bool IsAdmin { get; init; }

    /// <summary>
    /// Whether file transfer is enabled
    /// </summary>
    public bool FileTransferEnabled { get; init; }

    /// <summary>
    /// Whether video chat is enabled
    /// </summary>
    public bool VideoChatEnabled { get; init; }

    /// <summary>
    /// Flash media server URL for video chat
    /// </summary>
    public string? FlashMediaServer { get; init; }

    /// <summary>
    /// Active video broadcasts in the room
    /// </summary>
    public required Dictionary<string, string> Broadcasts { get; init; }
}

/// <summary>
/// Response model for getting chat room events
/// </summary>
public record GetEventsResponse
{
    /// <summary>
    /// Error message if the operation failed
    /// </summary>
    public string? Error { get; init; }

    /// <summary>
    /// List of messages and events
    /// </summary>
    public required Message[] Messages { get; init; }

    /// <summary>
    /// Users who joined during this event window
    /// </summary>
    public required User[] UsersJoined { get; init; }

    /// <summary>
    /// Users who left during this event window
    /// </summary>
    public required User[] UsersLeft { get; init; }

    /// <summary>
    /// Polling interval in milliseconds
    /// </summary>
    public int? CallInterval { get; init; }
}

/// <summary>
/// Response model for send message operations
/// </summary>
public record SendMessageResponse
{
    /// <summary>
    /// Error message if the operation failed
    /// </summary>
    public string? Error { get; init; }
}

/// <summary>
/// Response model for video broadcast operations
/// </summary>
public record BroadcastVideoResponse
{
    /// <summary>
    /// The unique identifier for the broadcast session
    /// </summary>
    public string? Guid { get; init; }
}

/// <summary>
/// Response model for API errors
/// </summary>
public record ApiErrorResponse
{
    /// <summary>
    /// The main error message
    /// </summary>
    public required string Error { get; init; }

    /// <summary>
    /// Additional error details or validation errors
    /// </summary>
    public string? Details { get; init; }
}