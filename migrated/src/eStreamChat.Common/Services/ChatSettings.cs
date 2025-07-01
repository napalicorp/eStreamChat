using eStreamChat.Interfaces;
using Microsoft.Extensions.Configuration;

namespace eStreamChat.Common.Services;

public class ChatSettings : IChatSettings
{
    private readonly IConfiguration _configuration;

    public ChatSettings(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public bool EnableFileTransfer => _configuration.GetValue<bool>("Chat:EnableFileTransfer", false);

    public bool EnableVideoChat => _configuration.GetValue<bool>("Chat:EnableVideoChat", false);

    public string? FlashMediaServer => _configuration.GetValue<string>("Chat:FlashMediaServer");

    public string? SendFileAllowedExtensions => _configuration.GetValue<string>("Chat:AllowedFileExtensions", ".jpg,.png,.gif");

    public string FlashServerType => _configuration.GetValue<string>("Chat:FlashServerType", "red5");
}