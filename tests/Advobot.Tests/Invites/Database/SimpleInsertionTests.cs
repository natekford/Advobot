using System.Linq;
using System.Threading.Tasks;

using Advobot.Invites.Models;
using Advobot.Tests.Fakes.Discord;
using Advobot.Tests.Fakes.Discord.Channels;
using Advobot.Tests.Fakes.Discord.Users;

using AdvorangesUtils;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Advobot.Tests.Invites.Database
{
	[TestClass]
	public sealed class SimpleInsertionTests : DatabaseTestsBase
	{
		[TestMethod]
		public async Task InviteInsertionAndRetrieval_Test()
		{
			var db = await GetDatabaseAsync().CAF();

			var client = new FakeClient();
			var guild = new FakeGuild(client);
			var channel = new FakeTextChannel(guild);
			var user = new FakeGuildUser(guild);
			var invite = new FakeInviteMetadata(channel, user);

			var listedInvite = new ListedInvite(invite, Time.UtcNow);
			await db.AddInviteAsync(listedInvite).CAF();

			var retrieved = await db.GetInviteAsync(guild.Id).CAF();
			Assert.IsNotNull(retrieved);
			Assert.AreEqual(listedInvite.GuildId, retrieved.GuildId);
			Assert.AreEqual(listedInvite.Code, retrieved.Code);
			Assert.AreEqual(listedInvite.HasGlobalEmotes, retrieved.HasGlobalEmotes);
			Assert.AreEqual(listedInvite.LastBumped, retrieved.LastBumped);
			Assert.AreEqual(listedInvite.MemberCount, retrieved.MemberCount);
			Assert.AreEqual(listedInvite.Name, retrieved.Name);
		}

		[TestMethod]
		public async Task KeywordInsertionAndRetrieval_Test()
		{
			const string WORD = "bird";

			var db = await GetDatabaseAsync().CAF();

			var client = new FakeClient();
			var guild = new FakeGuild(client);

			var keyword = new Keyword(guild, WORD);
			await db.AddKeywordAsync(keyword).CAF();

			var retrievedEnumerable = await db.GetKeywords(guild.Id).CAF();
			Assert.IsNotNull(retrievedEnumerable);
			var retrievedArray = retrievedEnumerable.ToArray();
			Assert.AreEqual(1, retrievedArray.Length);
			var retrieved = retrievedArray[0];
			Assert.IsNotNull(retrieved);
			Assert.AreEqual(keyword.GuildId, retrieved.GuildId);
			Assert.AreEqual(keyword.Word, retrieved.Word);
		}
	}
}