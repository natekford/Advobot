using System.Threading.Tasks;

using Advobot.Gacha.Database;
using Advobot.Gacha.Models;
using Advobot.Gacha.ReadOnlyModels;
using Advobot.Gacha.TypeReaders;
using Advobot.Gacha.Utilities;
using Advobot.Tests.Commands.Gacha.Fakes;
using Advobot.Tests.Commands.Gacha.Utilities;
using Advobot.Tests.TestBases;

using AdvorangesUtils;

using Discord.Commands;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Advobot.Tests.Commands.Gacha.TypeReaders
{
	[TestClass]
	public sealed class CharacterTypeReader_Tests : TypeReaderTestsBase
	{
		private readonly FakeGachaDatabase _Db = new FakeGachaDatabase();
		protected override TypeReader Instance { get; } = new CharacterTypeReader();

		[TestMethod]
		public async Task InvalidMultipleMatches_Test()
		{
			var source = GachaTestUtils.GenerateFakeSource();
			var characters = new[]
			{
				GenerateStaticCharacter(source, "bobby"),
				GenerateStaticCharacter(source, "bobby"),
			};
			await _Db.AddSourceAsync(source).CAF();
			await _Db.AddCharactersAsync(characters).CAF();

			var result = await ReadAsync(characters[0].Name).CAF();
			Assert.IsFalse(result.IsSuccess);
		}

		[TestMethod]
		public async Task Valid_Test()
		{
			var source = GachaTestUtils.GenerateFakeSource();
			var characters = new[]
			{
				GenerateStaticCharacter(source, "bobby"),
				GenerateStaticCharacter(source, "not bobby"),
			};
			await _Db.AddSourceAsync(source).CAF();
			await _Db.AddCharactersAsync(characters).CAF();

			var result = await ReadAsync(characters[0].Name).CAF();
			Assert.IsTrue(result.IsSuccess);
		}

		protected override void ModifyServices(IServiceCollection services)
		{
			services
				.AddSingleton<IGachaDatabase>(_Db);
		}

		private IReadOnlyCharacter GenerateStaticCharacter(IReadOnlySource fakeSource, string name)
		{
			return new Character(fakeSource)
			{
				CharacterId = TimeUtils.UtcNowTicks,
				Name = name,
				GenderIcon = "\uD83D\uDE39",
				Gender = Gender.Other,
				RollType = RollType.All,
				IsFakeCharacter = true,
			};
		}
	}
}