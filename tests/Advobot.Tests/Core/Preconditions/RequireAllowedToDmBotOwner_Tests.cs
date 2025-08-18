using Advobot.Preconditions;
using Advobot.Services.BotConfig;
using Advobot.Tests.Fakes.Services.BotConfig;
using Advobot.Tests.TestBases;

using Microsoft.Extensions.DependencyInjection;

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