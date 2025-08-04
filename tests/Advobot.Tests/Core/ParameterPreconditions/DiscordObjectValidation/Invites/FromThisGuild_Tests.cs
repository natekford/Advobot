using Advobot.ParameterPreconditions.DiscordObjectValidation.Invites;
using Advobot.Tests.Fakes.Discord;
using Advobot.Tests.Fakes.Discord.Channels;
using Advobot.Tests.Fakes.Discord.Users;
using Advobot.Tests.TestBases;

using AdvorangesUtils;

namespace Advobot.Tests.Core.ParameterPreconditions.DiscordObjectValidation.Invites;

[TestClass]
public sealed class FromThisGuild_Tests : ParameterPrecondition_Tests<FromThisGuild>
{
	protected override FromThisGuild Instance { get; } = new();

	[TestMethod]
	public async Task FromThisGuild_Test()
	{
		var invite = new FakeInviteMetadata(Context.Channel, Context.User);

		await AssertSuccessAsync(invite).CAF();
	}

	[TestMethod]
	public async Task NotFromThisGuild_Test()
	{
		var guild = new FakeGuild(Context.Client);
		var channel = new FakeTextChannel(guild);
		var user = new FakeGuildUser(guild);
		var invite = new FakeInviteMetadata(channel, user);

		await AssertFailureAsync(invite).CAF();
	}
}