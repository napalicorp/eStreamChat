using FluentValidation;
using eStreamChat.Common.Services;
using Microsoft.OpenApi.Models;
using eStreamChat.Api.Middleware;
using eStreamChat.Api.Models.Requests;
using eStreamChat.Api.Models.Responses;
using eStreamChat.Api.Validators;
using eStreamChat.Interfaces;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "eStreamChat API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Chat token using the Bearer scheme",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Add validators
builder.Services.AddScoped<IValidator<JoinChatRoomRequest>, JoinChatRoomRequestValidator>();
builder.Services.AddScoped<IValidator<LeaveChatRoomRequest>, LeaveChatRoomRequestValidator>();
builder.Services.AddScoped<IValidator<GetEventsRequest>, GetEventsRequestValidator>();
builder.Services.AddScoped<IValidator<SendMessageRequest>, SendMessageRequestValidator>();
builder.Services.AddScoped<IValidator<SendCommandRequest>, SendCommandRequestValidator>();
builder.Services.AddScoped<IValidator<BroadcastVideoRequest>, BroadcastVideoRequestValidator>();
builder.Services.AddScoped<IValidator<StopBroadcastRequest>, StopBroadcastRequestValidator>();

// Add HttpContext accessor
builder.Services.AddHttpContextAccessor();

// Add services
builder.Services.AddScoped<IChatRoomProvider, ChatRoomProvider>();
builder.Services.AddScoped<IChatRoomStorage, ChatRoomStorage>();
builder.Services.AddScoped<IChatUserProvider, ChatUserProvider>();
builder.Services.AddScoped<IChatSettings, ChatSettings>();
builder.Services.AddScoped<IMessengerPresenceProvider, MessengerPresenceProvider>();

var app = builder.Build();

// Configure middleware
app.UseErrorHandling();
app.UseChatAuthentication();
app.UseSwagger();
app.UseSwaggerUI();

// Configure endpoints
var api = app.MapGroup("/api/chat");

api.MapPost("rooms/{chatRoomId}/join", async Task<IResult> (
    string chatRoomId,
    JoinChatRoomRequest request,
    IValidator<JoinChatRoomRequest> validator,
    IChatRoomProvider roomProvider,
    IChatRoomStorage roomStorage,
    IChatUserProvider userProvider,
    IChatSettings settings,
    IMessengerPresenceProvider? messengerProvider,
    HttpContext context) =>
{
    await validator.ValidateAndThrowAsync(request);

    if (request.Href != null) 
        context.Items["href"] = request.Href;

    var user = userProvider.GetCurrentlyLoggedUser();
    if (user == null)
    {
        var loginUrl = userProvider.GetLoginUrl(request.Href);
        return Results.BadRequest(new JoinChatRoomResponse
        {
            Error = loginUrl != null 
                ? "You need to login in order to use the chat! Redirecting to login page..." 
                : "You need to login in order to use the chat!",
            RedirectUrl = loginUrl,
            Users = Array.Empty<User>(),
            Broadcasts = new Dictionary<string, string>()
        });
    }

    var room = roomProvider.GetChatRoom(chatRoomId);
    if (room == null)
    {
        return Results.NotFound(new JoinChatRoomResponse
        {
            Error = "The specified chat room does not exist!",
            Users = Array.Empty<User>(),
            Broadcasts = new Dictionary<string, string>()
        });
    }

    string? reason;
    if (!roomProvider.HasChatAccess(user.Id, room.Id, out reason))
    {
        var unauthorizedResponse = new JoinChatRoomResponse
        {
            Error = string.IsNullOrEmpty(reason)
                ? $"You don't have access to chat room '{room.Name}'."
                : reason,
            Users = Array.Empty<User>(),
            Broadcasts = new Dictionary<string, string>()
        };
        return Results.Json(unauthorizedResponse, statusCode: StatusCodes.Status401Unauthorized);
    }

    if (room.Id != "-2" && roomStorage.GetUsersInRoom(room.Id).Length >= room.MaxUsers)
    {
        return Results.BadRequest(new JoinChatRoomResponse
        {
            Error = $"The chat room '{room.Name}' is full. Please try again later.",
            Users = Array.Empty<User>(),
            Broadcasts = new Dictionary<string, string>()
        });
    }

    roomStorage.AddUserToRoom(room.Id, user.Id);
    roomStorage.AddMessage(room.Id, new Message
    {
        Content = $"User {user.DisplayName} has joined the chat.",
        FromUserId = user.Id,
        MessageType = MessageType.UserJoined,
        Timestamp = DateTimeOffset.UtcNow
    });

    var response = new JoinChatRoomResponse
    {
        ChatRoomName = room.Name,
        ChatRoomTopic = room.Topic,
        Users = roomStorage.GetUsersInRoom(room.Id).Select(userProvider.GetUser).ToArray(),
        Token = roomStorage.GenerateUserToken(user.Id),
        UserId = user.Id,
        IsAdmin = userProvider.IsChatAdmin(user.Id, room.Id),
        FileTransferEnabled = settings.EnableFileTransfer,
        VideoChatEnabled = settings.EnableVideoChat,
        FlashMediaServer = settings.FlashMediaServer,
        Broadcasts = roomStorage.GetBroadcasts(room.Id, user.Id)
    };

    return Results.Ok(response);
})
.WithName("JoinChatRoom")
.WithOpenApi()
.AddEndpointFilter((context, next) => next(context)); // Skip authentication for this endpoint

