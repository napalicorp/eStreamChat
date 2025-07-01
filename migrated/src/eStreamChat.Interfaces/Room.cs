namespace eStreamChat.Interfaces;

/// <summary>
/// Represents a chat room with its properties and settings
/// </summary>
public sealed record Room
{
    /// <summary>
    /// Gets the unique identifier for the room
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// Gets the display name of the room
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Gets the room's topic or description
    /// </summary>
    public string? Topic { get; init; }

    /// <summary>
    /// Gets the password required to join the room, if any
    /// </summary>
    public string? Password { get; init; }

    /// <summary>
    /// Gets the maximum number of users allowed in the room
    /// A value of 0 indicates no limit
    /// </summary>
    public int MaxUsers { get; init; }

    /// <summary>
    /// Gets whether the room is visible in room listings
    /// </summary>
    public bool Visible { get; init; }
}