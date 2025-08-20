using Advobot.Preconditions;
using Advobot.Tests.TestBases;

namespace Advobot.Tests.Core.Preconditions;

[TestClass]
public sealed class RequireAllowedToDmBotOwner_Tests
	: Precondition_Tests<RequireAllowedToDmBotOwner>
{
	protected override RequireAllowedToDmBotOwner Instance { get; } = new();

	[TestMethod]
	public async Task CanDmBotOwner_Test()
	{
		var result = await CheckPermissionsAsync().ConfigureAwait(false);
		Assert.IsTrue(result.IsSuccess);
	}

	[TestMethod]
	public async Task CannotDmBotOwner_Test()
	{
		Config.UsersUnableToDmOwner.Add(Context.User.Id);

		var result = await CheckPermissionsAsync().ConfigureAwait(false);
		Assert.IsFalse(result.IsSuccess);
	}
}