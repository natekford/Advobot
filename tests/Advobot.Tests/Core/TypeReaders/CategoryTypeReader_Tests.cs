using Advobot.Tests.Fakes.Services.HelpEntries;
using Advobot.Tests.TestBases;
using Advobot.TypeReaders;

namespace Advobot.Tests.Core.TypeReaders;

[TestClass]
public sealed class CommandCategory_Tests : TypeReader_Tests<CategoryTypeReader>
{
	protected override CategoryTypeReader Instance { get; } = new();

	[TestMethod]
	public async Task Valid_Test()
	{
		Help.Add(new FakeHelpEntry { Category = "i exist" });

		var result = await ReadAsync(Help.GetCategories().First()).ConfigureAwait(false);
		Assert.IsTrue(result.IsSuccess);
	}
}