api.MapPost("rooms/{chatRoomId}/leave", async Task<IResult> (
    string chatRoomId,
    LeaveChatRoomRequest request,
    IValidator<LeaveChatRoomRequest> validator,
    IChatRoomStorage roomStorage,
    IChatUserProvider userProvider,
    HttpContext context) =>
{
    await validator.ValidateAndThrowAsync(request);
    var userId = context.Items["UserId"] as string;

    var user = userProvider.GetUser(userId!);
    if (chatRoomId != "-2")
        roomStorage.RemoveUserFromRoom(chatRoomId, userId!);

    roomStorage.AddMessage(chatRoomId, new Message
    {
        Content = $"User {user.DisplayName} has left the chat.",
        FromUserId = userId!,
        ToUserId = request.MessengerTargetUserId,
        MessageType = MessageType.UserLeft,
        Timestamp = DateTimeOffset.UtcNow
    });

    return Results.Ok();
})
.WithName("LeaveChatRoom")
.WithOpenApi();

api.MapGet("rooms/{chatRoomId}/events", async Task<IResult> (
    string chatRoomId,
    [AsParameters] GetEventsRequest request,
    IValidator<GetEventsRequest> validator,
    IChatRoomStorage roomStorage,
    IChatUserProvider userProvider,
    HttpContext context) =>
{
    await validator.ValidateAndThrowAsync(request);
    var userId = context.Items["UserId"] as string;

    roomStorage.UpdateOnline(chatRoomId, userId!);

    var messages = roomStorage.GetMessages(chatRoomId, userId!, request.FromTimestamp);
    if (request.MessengerTargetUserId != null)
    {
        messages = messages.Where(m => m.FromUserId == userId || m.FromUserId == request.MessengerTargetUserId).ToArray();
    }

    var filteredMessages = messages.Where(m => 
        !userProvider.IsUserIgnored(userId!, m.FromUserId) ||
        m.MessageType == MessageType.UserJoined ||
        m.MessageType == MessageType.UserLeft ||
        m.MessageType == MessageType.Kicked).ToArray();

    var joinMessages = messages.Where(m => m.MessageType == MessageType.UserJoined);
    var leaveMessages = messages.Where(m => m.MessageType == MessageType.UserLeft || m.MessageType == MessageType.Kicked);

    var joinedUsers = joinMessages
        .Where(j => !leaveMessages.Any(l => l.FromUserId == j.FromUserId && l.Timestamp > j.Timestamp))
        .Select(m => userProvider.GetUser(m.FromUserId))
        .ToArray();

    var leftUsers = leaveMessages
        .Where(l => !joinMessages.Any(j => j.FromUserId == l.FromUserId && j.Timestamp > l.Timestamp))
        .Select(m => userProvider.GetUser(m.FromUserId))
        .ToArray();

    return Results.Ok(new GetEventsResponse
    {
        Messages = filteredMessages,
        UsersJoined = joinedUsers,
        UsersLeft = leftUsers,
        CallInterval = builder.Configuration.GetValue<int?>("PollingInterval")
    });
})
.WithName("GetEvents")
.WithOpenApi();

api.MapPost("rooms/{chatRoomId}/messages", async Task<IResult> (
    string chatRoomId,
    SendMessageRequest request,
    IValidator<SendMessageRequest> validator,
    IChatRoomStorage roomStorage,
    HttpContext context) =>
{
    await validator.ValidateAndThrowAsync(request);
    var userId = context.Items["UserId"] as string;

    if (!roomStorage.IsUserInRoom(chatRoomId, userId!))
        return Results.BadRequest(new SendMessageResponse { Error = "Chat disconnected!" });

    var message = new Message
    {
        Content = WebUtility.HtmlEncode(request.Message),
        FromUserId = userId!,
        ToUserId = request.ToUserId,
        MessageType = MessageType.User,
        Timestamp = DateTimeOffset.UtcNow,
        FormatOptions = new MessageFormatOptions
        {
            Bold = request.Bold,
            Italic = request.Italic,
            Underline = request.Underline,
            Color = request.Color,
            FontName = request.FontName,
            FontSize = request.FontSize ?? 0
        }
    };

    roomStorage.AddMessage(chatRoomId, message);
    return Results.Ok(new SendMessageResponse());
})
.WithName("SendMessage")
.WithOpenApi();

