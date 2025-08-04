using Advobot.Preconditions;
using Advobot.Tests.TestBases;

using AdvorangesUtils;

namespace Advobot.Tests.Core.Preconditions;

[TestClass]
public sealed class RequireGuildOwner_Tests : Precondition_Tests<RequireGuildOwner>
{
	protected override RequireGuildOwner Instance { get; } = new();

	[TestMethod]
	public async Task InvokerIsNotOwner_Test()
	{
		var result = await CheckPermissionsAsync().CAF();
		Assert.IsFalse(result.IsSuccess);
	}

	[TestMethod]
	public async Task InvokerIsOwner_Test()
	{
		Context.Guild.FakeOwner = Context.User;

		var result = await CheckPermissionsAsync().CAF();
		Assert.IsTrue(result.IsSuccess);
	}
}