using System.Threading.Tasks;

using Advobot.Gacha.Database;
using Advobot.Gacha.Models;
using Advobot.Gacha.ReadOnlyModels;
using Advobot.Gacha.TypeReaders;
using Advobot.Gacha.Utilities;
using Advobot.Tests.TestBases;

using AdvorangesUtils;

using Discord.Commands;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Advobot.Tests.Commands.Gacha.TypeReaders
{
	[TestClass]
	public sealed class SourceTypeReader_Tests : TypeReaderTestsBase
	{
		private readonly FakeGachaDatabase _Db = new();
		protected override TypeReader Instance { get; } = new SourceTypeReader();

		[TestMethod]
		public async Task InvalidMultipleMatches_Test()
		{
			var sources = new[]
			{
				GenerateStaticSource("Gamers!"),
				GenerateStaticSource("Gamers!"),
			};
			await _Db.AddSourcesAsync(sources).CAF();

			var result = await ReadAsync(sources[0].Name).CAF();
			Assert.IsFalse(result.IsSuccess);
		}

		[TestMethod]
		public async Task Valid_Test()
		{
			var sources = new[]
			{
				GenerateStaticSource("Gamers!"),
				GenerateStaticSource("not Gamers!"),
			};
			await _Db.AddSourcesAsync(sources).CAF();

			var result = await ReadAsync(sources[0].Name).CAF();
			Assert.IsTrue(result.IsSuccess);
		}

		protected override void ModifyServices(IServiceCollection services)
		{
			services
				.AddSingleton<IGachaDatabase>(_Db);
		}

		private IReadOnlySource GenerateStaticSource(string name)
		{
			return new Source
			{
				SourceId = TimeUtils.UtcNowTicks,
				Name = name,
			};
		}
	}
}