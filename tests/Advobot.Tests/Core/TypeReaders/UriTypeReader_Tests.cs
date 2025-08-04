using Advobot.Tests.TestBases;
using Advobot.TypeReaders;

using AdvorangesUtils;

namespace Advobot.Tests.Core.TypeReaders;

[TestClass]
public sealed class UriTypeReader_Tests : TypeReader_Tests<UriTypeReader>
{
	protected override UriTypeReader Instance { get; } = new();

	[TestMethod]
	public async Task Valid_Test()
	{
		var result = await ReadAsync("https://www.google.com").CAF();
		Assert.IsTrue(result.IsSuccess);
		Assert.IsInstanceOfType(result.BestMatch, typeof(Uri));
	}
}