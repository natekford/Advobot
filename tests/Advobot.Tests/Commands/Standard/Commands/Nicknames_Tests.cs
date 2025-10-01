using Advobot.Standard.Commands;
using Advobot.Tests.Fakes.Discord.Users;
using Advobot.Tests.TestBases;

namespace Advobot.Tests.Commands.Standard.Commands;

[TestClass]
public sealed class Nicknames_Tests : Command_Tests
{
	[TestMethod]
	public async Task RemoveAllNicknames_Test()
	{
		var user = new FakeGuildUser(Context.Guild)
		{
			Nickname = "asdf"
		};

		var input = $"{nameof(Nicknames.RemoveAllNickNames)} true";

		var result = await ExecuteWithResultAsync(input).ConfigureAwait(false);
		Assert.IsTrue(result.InnerResult.IsSuccess);
		Assert.IsNull(user.Nickname);
	}
}