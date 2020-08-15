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
	public sealed class HelpEntryTypeReader_Tests : TypeReaderTestsBase
	{
		private readonly HelpEntryService _HelpEntries = new HelpEntryService();
		protected override TypeReader Instance { get; } = new HelpEntryTypeReader();

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

			var result = await ReadAsync(_HelpEntries.GetHelpEntries()[0].Name).CAF();
			Assert.IsTrue(result.IsSuccess);
			Assert.IsInstanceOfType(result.BestMatch, typeof(IModuleHelpEntry));
		}

		protected override void ModifyServices(IServiceCollection services)
		{
			services
				.AddSingleton<IHelpEntryService>(_HelpEntries);
		}
	}
}