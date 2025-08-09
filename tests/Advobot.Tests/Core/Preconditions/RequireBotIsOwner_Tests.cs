using Advobot.Preconditions;
using Advobot.Tests.TestBases;

namespace Advobot.Tests.Core.Preconditions;

[TestClass]
public sealed class RequireBotIsOwner_Tests : Precondition_Tests<RequireBotIsOwner>
{
	protected override RequireBotIsOwner Instance { get; } = new();

	[TestMethod]
	public async Task BotIsNotOwner_Test()
	{
		var result = await CheckPermissionsAsync().ConfigureAwait(false);
		Assert.IsFalse(result.IsSuccess);
	}

	[TestMethod]
	public async Task BotIsOwner_Test()
	{
		Context.Guild.FakeOwner = Context.Guild.FakeCurrentUser;

		var result = await CheckPermissionsAsync().ConfigureAwait(false);
		Assert.IsTrue(result.IsSuccess);
	}
}