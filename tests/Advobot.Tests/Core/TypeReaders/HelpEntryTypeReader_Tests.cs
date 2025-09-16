#warning reenable
/*
namespace Advobot.Tests.Core.TypeReaders;

[TestClass]
public sealed class HelpEntryTypeReader_Tests : TypeReader_Tests<HelpEntryTypeReader>
{
	protected override HelpEntryTypeReader Instance { get; } = new();

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
		Assert.IsInstanceOfType<IHelpModule>(result.BestMatch);
	}
}*/