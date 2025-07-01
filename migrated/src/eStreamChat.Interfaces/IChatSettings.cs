namespace eStreamChat.Interfaces;

/// <summary>
/// Defines a contract for chat system configuration settings
/// </summary>
public interface IChatSettings
{
    /// <summary>
    /// Gets whether file transfer functionality is enabled
    /// </summary>
    bool EnableFileTransfer { get; }

    /// <summary>
    /// Gets the comma-separated list of allowed file extensions for file transfers
    /// </summary>
    /// <remarks>Only applicable when EnableFileTransfer is true</remarks>
    string? SendFileAllowedExtensions { get; }

    /// <summary>
    /// Gets whether video chat functionality is enabled
    /// </summary>
    bool EnableVideoChat { get; }

    /// <summary>
    /// Gets the Flash Media Server URL for video chat
    /// </summary>
    /// <remarks>Only applicable when EnableVideoChat is true</remarks>
    string? FlashMediaServer { get; }

    /// <summary>
    /// Gets the Flash server type configuration
    /// </summary>
    /// <remarks>Only applicable when EnableVideoChat is true</remarks>
    string? FlashServerType { get; }
}