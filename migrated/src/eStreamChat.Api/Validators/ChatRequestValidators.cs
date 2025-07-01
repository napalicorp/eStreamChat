using FluentValidation;
using eStreamChat.Api.Models.Requests;

namespace eStreamChat.Api.Validators;

public class JoinChatRoomRequestValidator : AbstractValidator<JoinChatRoomRequest>
{
    public JoinChatRoomRequestValidator()
    {
        RuleFor(x => x.ChatRoomId).NotEmpty();
        RuleFor(x => x.Href).NotEmpty();
    }
}

public class LeaveChatRoomRequestValidator : AbstractValidator<LeaveChatRoomRequest>
{
    public LeaveChatRoomRequestValidator()
    {
        RuleFor(x => x.ChatRoomId).NotEmpty();
        RuleFor(x => x.Token).NotEmpty();
    }
}

public class GetEventsRequestValidator : AbstractValidator<GetEventsRequest>
{
    public GetEventsRequestValidator()
    {
        RuleFor(x => x.ChatRoomId).NotEmpty();
        RuleFor(x => x.Token).NotEmpty();
        RuleFor(x => x.FromTimestamp).NotEmpty();
    }
}

public class SendMessageRequestValidator : AbstractValidator<SendMessageRequest>
{
    public SendMessageRequestValidator()
    {
        RuleFor(x => x.ChatRoomId).NotEmpty();
        RuleFor(x => x.Token).NotEmpty();
        RuleFor(x => x.Message).NotEmpty().MaximumLength(2000);
        RuleFor(x => x.FontSize).InclusiveBetween(8, 24).When(x => x.FontSize.HasValue);
        RuleFor(x => x.Color).Matches(@"^#?[0-9A-Fa-f]{6}$").When(x => !string.IsNullOrEmpty(x.Color));
    }
}

public class SendCommandRequestValidator : AbstractValidator<SendCommandRequest>
{
    public SendCommandRequestValidator()
    {
        RuleFor(x => x.ChatRoomId).NotEmpty();
        RuleFor(x => x.Token).NotEmpty();
        RuleFor(x => x.TargetUserId).NotEmpty();
        RuleFor(x => x.Command).NotEmpty()
            .Must(x => new[] { "ignore", "kick", "ban", "slap" }.Contains(x.ToLower()))
            .WithMessage("Invalid command. Allowed commands are: ignore, kick, ban, slap");
    }
}

public class BroadcastVideoRequestValidator : AbstractValidator<BroadcastVideoRequest>
{
    public BroadcastVideoRequestValidator()
    {
        RuleFor(x => x.ChatRoomId).NotEmpty();
        RuleFor(x => x.Token).NotEmpty();
    }
}

public class StopBroadcastRequestValidator : AbstractValidator<StopBroadcastRequest>
{
    public StopBroadcastRequestValidator()
    {
        RuleFor(x => x.ChatRoomId).NotEmpty();
        RuleFor(x => x.Token).NotEmpty();
    }
}