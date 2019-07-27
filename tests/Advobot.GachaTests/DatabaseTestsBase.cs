using Advobot.Gacha.Database;
using Advobot.Gacha.Models;
using Advobot.GachaTests.Utilities;
using AdvorangesUtils;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Advobot.GachaTests
{
	public abstract class DatabaseTestsBase
	{
		protected IServiceProvider Provider { get; }
		protected Random Rng { get; } = new Random();

		public DatabaseTestsBase()
		{
			Provider = new ServiceCollection()
				.AddSingleton<GachaDatabase>()
				.AddSingleton<IDatabaseStarter, SQLiteTestDatabaseFactory>()
				.BuildServiceProvider();
		}

		protected async Task<GachaDatabase> GetDatabaseAsync()
		{
			var db = Provider.GetRequiredService<GachaDatabase>();
#pragma warning disable IDE0059 // Value assigned to symbol is never used
			var tables = await db.CreateDatabaseAsync().CAF();
#pragma warning restore IDE0059 // Value assigned to symbol is never used
			return db;
		}
		protected Source GenerateFakeSource(long sourceId = 1)
		{
			return new Source
			{
				SourceId = sourceId,
				Name = Guid.NewGuid().ToString(),
			};
		}
		protected Character GenerateFakeCharacter(Source fakeSource, long characterId = 1)
		{
			return new Character(fakeSource)
			{
				CharacterId = characterId,
				Name = Guid.NewGuid().ToString(),
				GenderIcon = "\uD83D\uDE39",
				Gender = Gender.Other,
				RollType = RollType.All,
				IsFakeCharacter = true,
			};
		}
		protected User GenerateFakeUser(ulong? userId = null, ulong? guildId = null)
		{
			return new User
			{
				UserId = (userId ?? Rng.NextUlong()).ToString(),
				GuildId = (guildId ?? Rng.NextUlong()).ToString(),
			};
		}
		protected Claim GenerateFakeClaim(User user, Character character)
		{
			return new Claim(user, character)
			{
				IsPrimaryClaim = Rng.NextBool(),
			};
		}
		protected Wish GenerateFakeWish(User user, Character character)
		{
			return new Wish(user, character)
			{

			};
		}
	}
}
