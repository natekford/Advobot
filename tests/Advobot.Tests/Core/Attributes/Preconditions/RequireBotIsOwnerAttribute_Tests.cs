using Advobot.Attributes.Preconditions;
using Advobot.Tests.TestBases;

using AdvorangesUtils;

using Discord.Commands;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Advobot.Tests.Core.Attributes.Preconditions;

[TestClass]
public sealed class RequireBotIsOwnerAttribute_Tests : PreconditionTestsBase
{
	protected override PreconditionAttribute Instance { get; }
		= new RequireBotIsOwnerAttribute();

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