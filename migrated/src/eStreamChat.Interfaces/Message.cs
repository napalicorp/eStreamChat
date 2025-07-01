namespace eStreamChat.Interfaces;

/// <summary>
/// Defines the type of message in the chat system
/// </summary>
public enum MessageType
{
    System = 1,
    User = 2,
    Status = 3,
    UserJoined = 4,
    UserLeft = 5,
    SendFile = 6,
    SendImageFile = 7,
    Kicked = 8, // Used also for banned
    VideoBroadcast = 9,
    StopVideoBroadcast = 10,
    RequestAccepted = 12,
    RequestDeclined = 13
}

/// <summary>
/// Represents formatting options for a chat message
/// </summary>
public sealed record MessageFormatOptions
{
    public bool Bold { get; init; }
    public string? Color { get; init; }
    public string? FontName { get; init; }
    public int FontSize { get; init; }
    public bool Italic { get; init; }
    public bool Underline { get; init; }
}

/// <summary>
/// Represents a chat message with its content and metadata
/// </summary>
public sealed record Message
{
    public required string Content { get; init; }
    public MessageFormatOptions? FormatOptions { get; init; }
    public required string FromUserId { get; init; }
    public required MessageType MessageType { get; init; }
    public required DateTimeOffset Timestamp { get; init; }
    public string? ToUserId { get; init; }
}

/// <summary>
/// Represents a video broadcast session between users
/// </summary>
public sealed record Broadcast
{
    public required string Guid { get; init; }
    public required string ReceiverId { get; init; }
    public required string SenderId { get; init; }
}