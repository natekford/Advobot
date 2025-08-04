using Advobot.Preconditions;
using Advobot.Tests.TestBases;

using AdvorangesUtils;

namespace Advobot.Tests.Core.Preconditions;

[TestClass]
public sealed class RequireBotOwner_Tests : Precondition_Tests<RequireBotOwner>
{
	protected override RequireBotOwner Instance { get; } = new();

	[TestMethod]
	public async Task InvokerIsNotOwner_Test()
	{
		var result = await CheckPermissionsAsync().CAF();
		Assert.IsFalse(result.IsSuccess);
	}

	[TestMethod]
	public async Task InvokerIsOwner_Test()
	{
		Context.Client.FakeApplication.Owner = Context.User;

		var result = await CheckPermissionsAsync().CAF();
		Assert.IsTrue(result.IsSuccess);
	}
}