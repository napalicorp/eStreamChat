namespace eStreamChat.Api.Models.Requests;

/// <summary>
/// Request model for joining a chat room
/// </summary>
public record JoinChatRoomRequest
{
    /// <summary>
    /// The ID of the chat room to join
    /// </summary>
    public required string ChatRoomId { get; init; }

    /// <summary>
    /// The URL from which the join request originated
    /// </summary>
    public required string Href { get; init; }
}

/// <summary>
/// Request model for leaving a chat room
/// </summary>
public record LeaveChatRoomRequest
{
    /// <summary>
    /// The ID of the chat room to leave
    /// </summary>
    public required string ChatRoomId { get; init; }

    /// <summary>
    /// The authentication token of the user
    /// </summary>
    public required string Token { get; init; }

    /// <summary>
    /// The target user ID for messenger mode
    /// </summary>
    public string? MessengerTargetUserId { get; init; }
}

/// <summary>
/// Request model for getting chat room events
/// </summary>
public record GetEventsRequest
{
    /// <summary>
    /// The ID of the chat room
    /// </summary>
    public required string ChatRoomId { get; init; }

    /// <summary>
    /// The authentication token of the user
    /// </summary>
    public required string Token { get; init; }

    /// <summary>
    /// The timestamp from which to get events
    /// </summary>
    public required DateTimeOffset FromTimestamp { get; init; }

    /// <summary>
    /// The target user ID for messenger mode
    /// </summary>
    public string? MessengerTargetUserId { get; init; }
}

/// <summary>
/// Request model for sending a chat message
/// </summary>
public record SendMessageRequest
{
    /// <summary>
    /// The ID of the chat room
    /// </summary>
    public required string ChatRoomId { get; init; }

    /// <summary>
    /// The authentication token of the user
    /// </summary>
    public required string Token { get; init; }

    /// <summary>
    /// The target user ID for private messages
    /// </summary>
    public string? ToUserId { get; init; }

    /// <summary>
    /// The message content
    /// </summary>
    public required string Message { get; init; }

    /// <summary>
    /// Whether the message should be bold
    /// </summary>
    public bool Bold { get; init; }

    /// <summary>
    /// Whether the message should be italic
    /// </summary>
    public bool Italic { get; init; }

    /// <summary>
    /// Whether the message should be underlined
    /// </summary>
    public bool Underline { get; init; }

    /// <summary>
    /// The font name for the message
    /// </summary>
    public string? FontName { get; init; }

    /// <summary>
    /// The font size for the message
    /// </summary>
    public int? FontSize { get; init; }

    /// <summary>
    /// The color for the message (hex format)
    /// </summary>
    public string? Color { get; init; }
}

/// <summary>
/// Request model for sending a chat command
/// </summary>
public record SendCommandRequest
{
    /// <summary>
    /// The ID of the chat room
    /// </summary>
    public required string ChatRoomId { get; init; }

    /// <summary>
    /// The authentication token of the user
    /// </summary>
    public required string Token { get; init; }

    /// <summary>
    /// The target user ID for the command
    /// </summary>
    public required string TargetUserId { get; init; }

    /// <summary>
    /// The command to execute (ignore, kick, ban, slap)
    /// </summary>
    public required string Command { get; init; }
}

/// <summary>
/// Request model for starting a video broadcast
/// </summary>
public record BroadcastVideoRequest
{
    /// <summary>
    /// The previous broadcast GUID if reconnecting
    /// </summary>
    public string? PrevGuid { get; init; }

    /// <summary>
    /// The authentication token of the user
    /// </summary>
    public required string Token { get; init; }

    /// <summary>
    /// The ID of the chat room
    /// </summary>
    public required string ChatRoomId { get; init; }

    /// <summary>
    /// The target user ID for private broadcasts
    /// </summary>
    public string? TargetUserId { get; init; }
}

/// <summary>
/// Request model for stopping a video broadcast
/// </summary>
public record StopBroadcastRequest
{
    /// <summary>
    /// The authentication token of the user
    /// </summary>
    public required string Token { get; init; }

    /// <summary>
    /// The ID of the chat room
    /// </summary>
    public required string ChatRoomId { get; init; }
}