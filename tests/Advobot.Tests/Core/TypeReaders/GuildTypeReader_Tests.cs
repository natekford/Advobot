using Advobot.Tests.TestBases;
using Advobot.TypeReaders;

using AdvorangesUtils;

using Discord;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Advobot.Tests.Core.TypeReaders;

[TestClass]
public sealed class GuildTypeReader_Tests : TypeReader_Tests<GuildTypeReader>
{
	protected override GuildTypeReader Instance { get; } = new();

	[TestMethod]
	public async Task ValidId_Test()
	{
		var result = await ReadAsync(Context.Guild.Id.ToString()).CAF();
		Assert.IsTrue(result.IsSuccess);
		Assert.IsInstanceOfType(result.BestMatch, typeof(IGuild));
	}

	[TestMethod]
	public async Task ValidName_Test()
	{
		var result = await ReadAsync(Context.Guild.Name).CAF();
		Assert.IsTrue(result.IsSuccess);
		Assert.IsInstanceOfType(result.BestMatch, typeof(IGuild));
	}
}