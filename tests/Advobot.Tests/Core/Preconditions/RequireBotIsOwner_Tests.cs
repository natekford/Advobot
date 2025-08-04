using Advobot.Preconditions;
using Advobot.Tests.TestBases;

using AdvorangesUtils;

namespace Advobot.Tests.Core.Preconditions;

[TestClass]
public sealed class RequireBotIsOwner_Tests : Precondition_Tests<RequireBotIsOwner>
{
	protected override RequireBotIsOwner Instance { get; } = new();

	[TestMethod]
	public async Task BotIsNotOwner_Test()
	{
		var result = await CheckPermissionsAsync().CAF();
		Assert.IsFalse(result.IsSuccess);
	}

	[TestMethod]
	public async Task BotIsOwner_Test()
	{
		Context.Guild.FakeOwner = Context.Guild.FakeCurrentUser;

		var result = await CheckPermissionsAsync().CAF();
		Assert.IsTrue(result.IsSuccess);
	}
}