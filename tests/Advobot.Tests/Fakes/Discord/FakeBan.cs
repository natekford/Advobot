using Advobot.Tests.Fakes.Discord.Users;

using Discord;

namespace Advobot.Tests.Fakes.Discord;

public sealed class FakeBan : IBan
{
	public string Reason { get; set; }
	public IUser User { get; set; }

	public FakeBan(IUser user)
	{
		User = user;
	}

	public FakeBan(ulong id)
	{
		User = new FakeUser
		{
			Id = id,
		};
	}
}