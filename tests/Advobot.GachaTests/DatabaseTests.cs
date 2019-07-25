using Advobot.Gacha.Database;
using Advobot.Gacha.Models;
using Advobot.GachaTests.Utils;
using AdvorangesUtils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;

namespace Advobot.GachaTests
{
	[TestClass]
	public class DatabaseTests
	{
		private readonly IServiceProvider _Provider;
		private readonly Random _Rng = new Random();

		public DatabaseTests()
		{
			_Provider = new ServiceCollection()
				.AddSingleton<GachaDatabase>()
				.AddSingleton<IDatabaseStarter, SQLiteTestDatabaseFactory>()
				.BuildServiceProvider();
		}

		private async Task<GachaDatabase> GetDatabaseAsync()
		{
			var db = _Provider.GetRequiredService<GachaDatabase>();
			var tables = await db.CreateDatabaseAsync().CAF();
			return db;
		}

		[TestMethod]
		public async Task UserInsertion_Test()
		{
			var db = await GetDatabaseAsync().CAF();

			var userId = _Rng.NextUlong();
			var guildId = _Rng.NextUlong();

			var user = new User
			{
				UserId = userId.ToString(),
				GuildId = guildId.ToString(),
			};
			await db.AddUserAsync(user).CAF();

			var retrieved = await db.GetUserAsync(guildId, userId).CAF();

			Assert.AreNotEqual(null, retrieved);
			Assert.AreEqual(user.UserId, retrieved.UserId);
			Assert.AreEqual(user.GuildId, retrieved.GuildId);
		}
	}
}
