using Advobot.Preconditions;
using Advobot.Tests.TestBases;

namespace Advobot.Tests.Core.Preconditions;

[TestClass]
public sealed class RequireGuildOwner_Tests : Precondition_Tests<RequireGuildOwner>
{
	protected override RequireGuildOwner Instance { get; } = new();

	[TestMethod]
	public async Task InvokerIsNotOwner_Test()
	{
		var result = await CheckPermissionsAsync().ConfigureAwait(false);
		Assert.IsFalse(result.IsSuccess);
	}

	[TestMethod]
	public async Task InvokerIsOwner_Test()
	{
		Context.Guild.FakeOwner = Context.User;

		var result = await CheckPermissionsAsync().ConfigureAwait(false);
		Assert.IsTrue(result.IsSuccess);
	}
}