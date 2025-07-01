namespace eStreamChat.Interfaces;

/// <summary>
/// Defines a contract for logging messages in the chat system
/// </summary>
public interface ILogger
{
    /// <summary>
    /// Logs a message to the configured logging system
    /// </summary>
    /// <param name="message">The message to log</param>
    /// <exception cref="ArgumentNullException">Thrown when message is null</exception>
    void Log(string message);
}