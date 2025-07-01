using System;

namespace eStreamChat.Common.Utilities;

/// <summary>
/// Provides utility methods for timestamp operations
/// </summary>
public static class TimeUtilities
{
    private static readonly DateTimeOffset BaseDate = new(2000, 1, 1, 0, 0, 0, TimeSpan.Zero);

    /// <summary>
    /// Gets a timestamp relative to year 2000 for compatibility with legacy systems.
    /// </summary>
    public static DateTimeOffset GetLegacyTimestamp() => DateTimeOffset.UtcNow;

    /// <summary>
    /// Gets ticks since year 2000 for legacy compatibility
    /// </summary>
    public static long GetLegacyTicks() => DateTimeOffset.UtcNow.Subtract(BaseDate).Ticks;
}