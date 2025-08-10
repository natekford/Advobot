using Advobot.Services.Help;
using Advobot.Tests.Fakes.Services.HelpEntries;
using Advobot.Tests.TestBases;
using Advobot.TypeReaders;

using Microsoft.Extensions.DependencyInjection;

namespace Advobot.Tests.Core.TypeReaders;

[TestClass]
public sealed class CommandCategory_Tests : TypeReader_Tests<CategoryTypeReader>
{
	private readonly NaiveHelpService _Service = new();
	protected override CategoryTypeReader Instance { get; } = new();

	[TestMethod]
	public async Task Valid_Test()
	{
		_Service.Add(new FakeHelpEntry { Category = "i exist" });

		var result = await ReadAsync(_Service.GetCategories().First()).ConfigureAwait(false);
		Assert.IsTrue(result.IsSuccess);
	}

	protected override void ModifyServices(IServiceCollection services)
	{
		services
			.AddSingleton<IHelpService>(_Service);
	}
}