using Advobot.Services.Help;
using Advobot.Tests.Fakes.Services.HelpEntries;
using Advobot.Tests.TestBases;
using Advobot.TypeReaders;

namespace Advobot.Tests.Core.TypeReaders;

[TestClass]
public sealed class CloseHelpEntryTypeReader_Tests
	: TypeReader_Tests<CloseHelpEntryTypeReader>
{
	protected override CloseHelpEntryTypeReader Instance { get; } = new();

	[TestMethod]
	public async Task Valid_Test()
	{
		foreach (var name in new[]
		{
			"dog",
			"bog",
			"pneumonoultramicroscopicsilicovolcanoconiosis"
		})
		{
			Help.Add(new FakeHelpEntry
			{
				Name = name,
			});
		}

		var result = await ReadAsync(Help.GetHelpModules(true).First().Name).ConfigureAwait(false);
		Assert.IsTrue(result.IsSuccess);
		Assert.IsInstanceOfType<IEnumerable<IHelpModule>>(result.BestMatch);
	}
}