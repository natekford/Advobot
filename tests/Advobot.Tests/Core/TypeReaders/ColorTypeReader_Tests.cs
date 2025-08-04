using Advobot.Tests.TestBases;
using Advobot.TypeReaders;

using AdvorangesUtils;

using Discord;

namespace Advobot.Tests.Core.TypeReaders;

[TestClass]
public sealed class ColorTypeReader_Tests : TypeReader_Tests<ColorTypeReader>
{
	protected override ColorTypeReader Instance { get; } = new();

	[TestMethod]
	public async Task ValidEmpty_Test()
	{
		var result = await ReadAsync(null).CAF();
		Assert.IsTrue(result.IsSuccess);
		Assert.IsInstanceOfType(result.BestMatch, typeof(Color));
	}

	[TestMethod]
	public async Task ValidHex_Test()
	{
		var result = await ReadAsync(Color.Red.RawValue.ToString("X6")).CAF();
		Assert.IsTrue(result.IsSuccess);
		Assert.IsInstanceOfType(result.BestMatch, typeof(Color));
	}

	[TestMethod]
	public async Task ValidName_Test()
	{
		var result = await ReadAsync("Red").CAF();
		Assert.IsTrue(result.IsSuccess);
		Assert.IsInstanceOfType(result.BestMatch, typeof(Color));
	}

	[TestMethod]
	public async Task ValidRGB_Test()
	{
		var result = await ReadAsync("100/100/100").CAF();
		Assert.IsTrue(result.IsSuccess);
		Assert.IsInstanceOfType(result.BestMatch, typeof(Color));
	}
}