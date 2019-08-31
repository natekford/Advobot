using System.Threading.Tasks;

using Advobot.Attributes.ParameterPreconditions.Strings;
using Advobot.Services.HelpEntries;
using Advobot.Tests.Fakes.Services.HelpEntries;

using AdvorangesUtils;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Advobot.Tests.Core.Attributes.ParameterPreconditions.Strings
{
	[TestClass]
	public sealed class CommandCategoryAttribute_Tests
		: ParameterPreconditionsTestsBase<CommandCategoryAttribute>
	{
		private readonly HelpEntryService _Service;

		public CommandCategoryAttribute_Tests()
		{
			_Service = new HelpEntryService();

			Services = new ServiceCollection()
				.AddSingleton<IHelpEntryService>(_Service)
				.BuildServiceProvider();
		}

		[TestMethod]
		public async Task CategoryExisting_Test()
		{
			const string CATEGORY = "i exist";

			_Service.Add(new FakeHelpEntry { Category = CATEGORY });

			var result = await CheckAsync(CATEGORY).CAF();
			Assert.AreEqual(true, result.IsSuccess);
		}

		[TestMethod]
		public async Task CategoryNotExisting_Test()
		{
			var result = await CheckAsync("i dont exist").CAF();
			Assert.AreEqual(false, result.IsSuccess);
		}

		[TestMethod]
		public async Task FailsOnNotString_Test()
			=> await AssertPreconditionFailsOnInvalidType(CheckAsync(1)).CAF();
	}
}