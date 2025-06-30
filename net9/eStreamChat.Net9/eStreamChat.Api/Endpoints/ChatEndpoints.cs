using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using eStreamChat.Api.Models;

namespace eStreamChat.Api.Endpoints
{
    public static class ChatEndpoints
    {
        public static void MapChatEndpoints(this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapPost("/chat/join", async (HttpContext context, IChatRoomProvider chatRoomProvider, IChatRoomStorage chatRoomStorage, IChatUserProvider chatUserProvider, IChatSettings chatSettingsProvider, IMessengerPresenceProvider messengerProvider, ILogger<ChatEndpoints> logger) =>
            {
                var chatRoomId = context.Request.Form["chatRoomId"];
                var href = context.Request.Form["href"];
            
                if (href != null) context.Items["href"] = href;
            
                var result = new JoinChatRoomResult
                {
                    FileTransferEnabled = chatSettingsProvider.EnableFileTransfer,
                    VideoChatEnabled = chatSettingsProvider.EnableVideoChat,
                    FlashMediaServer = chatSettingsProvider.FlashMediaServer
                };
            
                User user;
                try
                {
                    user = chatUserProvider.GetCurrentlyLoggedUser();
                }
                catch (System.Security.SecurityException err)
                {
                    result.Error = err.Message;
                    await context.Response.WriteAsJsonAsync(result);
                    return;
                }
            
                if (user == null)
                {
                    string loginUrl = chatUserProvider.GetLoginUrl(href);
                    result.Error = "You need to login in order to use the chat!";
                    if (loginUrl != null)
                    {
                        result.Error += " Redirecting to login page...";
                        result.RedirectUrl = loginUrl;
                    }
                    await context.Response.WriteAsJsonAsync(result);
                    return;
                }
            
                Room room = chatRoomProvider.GetChatRoom(chatRoomId);
            
                if (room == null)
                {
                    result.Error = "The specified chat room does not exist!";
                    await context.Response.WriteAsJsonAsync(result);
                    return;
                }
            
                var url = new Uri(href);
                var queryKeyValues = HttpUtility.ParseQueryString(url.Query);
                var isInitiator = queryKeyValues["init"];
                var targetUserId = queryKeyValues["target"];
                bool alreadyConnected = href.IndexOf('#') == -1 ? false : (href.Substring(href.IndexOf('#')) == "#connected");
            
                string reason;
            
                if (room.Id == "-2")
                {
                    if (isInitiator == "1" && !chatRoomProvider.HasChatAccess(user.Id, room.Id, out reason))
                    {
                        result.Error = string.IsNullOrEmpty(reason) ? "You don't have access to the messenger." : reason;
                        await context.Response.WriteAsJsonAsync(result);
                        return;
                    }
                }
                else
                {
                    if (!chatRoomProvider.HasChatAccess(user.Id, room.Id, out reason))
                    {
                        result.Error = string.IsNullOrEmpty(reason) ? $"You don't have access to chat room '{room.Name}'." : reason;
                        await context.Response.WriteAsJsonAsync(result);
                        return;
                    }
                }
            
                if (room.Id != "-2" && chatRoomStorage.GetUsersInRoom(room.Id).Length >= room.MaxUsers)
                {
                    result.Error = $"The chat room '{room.Name}' is full. Please try again later.";
                    await context.Response.WriteAsJsonAsync(result);
                    return;
                }
            
                chatRoomStorage.AddUserToRoom(room.Id, user.Id);
                chatRoomStorage.AddMessage(room.Id, new Message
                {
                    Content = $"User {user.DisplayName} has joined the chat.",
                    FromUserId = user.Id,
                    ToUserId = string.IsNullOrEmpty(targetUserId) ? null : targetUserId,
                    MessageType = MessageTypeEnum.UserJoined,
                    Timestamp = Miscellaneous.GetTimestamp()
                });
            
                if (isInitiator != null && !alreadyConnected)
                {
                    chatRoomStorage.DeleteAllMessagesFor(room.Id, user.Id);
                }
            
                if (!alreadyConnected && isInitiator == "1" && !string.IsNullOrWhiteSpace(targetUserId))
                {
                    try
                    {
                        var targetUser = chatUserProvider.GetUser(targetUserId);
            
                        var timestamp = DateTime.Now.ToFileTimeUtc();
                        var hash = Miscellaneous.CalculateChatAuthHash(targetUserId, user.Id, timestamp.ToString());
            
                        var request = new ChatRequest
                        {
                            FromThumbnailUrl = user.ThumbnailUrl,
                            FromProfileUrl = user.ProfileUrl,
                            FromUserId = user.Id,
                            FromUsername = user.DisplayName,
                            ToUserId = targetUserId,
                            ToUsername = targetUser.DisplayName,
                            MessengerUrl = $"{url.GetLeftPart(UriPartial.Path)}?init=0&id={targetUserId}&target={user.Id}&timestamp={timestamp}&hash={hash}"
                        };
            
                        messengerProvider.AddChatRequest(request);
                    }
                    catch (System.Security.SecurityException) { }
                }
            
                result.ChatRoomName = room.Name;
                result.ChatRoomTopic = room.Topic;
                result.Users = chatRoomStorage.GetUsersInRoom(room.Id).Select(chatUserProvider.GetUser).ToArray();
                result.Token = chatRoomStorage.GenerateUserToken(user.Id);
                result.UserId = user.Id;
                result.IsAdmin = chatUserProvider.IsChatAdmin(user.Id, room.Id);
            
                StopVideoBroadcast(user.Id, room.Id);
                result.Broadcasts = chatRoomStorage.GetBroadcasts(room.Id, user.Id);
            
                await context.Response.WriteAsJsonAsync(result);
            });

            endpoints.MapPost("/chat/leave", async (HttpContext context, IChatRoomProvider chatRoomProvider, IChatRoomStorage chatRoomStorage, IChatUserProvider chatUserProvider, ILogger<ChatEndpoints> logger) =>
            {
                var chatRoomId = context.Request.Form["chatRoomId"];
                var token = context.Request.Form["token"];
                var messengerTargetUserId = context.Request.Form["messengerTargetUserId"];
            
                string userId = chatRoomStorage.GetUserIdByToken(token);
                if (userId == null) return;
                var user = chatUserProvider.GetUser(userId);
            
                Room room = chatRoomProvider.GetChatRoom(chatRoomId);
                if (room == null) return;
            
                if (chatRoomId != "-2")
                    chatRoomStorage.RemoveUserFromRoom(room.Id, user.Id);
            
                chatRoomStorage.AddMessage(room.Id, new Message
                {
                    Content = $"User {user.DisplayName} has left the chat.",
                    FromUserId = user.Id,
                    ToUserId = messengerTargetUserId,
                    MessageType = MessageTypeEnum.UserLeft,
                    Timestamp = Miscellaneous.GetTimestamp()
                });
            
                StopVideoBroadcast(userId, room.Id);
            
                var result = new { Success = true, Message = "Left chat room" };
                await context.Response.WriteAsJsonAsync(result);
            });

            endpoints.MapGet("/chat/events", async (HttpContext context, IChatRoomStorage chatRoomStorage, IChatUserProvider chatUserProvider, ILogger<ChatEndpoints> logger) =>
            {
                var chatRoomId = context.Request.Query["chatRoomId"];
                var token = context.Request.Query["token"];
                var fromTimestamp = long.Parse(context.Request.Query["fromTimestamp"]);
                var messengerTargetUserId = context.Request.Query["messengerTargetUserId"];
            
                var result = new EventsResult();
                string userId = chatRoomStorage.GetUserIdByToken(token);
                if (userId == null)
                {
                    result.Error = "Chat disconnected!";
                    await context.Response.WriteAsJsonAsync(result);
                    return;
                }
            
                chatRoomStorage.UpdateOnline(chatRoomId, userId);
            
                IEnumerable<Message> messages = chatRoomStorage.GetMessages(chatRoomId, userId, fromTimestamp);
            
                if (messengerTargetUserId != null)
                {
                    messages = messages.Where(m => m.FromUserId == userId || m.FromUserId == messengerTargetUserId);
                }
            
                result.Messages = messages.Where(m => !chatUserProvider.IsUserIgnored(userId, m.FromUserId) ||
                                                      m.MessageType == MessageTypeEnum.UserJoined ||
                                                      m.MessageType == MessageTypeEnum.UserLeft ||
                                                      m.MessageType == MessageTypeEnum.Kicked).ToArray();
            
                var joinMessages = messages.Where(m => m.MessageType == MessageTypeEnum.UserJoined);
                var leaveMessages = messages.Where(m => m.MessageType == MessageTypeEnum.UserLeft || m.MessageType == MessageTypeEnum.Kicked);
            
                var joinedUsers = joinMessages.Where(j => !leaveMessages.Any(l => l.FromUserId == j.FromUserId && l.Timestamp > j.Timestamp)).Select(
                                    m => chatUserProvider.GetUser(m.FromUserId)).ToArray();
                var leftUsers = leaveMessages.Where(l => !joinMessages.Any(j => j.FromUserId == l.FromUserId && j.Timestamp > l.Timestamp)).Select(
                                    m => chatUserProvider.GetUser(m.FromUserId)).ToArray();
            
                result.UsersJoined = joinedUsers;
                result.UsersLeft = leftUsers;
            
                if (context.RequestServices.GetService<IConfiguration>()["PollingInterval"] != null)
                    result.CallInterval = Convert.ToInt32(context.RequestServices.GetService<IConfiguration>()["PollingInterval"]);
            
                await context.Response.WriteAsJsonAsync(result);
            });

            endpoints.MapPost("/chat/message", async (HttpContext context, IChatRoomStorage chatRoomStorage, IChatUserProvider chatUserProvider, ILogger<ChatEndpoints> logger) =>
            {
                var chatRoomId = context.Request.Form["chatRoomId"];
                var token = context.Request.Form["token"];
                var toUserId = context.Request.Form["toUserId"];
                var message = context.Request.Form["message"];
                var bold = bool.Parse(context.Request.Form["bold"]);
                var italic = bool.Parse(context.Request.Form["italic"]);
                var underline = bool.Parse(context.Request.Form["underline"]);
                var fontName = context.Request.Form["fontName"];
                var fontSize = int.Parse(context.Request.Form["fontSize"]);
                var color = context.Request.Form["color"];
            
                var result = new SendMessageResult();
                string userId = chatRoomStorage.GetUserIdByToken(token);
                if (userId == null)
                {
                    result.Error = "Chat disconnected!";
                    await context.Response.WriteAsJsonAsync(result);
                    return;
                }
            
                if (!chatRoomStorage.IsUserInRoom(chatRoomId, userId))
                {
                    result.Error = "Chat disconnected!";
                    await context.Response.WriteAsJsonAsync(result);
                    return;
                }
            
                if (!string.IsNullOrEmpty(color))
                    color = Regex.Replace(color, @"[^\w\#]", string.Empty);
            
                var formatOptions = new MessageFormatOptions
                {
                    Bold = bold,
                    Italic = italic,
                    Underline = underline,
                    Color = color,
                    FontName = fontName,
                    FontSize = fontSize
                };
            
                chatRoomStorage.AddMessage(chatRoomId, new Message
                {
                    Content = WebUtility.HtmlEncode(message),
                    FromUserId = userId,
                    ToUserId = toUserId,
                    MessageType = MessageTypeEnum.User,
                    Timestamp = Miscellaneous.GetTimestamp(),
                    FormatOptions = formatOptions
                });
            
                await context.Response.WriteAsJsonAsync(result);
            });

            endpoints.MapPost("/chat/command", async (HttpContext context, IChatRoomProvider chatRoomProvider, IChatRoomStorage chatRoomStorage, IChatUserProvider chatUserProvider, ILogger<ChatEndpoints> logger) =>
            {
                var chatRoomId = context.Request.Form["chatRoomId"];
                var token = context.Request.Form["token"];
                var targetUserId = context.Request.Form["targetUserId"];
                var command = context.Request.Form["command"];
            
                var result = new SendMessageResult();
                string userId = chatRoomStorage.GetUserIdByToken(token);
                if (userId == null)
                {
                    result.Error = "Chat disconnected!";
                    await context.Response.WriteAsJsonAsync(result);
                    return;
                }
            
                if (!chatRoomStorage.IsUserInRoom(chatRoomId, userId))
                {
                    result.Error = "Chat disconnected!";
                    await context.Response.WriteAsJsonAsync(result);
                    return;
                }
            
                var user = chatUserProvider.GetUser(userId);
                var targetUser = chatUserProvider.GetUser(targetUserId);
            
                if (command == "ignore")
                {
                    chatUserProvider.IgnoreUser(userId, targetUserId);
                }
                else if (command == "kick")
                {
                    if (chatUserProvider.IsChatAdmin(userId, chatRoomId) && chatRoomStorage.IsUserInRoom(chatRoomId, targetUserId))
                    {
                        chatRoomStorage.RemoveUserFromRoom(chatRoomId, targetUserId);
            
                        chatRoomStorage.AddMessage(chatRoomId, new Message
                        {
                            Content = $"User {targetUser.DisplayName} has been kicked off the room by {user.DisplayName}.",
                            FromUserId = targetUserId,
                            MessageType = MessageTypeEnum.Kicked,
                            Timestamp = Miscellaneous.GetTimestamp()
                        });
                    }
                }
                else if (command == "ban")
                {
                    if (chatUserProvider.IsChatAdmin(userId, chatRoomId) && chatRoomStorage.IsUserInRoom(chatRoomId, targetUserId))
                    {
                        chatRoomStorage.RemoveUserFromRoom(chatRoomId, targetUserId);
            
                        chatRoomStorage.AddMessage(chatRoomId, new Message
                        {
                            Content = $"User {targetUser.DisplayName} has been kicked off the room by {user.DisplayName} (Banned).",
                            FromUserId = targetUserId,
                            MessageType = MessageTypeEnum.Kicked,
                            Timestamp = Miscellaneous.GetTimestamp()
                        });
            
                        chatRoomProvider.BanUser(chatRoomId, userId, targetUserId);
                    }
                }
                else if (command == "slap")
                {
                    chatRoomStorage.AddMessage(chatRoomId, new Message
                    {
                        Content = $"{user.DisplayName} slaps {targetUser.DisplayName} around with a large trout.",
                        FromUserId = userId,
                        MessageType = MessageTypeEnum.System,
                        Timestamp = Miscellaneous.GetTimestamp()
                    });
                }
            
                await context.Response.WriteAsJsonAsync(result);
            });

            endpoints.MapPost("/chat/video/broadcast", async (HttpContext context, IChatRoomStorage chatRoomStorage, IChatSettings chatSettingsProvider, ILogger<ChatEndpoints> logger) =>
            {
                var prevGuid = context.Request.Form["prevGuid"];
                var token = context.Request.Form["token"];
                var chatRoomId = int.Parse(context.Request.Form["chatRoomId"]);
                var targetUserId = context.Request.Form["targetUserId"];
            
                if (!chatSettingsProvider.EnableVideoChat)
                {
                    await context.Response.WriteAsJsonAsync((string)null);
                    return;
                }
            
                string userId = chatRoomStorage.GetUserIdByToken(token);
                string guid = prevGuid ?? Guid.NewGuid().ToString();
            
                chatRoomStorage.AddMessage(chatRoomId.ToString(), new Message
                {
                    Content = guid,
                    FromUserId = userId,
                    ToUserId = targetUserId,
                    MessageType = MessageTypeEnum.VideoBroadcast,
                    Timestamp = Miscellaneous.GetTimestamp()
                });
            
                chatRoomStorage.RegisterBroadcast(chatRoomId.ToString(), userId, targetUserId, guid);
                await context.Response.WriteAsJsonAsync(guid);
            });

            endpoints.MapPost("/chat/video/stop", async (HttpContext context, IChatRoomStorage chatRoomStorage, ILogger<ChatEndpoints> logger) =>
            {
                var token = context.Request.Form["token"];
                var chatRoomId = int.Parse(context.Request.Form["chatRoomId"]);
            
                string userId = chatRoomStorage.GetUserIdByToken(token);
            
                StopVideoBroadcast(userId, chatRoomId.ToString());
            
                var result = new { Success = true, Message = "Video broadcast stopped" };
                await context.Response.WriteAsJsonAsync(result);
            });
        }
    }
}