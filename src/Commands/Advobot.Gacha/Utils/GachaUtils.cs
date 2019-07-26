using AdvorangesUtils;
using Discord;
using System;
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
	}
}
