using System.Threading.Tasks;

using Advobot.Services.HelpEntries;
using Advobot.Tests.Fakes.Services.HelpEntries;
using Advobot.TypeReaders;

using AdvorangesUtils;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Advobot.Tests.Core.TypeReaders
{
	[TestClass]
	public sealed class HelpEntryTypeReader_Tests
		: TypeReader_TestsBase<HelpEntryTypeReader>
	{
		private static readonly string[] NAMES = new[]
		{
			"dog",
			"bog",
			"pneumonoultramicroscopicsilicovolcanoconiosis"
		};

		public HelpEntryTypeReader_Tests()
		{
			var helpEntries = new HelpEntryService();
			foreach (var name in NAMES)
			{
				helpEntries.Add(new FakeHelpEntry
				{
					Name = name,
				});
			}

			Services = new ServiceCollection()
				.AddSingleton<IHelpEntryService>(helpEntries)
				.BuildServiceProvider();
		}

		[TestMethod]
		public async Task Invalid_Test()
		{
			var result = await ReadAsync("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa").CAF();
			Assert.IsFalse(result.IsSuccess);
		}

		[TestMethod]
		public async Task Valid_Test()
		{
			var result = await ReadAsync(NAMES[0]).CAF();
			Assert.IsTrue(result.IsSuccess);
			Assert.IsInstanceOfType(result.BestMatch, typeof(IModuleHelpEntry));
		}
	}
}