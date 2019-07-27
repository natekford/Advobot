using Advobot.Gacha.Metadata;
using Advobot.Gacha.Relationships;
using AdvorangesUtils;
using Dapper;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Threading.Tasks;

namespace Advobot.Gacha.Utils
{
	public static class RankUtils
	{
		private static readonly ConcurrentDictionary<long, Lazy<Task<long>>> _Times
			= new ConcurrentDictionary<long, Lazy<Task<long>>>();

		public static async Task<AmountAndRank> GetRankAsync<T>(
			this SQLiteConnection connection,
			string tableName,
			long id)
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
				var created = await _Times.GetOrAdd(cId, key => new Lazy<Task<long>>(
					connection.QuerySingleAsync<long>(@"
						SELECT TimeCreated
						FROM Character
						WHERE CharacterId = @CharacterId
					", new { CharacterId = key }))
				).Value.CAF();
				normalizedDict[cId] = Normalize(created.ToTime(), dict[cId]);
			}

			var amount = dict.TryGetValue(id, out var v) ? v : 0;
			var rank = GetRank(dict.Values, amount);
			var normalizedAmount = normalizedDict.TryGetValue(id, out var n) ? n : 0;
			var normalizedRank = GetRank(normalizedDict.Values, normalizedAmount);
			return new AmountAndRank(tableName, amount, rank, normalizedAmount, normalizedRank);
		}
		public static double Normalize(DateTime timeCreated, int amount)
			=> amount / (DateTime.UtcNow - timeCreated).TotalDays;
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
	}
}
