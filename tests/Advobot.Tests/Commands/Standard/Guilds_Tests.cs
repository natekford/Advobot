using Advobot.Standard.Commands;
using Advobot.Tests.Fakes.Discord;
using Advobot.Tests.TestBases;

namespace Advobot.Tests.Commands.Standard;

[TestClass]
public sealed class Guilds_Tests : Command_Tests
{
	[TestMethod]
	public async Task LeaveGuildCurrent_Test()
	{
		Context.Guild.FakeOwner = Context.User;

		const string INPUT = nameof(Guilds.LeaveGuild);

		var result = await ExecuteWithResultAsync(INPUT).ConfigureAwait(false);
		Assert.IsTrue(result.IsSuccess);
		Assert.IsEmpty(Context.Client.FakeGuilds);
	}

	[TestMethod]
	public async Task LeaveGuildRemote_Test()
	{
		Context.User.Id = Context.Client.FakeApplication.Owner.Id;
		var newGuild = new FakeGuild(Context.Client);

		var input = $"{nameof(Guilds.LeaveGuild)} {newGuild}";

		var result = await ExecuteWithResultAsync(input).ConfigureAwait(false);
		Assert.IsTrue(result.IsSuccess);
		Assert.DoesNotContain(newGuild, Context.Client.FakeGuilds);
		Assert.HasCount(1, Context.Client.FakeGuilds);
	}
}