using Advobot.Services.HelpEntries;
using Advobot.Tests.Fakes.Services.HelpEntries;
using Advobot.Tests.TestBases;
using Advobot.TypeReaders;

using AdvorangesUtils;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Advobot.Tests.Core.TypeReaders;

[TestClass]
public sealed class CommandCategory_Tests : TypeReader_Tests<CategoryTypeReader>
{
	private readonly HelpEntryService _Service = new();
	protected override CategoryTypeReader Instance { get; } = new();

	[TestMethod]
	public async Task Valid_Test()
	{
		_Service.Add(new FakeHelpEntry { Category = "i exist" });

		var result = await ReadAsync(_Service.GetCategories().First()).CAF();
		Assert.IsTrue(result.IsSuccess);
	}

	protected override void ModifyServices(IServiceCollection services)
	{
		services
			.AddSingleton<IHelpEntryService>(_Service);
	}
}