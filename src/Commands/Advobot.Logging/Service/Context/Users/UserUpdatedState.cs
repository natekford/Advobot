using Discord;

namespace Advobot.Logging.Service.Context.Users;

public class UserUpdatedState(IUser before, IGuildUser user) : UserState(user)
{
	public IUser Before { get; } = before;
	public override bool IsValid => base.IsValid && !(User.IsBot || User.IsWebhook);
}