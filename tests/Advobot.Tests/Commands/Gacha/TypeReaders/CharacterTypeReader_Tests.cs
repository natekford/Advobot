using System.Threading.Tasks;

using Advobot.Gacha.Database;
using Advobot.Gacha.Models;
using Advobot.Gacha.ReadOnlyModels;
using Advobot.Gacha.TypeReaders;
using Advobot.Gacha.Utilities;
using Advobot.Tests.Commands.Gacha.Fakes;
using Advobot.Tests.Commands.Gacha.Utilities;
using Advobot.Tests.Core.TypeReaders;

using AdvorangesUtils;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Advobot.Tests.Commands.Gacha.TypeReaders
{
	[TestClass]
	public sealed class CharacterTypeReader_Tests
		: TypeReader_TestsBase<CharacterTypeReader>
	{
		public const string NAME = "bobby";

		public CharacterTypeReader_Tests()
		{
			Services = new ServiceCollection()
				.AddSingleton<IGachaDatabase, FakeGachaDatabase>()
				.BuildServiceProvider();
		}

		[TestMethod]
		public async Task InvalidCharacter_Test()
		{
			var result = await ReadAsync("139284ahsdfnq1g2oasdf-09jasdf[ asdf 1234hansdfasdf").CAF();
			Assert.IsFalse(result.IsSuccess);
		}

		[TestMethod]
		public async Task InvalidMultipleMatches_Test()
		{
			var db = Services.GetRequiredService<IGachaDatabase>();

			var source = GachaTestUtils.GenerateFakeSource();
			var characters = new[]
			{
				GenerateStaticCharacter(source),
				GenerateStaticCharacter(source),
			};
			await db.AddSourceAsync(source).CAF();
			await db.AddCharactersAsync(characters).CAF();

			var result = await ReadAsync(NAME).CAF();
			Assert.IsFalse(result.IsSuccess);
		}

		[TestMethod]
		public async Task Valid_Test()
		{
			var db = Services.GetRequiredService<IGachaDatabase>();

			var source = GachaTestUtils.GenerateFakeSource();
			var characters = new[]
			{
				GenerateStaticCharacter(source),
				GenerateStaticCharacter(source, "not bobby"),
			};
			await db.AddSourceAsync(source).CAF();
			await db.AddCharactersAsync(characters).CAF();

			var result = await ReadAsync(NAME).CAF();
			Assert.IsTrue(result.IsSuccess);
		}

		private IReadOnlyCharacter GenerateStaticCharacter(IReadOnlySource fakeSource, string? name = null)
		{
			return new Character(fakeSource)
			{
				CharacterId = TimeUtils.UtcNowTicks,
				Name = name ?? NAME,
				GenderIcon = "\uD83D\uDE39",
				Gender = Gender.Other,
				RollType = RollType.All,
				IsFakeCharacter = true,
			};
		}
	}

	[TestClass]
	public sealed class SourceTypeReader_Tests
		: TypeReader_TestsBase<SourceTypeReader>
	{
		public const string NAME = "Gamers!";

		public SourceTypeReader_Tests()
		{
			Services = new ServiceCollection()
				.AddSingleton<IGachaDatabase, FakeGachaDatabase>()
				.BuildServiceProvider();
		}

		[TestMethod]
		public async Task InvalidCharacter_Test()
		{
			var result = await ReadAsync("139284ahsdfnq1g2oasdf-09jasdf[ asdf 1234hansdfasdf").CAF();
			Assert.IsFalse(result.IsSuccess);
		}

		[TestMethod]
		public async Task InvalidMultipleMatches_Test()
		{
			var db = Services.GetRequiredService<IGachaDatabase>();

			var sources = new[]
			{
				GenerateStaticSource(),
				GenerateStaticSource(),
			};
			await db.AddSourcesAsync(sources).CAF();

			var result = await ReadAsync(NAME).CAF();
			Assert.IsFalse(result.IsSuccess);
		}

		[TestMethod]
		public async Task Valid_Test()
		{
			var db = Services.GetRequiredService<IGachaDatabase>();

			var sources = new[]
			{
				GenerateStaticSource(),
				GenerateStaticSource("not Gamers!"),
			};
			await db.AddSourcesAsync(sources).CAF();

			var result = await ReadAsync(NAME).CAF();
			Assert.IsTrue(result.IsSuccess);
		}

		private IReadOnlySource GenerateStaticSource(string? name = null)
		{
			return new Source
			{
				SourceId = TimeUtils.UtcNowTicks,
				Name = name ?? NAME,
			};
		}
	}
}