api.MapPost("rooms/{chatRoomId}/commands", async Task<IResult> (
    string chatRoomId,
    SendCommandRequest request,
    IValidator<SendCommandRequest> validator,
    IChatRoomStorage roomStorage,
    IChatUserProvider userProvider,
    IChatRoomProvider roomProvider,
    HttpContext context) =>
{
    await validator.ValidateAndThrowAsync(request);
    var userId = context.Items["UserId"] as string;

    if (!roomStorage.IsUserInRoom(chatRoomId, userId!))
        return Results.BadRequest(new SendMessageResponse { Error = "Chat disconnected!" });

    var user = userProvider.GetUser(userId!);
    var targetUser = userProvider.GetUser(request.TargetUserId);

    switch (request.Command.ToLower())
    {
        case "ignore":
            userProvider.IgnoreUser(userId!, request.TargetUserId);
            break;

        case "kick":
            if (userProvider.IsChatAdmin(userId!, chatRoomId) && roomStorage.IsUserInRoom(chatRoomId, request.TargetUserId))
            {
                roomStorage.RemoveUserFromRoom(chatRoomId, request.TargetUserId);
                roomStorage.AddMessage(chatRoomId, new Message
                {
                    Content = $"User {targetUser.DisplayName} has been kicked off the room by {user.DisplayName}.",
                    FromUserId = request.TargetUserId,
                    MessageType = MessageType.Kicked,
                    Timestamp = DateTimeOffset.UtcNow
                });
            }
            break;

        case "ban":
            if (userProvider.IsChatAdmin(userId!, chatRoomId) && roomStorage.IsUserInRoom(chatRoomId, request.TargetUserId))
            {
                roomStorage.RemoveUserFromRoom(chatRoomId, request.TargetUserId);
                roomStorage.AddMessage(chatRoomId, new Message
                {
                    Content = $"User {targetUser.DisplayName} has been kicked off the room by {user.DisplayName} (Banned).",
                    FromUserId = request.TargetUserId,
                    MessageType = MessageType.Kicked,
                    Timestamp = DateTimeOffset.UtcNow
                });
                roomProvider.BanUser(chatRoomId, userId!, request.TargetUserId);
            }
            break;

        case "slap":
            roomStorage.AddMessage(chatRoomId, new Message
            {
                Content = $"{user.DisplayName} slaps {targetUser.DisplayName} around with a large trout.",
                FromUserId = userId!,
                MessageType = MessageType.System,
                Timestamp = DateTimeOffset.UtcNow
            });
            break;
    }

    return Results.Ok(new SendMessageResponse());
})
.WithName("SendCommand")
.WithOpenApi();

api.MapPost("rooms/{chatRoomId}/broadcasts", async Task<IResult> (
    string chatRoomId,
    BroadcastVideoRequest request,
    IValidator<BroadcastVideoRequest> validator,
    IChatRoomStorage roomStorage,
    IChatSettings settings,
    HttpContext context) =>
{
    await validator.ValidateAndThrowAsync(request);
    var userId = context.Items["UserId"] as string;

    if (!settings.EnableVideoChat)
        return Results.BadRequest();

    var guid = request.PrevGuid ?? Guid.NewGuid().ToString();

    roomStorage.AddMessage(chatRoomId, new Message
    {
        Content = guid,
        FromUserId = userId!,
        ToUserId = request.TargetUserId,
        MessageType = MessageType.VideoBroadcast,
        Timestamp = DateTimeOffset.UtcNow
    });

    roomStorage.RegisterBroadcast(chatRoomId, userId!, request.TargetUserId, guid);
    return Results.Ok(new BroadcastVideoResponse { Guid = guid });
})
.WithName("BroadcastVideo")
.WithOpenApi();

api.MapDelete("rooms/{chatRoomId}/broadcasts", async Task<IResult> (
    string chatRoomId,
    StopBroadcastRequest request,
    IValidator<StopBroadcastRequest> validator,
    IChatRoomStorage roomStorage,
    HttpContext context) =>
{
    await validator.ValidateAndThrowAsync(request);
    var userId = context.Items["UserId"] as string;

    if (roomStorage.UnregisterUserBroadcasts(chatRoomId, userId!))
    {
        roomStorage.AddMessage(chatRoomId, new Message
        {
            FromUserId = userId!,
            MessageType = MessageType.StopVideoBroadcast,
            Timestamp = DateTimeOffset.UtcNow,
            Content = string.Empty // Add required Content property
        });
    }

    return Results.Ok();
})
.WithName("StopBroadcast")
.WithOpenApi();

app.Run();