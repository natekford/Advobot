using Discord;

namespace Advobot.Tests.Fakes.Discord.Users;

public sealed class FakeSelfUser : FakeUser, ISelfUser
{
	public string Email { get; set; }
	public UserProperties Flags { get; set; }
	public bool IsMfaEnabled { get; set; }
	public bool IsVerified { get; set; }
	public string Locale { get; set; }
	public PremiumType PremiumType { get; set; }

	public Task ModifyAsync(Action<SelfUserProperties> func, RequestOptions? options = null)
	{
		var args = new SelfUserProperties();
		func(args);

		Username = args.Username.GetValueOrDefault(Username);
		if (args.Avatar.IsSpecified || args.Banner.IsSpecified)
		{
			throw new NotImplementedException();
		}

		return Task.CompletedTask;
	}
}