using Advobot.Gacha.Database;
using Advobot.Gacha.Metadata;
using Advobot.Gacha.Models;
using Advobot.Gacha.Relationships;

using AdvorangesUtils;

using Dapper;

using Discord;

using System.Data.SQLite;

namespace Advobot.Gacha.Utilities;

public static class RankUtils
{
	public static Task<IReadOnlyList<Character>> GetCharactersAsync(this IGachaDatabase db, string input)
	{
		var matches = db.CharacterIds.FindMatches(input);
		return db.GetCharactersAsync(matches.Select(x => x.Value.Id));
	}

	public static int GetRank<T>(IEnumerable<T> values, T amount) where T : IComparable<T>
	{
		//Find ones with a higher rank than the wanted one
		var rank = 1;
		foreach (var value in values)
		{
			if (value.CompareTo(amount) == 1)
			{
				++rank;
			}
		}
		return rank;
	}

	public static async Task<AmountAndRank> GetRankAsync<T>(
		this SQLiteConnection connection,
		string tableName,
		long id,
		DateTimeOffset now)
		where T : ICharacterChild
	{
		var query = await connection.QueryAsync<long>($@"
				SELECT CharacterId
				FROM {tableName}
			").CAF();

		//Find out how many exist for each character
		var dict = new Dictionary<long, int>();
		foreach (var cId in query)
		{
			dict.TryGetValue(cId, out var curr);
			dict[cId] = curr + 1;
		}

		//Normalize by amount per day
		var normalizedDict = new Dictionary<long, double>();
		foreach (var cId in dict.Keys)
		{
			normalizedDict[cId] = Normalize(now, cId.ToTime(), dict[cId]);
		}

		var amount = dict.TryGetValue(id, out var v) ? v : 0;
		var rank = GetRank(dict.Values, amount);
		var normalizedAmount = normalizedDict.TryGetValue(id, out var n) ? n : 0;
		var normalizedRank = GetRank(normalizedDict.Values, normalizedAmount);
		return new(tableName, amount, rank, normalizedAmount, normalizedRank);
	}

	public static Task<IReadOnlyList<Source>> GetSourcesAsync(this IGachaDatabase db, string input)
	{
		var matches = db.SourceIds.FindMatches(input);
		return db.GetSourcesAsync(matches.Select(x => x.Value.Id));
	}

	public static double Normalize(DateTimeOffset now, DateTimeOffset timeCreated, int amount)
		=> amount / (now - timeCreated).TotalDays;

	public static (ulong GuildId, ulong UserId) ToKey(this IGuildUser user)
		=> (user.GuildId, user.Id);
}