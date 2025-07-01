using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using eStreamChat.Interfaces;
using eStreamChat.Common.Utilities;

namespace eStreamChat.Common.Services;

public class UserCleanupOptions
{
    public TimeSpan UserTimeout { get; set; } = TimeSpan.FromMinutes(1);
    public TimeSpan CleanupInterval { get; set; } = TimeSpan.FromMinutes(1);
}

/// <summary>
/// Background service that periodically removes inactive users from chat rooms
/// </summary>
public class UserCleanupService : BackgroundService
{
    private readonly IChatUserProvider _userProvider;
    private readonly IChatRoomStorage _storage;
    private readonly ILogger<UserCleanupService> _logger;
    private readonly UserCleanupOptions _options;
    private readonly SemaphoreSlim _lock = new(1, 1);

    public UserCleanupService(
        IChatUserProvider userProvider,
        IChatRoomStorage storage,
        ILogger<UserCleanupService> logger,
        IOptions<UserCleanupOptions> options)
    {
        _userProvider = userProvider ?? throw new ArgumentNullException(nameof(userProvider));
        _storage = storage ?? throw new ArgumentNullException(nameof(storage));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                if (await _lock.WaitAsync(TimeSpan.Zero, stoppingToken))
                {
                    try
                    {
                        await CleanupInactiveUsersAsync(stoppingToken);
                    }
                    finally
                    {
                        _lock.Release();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while cleaning up inactive users");
            }

            await Task.Delay(_options.CleanupInterval, stoppingToken);
        }
    }

    private async Task CleanupInactiveUsersAsync(CancellationToken cancellationToken)
    {
        var removedUsers = _storage.RemoveInactiveUsers(_options.UserTimeout);
        var timestamp = TimeUtilities.GetLegacyTimestamp();

        foreach (var userInRoom in removedUsers)
        {
            if (cancellationToken.IsCancellationRequested)
                break;

            var userId = userInRoom.Key;
            var roomId = userInRoom.Value;

            var user = await Task.Run(() => _userProvider.GetUser(userId), cancellationToken);

            if (user != null)
            {
                _storage.AddMessage(roomId, new Message
                {
                    Content = $"User {user.DisplayName} has left the chat. (timeout)",
                    FromUserId = userId,
                    MessageType = MessageType.UserLeft,
                    Timestamp = timestamp
                });

                if (_storage.UnregisterUserBroadcasts(roomId, userId))
                {
                    _storage.AddMessage(roomId, new Message
                    {
                        Content = string.Empty,
                        FromUserId = userId,
                        MessageType = MessageType.StopVideoBroadcast,
                        Timestamp = timestamp
                    });
                }
            }
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("User cleanup service is stopping");
        await base.StopAsync(cancellationToken);
    }
}