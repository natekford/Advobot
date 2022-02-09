using Advobot.Tests.Fakes.Discord;
using Advobot.Tests.TestBases;
using Advobot.TypeReaders;

using AdvorangesUtils;

using Discord;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Advobot.Tests.Core.TypeReaders;

[TestClass]
public sealed class VoiceRegionTypeReader_Tests : TypeReader_Tests<VoiceRegionTypeReader>
{
	protected override VoiceRegionTypeReader Instance { get; } = new();

	[TestMethod]
	public async Task Valid_Test()
	{
		var region = new FakeVoiceRegion
		{
			Name = "america",
		};
		Context.Client.FakeVoiceRegions.Add(region);

		var result = await ReadAsync(region.Name).CAF();
		Assert.IsTrue(result.IsSuccess);
		Assert.IsInstanceOfType(result.BestMatch, typeof(IVoiceRegion));
	}
}