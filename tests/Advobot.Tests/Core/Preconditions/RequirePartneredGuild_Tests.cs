using Advobot.Preconditions;
using Advobot.Tests.TestBases;
using Advobot.Tests.Utilities;

using Discord;

namespace Advobot.Tests.Core.Preconditions;

[TestClass]
public sealed class RequirePartneredGuild_Tests
	: Precondition_Tests<RequirePartneredGuild>
{
	protected override RequirePartneredGuild Instance { get; } = new();

	[TestMethod]
	public async Task IsNotPartnered_Test()
	{
		var result = await CheckPermissionsAsync().ConfigureAwait(false);
		Assert.IsFalse(result.IsSuccess);
	}

	[TestMethod]
	public async Task IsPartnered_Test()
	{
		Context.Guild.Features = new GuildFeaturesCreationArgs
		{
			Value = GuildFeature.Partnered,
		}.Build();

		var result = await CheckPermissionsAsync().ConfigureAwait(false);
		Assert.IsTrue(result.IsSuccess);
	}
}