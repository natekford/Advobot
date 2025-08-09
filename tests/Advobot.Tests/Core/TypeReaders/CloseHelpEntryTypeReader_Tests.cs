using Advobot.Services.HelpEntries;
using Advobot.Tests.Fakes.Services.HelpEntries;
using Advobot.Tests.TestBases;
using Advobot.TypeReaders;

using Microsoft.Extensions.DependencyInjection;

namespace Advobot.Tests.Core.TypeReaders;

[TestClass]
public sealed class CloseHelpEntryTypeReader_Tests
	: TypeReader_Tests<CloseHelpEntryTypeReader>
{
	private readonly HelpEntryService _HelpEntries = new();
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
			_HelpEntries.Add(new FakeHelpEntry
			{
				Name = name,
			});
		}

		var result = await ReadAsync(_HelpEntries.GetHelpEntries().First().Name).ConfigureAwait(false);
		Assert.IsTrue(result.IsSuccess);
		Assert.IsInstanceOfType(result.BestMatch, typeof(IEnumerable<IModuleHelpEntry>));
	}

	protected override void ModifyServices(IServiceCollection services)
	{
		services
			.AddSingleton<IHelpEntryService>(_HelpEntries);
	}
}