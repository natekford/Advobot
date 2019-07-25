using Advobot.Gacha.Metadata;
using Advobot.Gacha.Relationships;
using AdvorangesUtils;
using Discord;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Advobot.Gacha.Utils
{
	public static class GachaUtils
	{
		public static async Task<bool> SafeAddReactionsAsync(this IUserMessage message, params IEmote[] emotes)
		{
			try
			{
				await message.AddReactionsAsync(emotes).CAF();
				return true;
			}
			catch (Exception e)
			{
				throw new NotImplementedException("Not implemented yet.", e);
			}
		}
		public static AmountAndRank GetRankAsync<T>(
			this DbSet<T> children,
			int id,
			string name)
			where T : class, ICharacterChild
		{
			var ids = children.Select(x => x.CharacterId).ToList();
			var grouped = ids.GroupBy(x => x).ToDictionary(x => x.Key, x => x.Count());

			var rank = 1;
			var amount = grouped.TryGetValue(id, out var val) ? val : 0;
			foreach (var kvp in grouped)
			{
				if (kvp.Value > amount)
				{
					++rank;
				}
			}
			return new AmountAndRank(name, amount, rank);
		}
	}
}
