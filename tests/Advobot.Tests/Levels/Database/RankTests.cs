using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Advobot.Levels.Database;
using Advobot.Levels.Metadata;
using Advobot.Levels.Models;
using Advobot.Levels.ReadOnlyModels;
using Advobot.Levels.Utilities;
using AdvorangesUtils;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Advobot.Tests.Levels.Database
{
	[TestClass]
	public sealed class RankTests : DatabaseTestsBase
	{
		private const ulong CHANNEL_ID = 2;
		private const ulong GUILD_ID = 1;

		[TestMethod]
		public async Task RankPage_Test()
		{
			var db = await GetDatabaseAsync().CAF();
			var data = await SeedDataAsync(db).CAF();

			const int START = 1;
			const int LENGTH = 3;

			var ordered = data.OrderByDescending(x => x.Experience).ToArray();

			//Add a user to a different 'server' to allow checking if the total count stays
			//the same later on
			var last = ordered.Last();
			var otherServer = new User(new SearchArgs(last.UserId, CHANNEL_ID + 1, GUILD_ID + 1)).AddXp(3);
			await db.UpsertUser(otherServer).CAF();

			var expected = ordered
				.Skip(START)
				.Take(LENGTH)
				.Select((x, i) => new Rank(x.UserId, x.Experience, START + i, data.Count))
				.ToArray();

			var args = new SearchArgs(null, GUILD_ID, CHANNEL_ID);
			var retrieved = await db.GetRanksAsync(args, START, LENGTH).CAF();
			Assert.AreEqual(expected.Length, retrieved.Count);

			for (var i = 0; i < expected.Length; ++i)
			{
				var expectedItem = expected[i];
				var retrievedItem = retrieved[i];
				Assert.IsNotNull(retrievedItem);
				Assert.AreEqual(expectedItem.Experience, retrievedItem.Experience);
				Assert.AreEqual(expectedItem.Position, retrievedItem.Position);
				Assert.AreEqual(expectedItem.TotalRankCount, retrievedItem.TotalRankCount);
				Assert.AreEqual(expectedItem.UserId, retrievedItem.UserId);
			}
		}

		[TestMethod]
		public async Task SingleRank_Test()
		{
			var db = await GetDatabaseAsync().CAF();
			var data = await SeedDataAsync(db).CAF();

			var picked = data[0];
			var index = data.OrderByDescending(x => x.Experience).ToList().IndexOf(picked);
			var expected = new Rank(picked.UserId, picked.Experience, index + 1, data.Count);

			var args = new SearchArgs(picked.UserId, picked.GuildId, picked.ChannelId);
			var retrieved = await db.GetRankAsync(args).CAF();
			Assert.IsNotNull(retrieved);
			Assert.AreEqual(expected.Experience, retrieved.Experience);
			Assert.AreEqual(expected.Position, retrieved.Position);
			Assert.AreEqual(expected.TotalRankCount, retrieved.TotalRankCount);
			Assert.AreEqual(expected.UserId, retrieved.UserId);
		}

		private async Task<IReadOnlyList<IReadOnlyUser>> SeedDataAsync(LevelDatabase db)
		{
			var data = new[]
			{
				new User(new SearchArgs(1000, GUILD_ID, CHANNEL_ID)).AddXp(100),
				new User(new SearchArgs(2000, GUILD_ID, CHANNEL_ID)).AddXp(300),
				new User(new SearchArgs(3000, GUILD_ID, CHANNEL_ID)).AddXp(50),
				new User(new SearchArgs(4000, GUILD_ID, CHANNEL_ID)).AddXp(1000),
				new User(new SearchArgs(5000, GUILD_ID, CHANNEL_ID)).AddXp(250),
			};
			foreach (var datum in data)
			{
				await db.UpsertUser(datum).CAF();
			}
			return data;
		}
	}
}