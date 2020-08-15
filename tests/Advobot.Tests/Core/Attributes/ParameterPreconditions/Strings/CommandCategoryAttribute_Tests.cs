using System.Threading.Tasks;

using Advobot.Attributes.ParameterPreconditions.Strings;
using Advobot.Services.HelpEntries;
using Advobot.Tests.Fakes.Services.HelpEntries;
using Advobot.Tests.TestBases;

using AdvorangesUtils;

using Discord.Commands;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Advobot.Tests.Core.Attributes.ParameterPreconditions.Strings
{
	[TestClass]
	public sealed class CommandCategoryAttribute_Tests : ParameterPreconditionTestsBase
	{
		private readonly HelpEntryService _Service = new HelpEntryService();
		protected override ParameterPreconditionAttribute Instance { get; }
			= new CommandCategoryAttribute();

		[TestMethod]
		public async Task CategoryExisting_Test()
		{
			_Service.Add(new FakeHelpEntry { Category = "i exist" });

			var result = await CheckPermissionsAsync(_Service.GetCategories()[0]).CAF();
			Assert.IsTrue(result.IsSuccess);
		}

		[TestMethod]
		public async Task CategoryNotExisting_Test()
		{
			var result = await CheckPermissionsAsync("i dont exist").CAF();
			Assert.IsFalse(result.IsSuccess);
		}

		protected override void ModifyServices(IServiceCollection services)
		{
			services
				.AddSingleton<IHelpEntryService>(_Service);
		}
	}
}