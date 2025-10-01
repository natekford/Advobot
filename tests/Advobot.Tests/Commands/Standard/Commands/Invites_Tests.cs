using Advobot.Standard.Commands;
using Advobot.Tests.Fakes.Discord;
using Advobot.Tests.TestBases;

namespace Advobot.Tests.Commands.Standard.Commands;

[TestClass]
public sealed class Invites_Tests : Command_Tests
{
	[TestMethod]
	public async Task DeleteInvite_Test()
	{
		var invite = new FakeInviteMetadata(Context.Channel, Context.User);

		var input = $"{nameof(Invites.DeleteInvite)} {invite}";

		var result = await ExecuteWithResultAsync(input).ConfigureAwait(false);
		Assert.IsTrue(result.InnerResult.IsSuccess);
		Assert.IsEmpty(Context.Guild.FakeInvites);
	}
}