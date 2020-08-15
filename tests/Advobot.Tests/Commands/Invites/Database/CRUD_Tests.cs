using System.Linq;
using System.Threading.Tasks;

using Advobot.Invites.Database;
using Advobot.Invites.Models;
using Advobot.Invites.ReadOnlyModels;
using Advobot.Tests.Fakes.Database;
using Advobot.Tests.Fakes.Discord;
using Advobot.Tests.TestBases;

using AdvorangesUtils;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Advobot.Tests.Commands.Invites.Database
{
	[TestClass]
	public sealed class CRUD_Tests
		: DatabaseTestsBase<InviteDatabase, FakeSQLiteConnectionString>
	{
		[TestMethod]
		public async Task InviteCRUD_Test()
		{
			var db = await GetDatabaseAsync().CAF();

			var client = new FakeClient();
			var (Guild, Invite) = client.CreateFakeInvite(Time);
			await db.AddInviteAsync(Invite).CAF();

			var retrieved = await db.GetInviteAsync(Guild.Id).CAF()!;
			Assert.IsNotNull(retrieved);
			Assert.AreEqual(Invite.GuildId, retrieved!.GuildId);
			Assert.AreEqual(Invite.Code, retrieved.Code);
			Assert.AreEqual(Invite.HasGlobalEmotes, retrieved.HasGlobalEmotes);
			Assert.AreEqual(Invite.LastBumped, retrieved.LastBumped);
			Assert.AreEqual(Invite.MemberCount, retrieved.MemberCount);
			Assert.AreEqual(Invite.Name, retrieved.Name);
		}

		[TestMethod]
		public async Task KeywordCRUD_Test()
		{
			var db = await GetDatabaseAsync().CAF();

			var client = new FakeClient();
			var guild = new FakeGuild(client);

			IReadOnlyKeyword keyword = new Keyword(guild, "bird");
			await db.AddKeywordAsync(keyword).CAF();

			var retrievedList = await db.GetKeywords(guild.Id).CAF();
			var retrieved = retrievedList.Single();
			Assert.IsNotNull(retrieved);
			Assert.AreEqual(keyword.GuildId, retrieved.GuildId);
			Assert.AreEqual(keyword.Word, retrieved.Word);
		}
	}
}