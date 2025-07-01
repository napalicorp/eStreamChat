using System;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace eStreamChat.Common.Security;

/// <summary>
/// Provides cryptographic hash generation functionality for chat authentication
/// </summary>
public class HashGenerator
{
    private readonly string _authSecretKey;

    public HashGenerator(IConfiguration configuration)
    {
        _authSecretKey = configuration["AuthSecretKey"] 
            ?? throw new InvalidOperationException("AuthSecretKey must be specified in configuration");
    }

    /// <summary>
    /// Calculates a chat authentication hash using SHA256
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="targetUserId">The target user ID</param>
    /// <param name="timestamp">The timestamp</param>
    /// <returns>The computed hash string</returns>
    public string CalculateChatAuthHash(string userId, string targetUserId, string timestamp)
    {
        ArgumentNullException.ThrowIfNull(userId);
        ArgumentNullException.ThrowIfNull(targetUserId);
        ArgumentNullException.ThrowIfNull(timestamp);

        byte[] paramBytes = Encoding.UTF8.GetBytes(userId + targetUserId + timestamp + _authSecretKey);
        
        using var sha256 = SHA256.Create();
        byte[] hashBytes = sha256.ComputeHash(paramBytes);
        
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }
}