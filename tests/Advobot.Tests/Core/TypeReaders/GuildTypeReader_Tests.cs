using Advobot.Tests.TestBases;
using Advobot.TypeReaders.Discord;

using Discord;

namespace Advobot.Tests.Core.TypeReaders;

[TestClass]
public sealed class GuildTypeReader_Tests : TypeReader_Tests<GuildTypeReader>
{
	protected override GuildTypeReader Instance { get; } = new();

	[TestMethod]
	public async Task ValidId_Test()
	{
		var result = await ReadAsync(Context.Guild.Id.ToString()).ConfigureAwait(false);
		Assert.IsTrue(result.InnerResult.IsSuccess);
		Assert.IsInstanceOfType<IGuild>(result.Value);
	}

	[TestMethod]
	public async Task ValidName_Test()
	{
		var result = await ReadAsync(Context.Guild.Name).ConfigureAwait(false);
		Assert.IsTrue(result.InnerResult.IsSuccess);
		Assert.IsInstanceOfType<IGuild>(result.Value);
	}
}