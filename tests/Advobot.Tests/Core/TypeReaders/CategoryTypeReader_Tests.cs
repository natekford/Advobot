using System.Linq;
using System.Threading.Tasks;

using Advobot.Services.HelpEntries;
using Advobot.Tests.Fakes.Services.HelpEntries;
using Advobot.Tests.TestBases;
using Advobot.TypeReaders;

using AdvorangesUtils;

using Discord.Commands;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Advobot.Tests.Core.TypeReaders
{
	[TestClass]
	public sealed class CommandCategoryAttribute_Tests : TypeReaderTestsBase
	{
		private readonly HelpEntryService _Service = new HelpEntryService();
		protected override TypeReader Instance { get; }
			= new CategoryTypeReader();

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
}