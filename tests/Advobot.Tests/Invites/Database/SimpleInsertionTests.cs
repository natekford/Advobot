using System.Linq;
using System.Threading.Tasks;

using Advobot.Invites.Models;
using Advobot.Invites.ReadOnlyModels;
using Advobot.Tests.Fakes.Discord;

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
			var (Guild, Invite) = CreateFakeInvite(client, Time);
			await db.AddInviteAsync(Invite).CAF();

			var retrieved = await db.GetInviteAsync(Guild.Id).CAF();
			Assert.IsNotNull(retrieved);
			Assert.AreEqual(Invite.GuildId, retrieved.GuildId);
			Assert.AreEqual(Invite.Code, retrieved.Code);
			Assert.AreEqual(Invite.HasGlobalEmotes, retrieved.HasGlobalEmotes);
			Assert.AreEqual(Invite.LastBumped, retrieved.LastBumped);
			Assert.AreEqual(Invite.MemberCount, retrieved.MemberCount);
			Assert.AreEqual(Invite.Name, retrieved.Name);
		}

		[TestMethod]
		public async Task KeywordInsertionAndRetrieval_Test()
		{
			var db = await GetDatabaseAsync().CAF();

			var client = new FakeClient();
			var guild = new FakeGuild(client);

			var keyword = (IReadOnlyKeyword)new Keyword(guild, "bird");
			await db.AddKeywordAsync(keyword).CAF();

			var retrievedList = await db.GetKeywords(guild.Id).CAF();
			var retrieved = retrievedList.Single();
			Assert.IsNotNull(retrieved);
			Assert.AreEqual(keyword.GuildId, retrieved.GuildId);
			Assert.AreEqual(keyword.Word, retrieved.Word);
		}
	}
}