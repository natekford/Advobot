using Advobot.Gacha.Relationships;
using AdvorangesUtils;
using Discord;
using System;
using System.Threading.Tasks;

namespace Advobot.Gacha.Utilities
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

		public static ulong GetGuildId(this IUserChild child)
			=> ulong.Parse(child.GuildId);
		public static ulong GetUserId(this IUserChild child)
			=> ulong.Parse(child.UserId);
	}
}
