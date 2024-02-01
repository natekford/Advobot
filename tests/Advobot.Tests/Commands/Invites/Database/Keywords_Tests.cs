using Advobot.Invites.Database;
using Advobot.Invites.Models;
using Advobot.Tests.Fakes.Database;
using Advobot.Tests.Fakes.Discord;
using Advobot.Tests.TestBases;

using AdvorangesUtils;

using Discord;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Advobot.Tests.Commands.Invites.Database;

[TestClass]
public sealed class Keywords_Tests
	: Database_Tests<InviteDatabase, FakeSQLiteConnectionString>
{
	[TestMethod]
	public async Task KeywordInviteGathering_Test()
	{
		var db = await GetDatabaseAsync().CAF();

		var client = new FakeClient();
		var counts = new Dictionary<string, HashSet<ulong>>();
		var keywords = new List<Keyword>();

		const string DOG = "dog";
		const string CAT = "cat";
		const string BAT = "bat";
		const string DAY = "day";
		const string BYE = "bye";
		const string DRY = "dry";

		var (Guild1, Invite1) = client.CreateFakeInvite(Time);
		await db.AddInviteAsync(Invite1).CAF();
		AddKeyword(counts, keywords, Guild1, DOG);
		AddKeyword(counts, keywords, Guild1, CAT);
		AddKeyword(counts, keywords, Guild1, BAT);
		var (Guild2, Invite2) = client.CreateFakeInvite(Time);
		await db.AddInviteAsync(Invite2).CAF();
		AddKeyword(counts, keywords, Guild2, DOG);
		AddKeyword(counts, keywords, Guild2, CAT);
		AddKeyword(counts, keywords, Guild2, DAY);
		var (Guild3, Invite3) = client.CreateFakeInvite(Time);
		await db.AddInviteAsync(Invite3).CAF();
		AddKeyword(counts, keywords, Guild3, DOG);
		AddKeyword(counts, keywords, Guild3, BYE);
		AddKeyword(counts, keywords, Guild3, DRY);
		await db.AddKeywordsAsync(keywords).CAF();

		foreach (var kvp in counts)
		{
			var invites = await db.GetInvitesAsync(new[] { kvp.Key }).CAF();
			Assert.AreEqual(kvp.Value.Count, invites.Count);
		}
	}

	private void AddKeyword(Dictionary<string, HashSet<ulong>> counts, List<Keyword> keywords, IGuild guild, string word)
	{
		if (!counts.TryGetValue(word, out var current))
		{
			counts.Add(word, current = []);
		}
		current.Add(guild.Id);
		keywords.Add(new Keyword(guild.Id, word));
	}
}