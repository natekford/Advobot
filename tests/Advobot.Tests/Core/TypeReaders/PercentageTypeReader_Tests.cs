using Advobot.Tests.TestBases;
using Advobot.TypeReaders;

using AdvorangesUtils;

using ImageMagick;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Advobot.Tests.Core.TypeReaders;

[TestClass]
public sealed class PercentageTypeReader_Tests : TypeReader_Tests<PercentageTypeReader>
{
	protected override PercentageTypeReader Instance { get; } = new();

	[TestMethod]
	public async Task Valid_Test()
	{
		var result = await ReadAsync(0.75.ToString()).CAF();
		Assert.IsTrue(result.IsSuccess);
		Assert.IsInstanceOfType(result.BestMatch, typeof(Percentage));
	}
}