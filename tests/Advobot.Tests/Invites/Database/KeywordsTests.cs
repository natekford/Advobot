using System.Collections.Generic;
using System.Threading.Tasks;

using Advobot.Invites.Models;
using Advobot.Tests.Fakes.Discord;

using AdvorangesUtils;

using Discord;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Advobot.Tests.Invites.Database
{
	[TestClass]
	public sealed class KeywordsTests : DatabaseTestsBase
	{
		[TestMethod]
		public async Task KeywordInviteGathering_Test()
		{
			var db = await GetDatabaseAsync().CAF();

			var client = new FakeClient();
			var counts = new Dictionary<string, int>();
			var keywords = new List<Keyword>();

			const string DOG = "dog";
			const string CAT = "cat";
			const string BAT = "bat";
			const string DAY = "day";
			const string BYE = "bye";
			const string DRY = "dry";

			var (Guild1, Invite1) = CreateFakeInvite(client, Time);
			await db.AddInviteAsync(Invite1).CAF();
			AddKeyword(counts, keywords, Guild1, DOG);
			AddKeyword(counts, keywords, Guild1, CAT);
			AddKeyword(counts, keywords, Guild1, BAT);
			var (Guild2, Invite2) = CreateFakeInvite(client, Time);
			await db.AddInviteAsync(Invite2).CAF();
			AddKeyword(counts, keywords, Guild2, DOG);
			AddKeyword(counts, keywords, Guild2, CAT);
			AddKeyword(counts, keywords, Guild2, DAY);
			var (Guild3, Invite3) = CreateFakeInvite(client, Time);
			await db.AddInviteAsync(Invite3).CAF();
			AddKeyword(counts, keywords, Guild3, DOG);
			AddKeyword(counts, keywords, Guild3, BYE);
			AddKeyword(counts, keywords, Guild3, DRY);
			await db.AddKeywordsAsync(keywords).CAF();

			foreach (var kvp in counts)
			{
				var invites = await db.GetInvitesAsync(new[] { kvp.Key }).CAF();
				Assert.AreEqual(kvp.Value, invites.Count);
			}
		}

		private void AddKeyword(Dictionary<string, int> counts, List<Keyword> keywords, IGuild guild, string word)
		{
			counts.TryGetValue(word, out var current);
			counts[word] = current + 1;
			keywords.Add(new Keyword(guild, word));
		}
	}
}