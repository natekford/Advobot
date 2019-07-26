using Advobot.Gacha.Metadata;
using Advobot.Gacha.Relationships;
using AdvorangesUtils;
using Dapper;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Threading.Tasks;

namespace Advobot.Gacha.Utils
{
	public static class GachaDatabaseUtils
	{
		public static async Task<AmountAndRank> GetRankAsync<T>(
			this SQLiteConnection connection,
			string tableName,
			long id)
			where T : ICharacterChild
		{
			var query = await connection.QueryAsync<int>($@"
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

			//Find ones with a higher rank than the wanted one
			var rank = 1;
			var amount = dict.TryGetValue(id, out var val) ? val : 0;
			foreach (var kvp in dict)
			{
				if (kvp.Value > amount)
				{
					++rank;
				}
			}
			return new AmountAndRank(tableName, amount, rank);
		}
	}
}
